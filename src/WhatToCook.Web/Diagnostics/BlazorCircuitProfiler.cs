using System.Diagnostics.Metrics;
using System.Text.Json;
using Microsoft.Extensions.Hosting;

namespace WhatToCook.Web.Diagnostics;

/// <summary>
/// Local, opt-in PERF-05 probe. Never registered outside Development; it writes
/// aggregate process metrics to a local file and exposes no diagnostic endpoint.
/// </summary>
internal sealed class BlazorCircuitProfiler(string outputPath, string phasePath) : BackgroundService
{
    private const int MaxRenderSamples = 100_000;
    private const string BlazorMeterPrefix = "Microsoft.AspNetCore.Components";
    private const string CircuitMeter = "Microsoft.AspNetCore.Components.Server.Circuits";
    private const string LifecycleMeter = "Microsoft.AspNetCore.Components.Lifecycle";
    private readonly MeterListener listener = new();
    private readonly object sync = new();
    private readonly List<double> renderSamples = [];
    private readonly HashSet<string> knownInstruments = new(StringComparer.Ordinal);
    private long activeCircuits;
    private long connectedCircuits;
    private string phase = "setup";
    private long allocatedAtPhaseStart;
    private int gen0AtPhaseStart;
    private int gen1AtPhaseStart;
    private int gen2AtPhaseStart;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath))!);
        allocatedAtPhaseStart = GC.GetTotalAllocatedBytes(precise: false);
        gen0AtPhaseStart = GC.CollectionCount(0);
        gen1AtPhaseStart = GC.CollectionCount(1);
        gen2AtPhaseStart = GC.CollectionCount(2);

        listener.InstrumentPublished = (instrument, listener) =>
        {
            if (instrument.Meter.Name.StartsWith(BlazorMeterPrefix, StringComparison.Ordinal))
            {
                lock (sync)
                {
                    var instrumentType = instrument.GetType().Name;
                    var identifier = $"{instrument.Meter.Name}/{instrument.Name}/{instrumentType}";
                    knownInstruments.Add(identifier);
                }
            }

            if (ShouldObserve(instrument))
            {
                listener.EnableMeasurementEvents(instrument);
            }
        };
        listener.SetMeasurementEventCallback<long>(
            (instrument, value, _, _) =>
            {
                if (instrument.Name == "aspnetcore.components.circuit.active")
                {
                    Interlocked.Add(ref activeCircuits, value);
                }
                else if (instrument.Name == "aspnetcore.components.circuit.connected")
                {
                    Interlocked.Add(ref connectedCircuits, value);
                }
                else if (IsRenderDuration(instrument.Name))
                {
                    RecordRender(value);
                }
            }
        );
        listener.SetMeasurementEventCallback<double>(
            (instrument, seconds, _, _) =>
            {
                if (!IsRenderDuration(instrument.Name))
                {
                    return;
                }

                RecordRender(seconds);
            }
        );
        listener.SetMeasurementEventCallback<int>(
            (instrument, value, _, _) =>
            {
                if (IsRenderDuration(instrument.Name))
                {
                    RecordRender(value);
                }
            }
        );
        listener.Start();

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                var requestedPhase = ReadPhase();
                double[] values;
                string currentPhase;
                lock (sync)
                {
                    if (requestedPhase != phase)
                    {
                        phase = requestedPhase;
                        renderSamples.Clear();
                        allocatedAtPhaseStart = GC.GetTotalAllocatedBytes(precise: false);
                        gen0AtPhaseStart = GC.CollectionCount(0);
                        gen1AtPhaseStart = GC.CollectionCount(1);
                        gen2AtPhaseStart = GC.CollectionCount(2);
                    }

                    currentPhase = phase;
                    values = renderSamples.Order().ToArray();
                }

                string[] discovered;
                lock (sync)
                {
                    discovered = knownInstruments.Order().ToArray();
                }

                var snapshot = new
                {
                    Phase = currentPhase,
                    TimestampUtc = DateTimeOffset.UtcNow,
                    ActiveCircuits = Interlocked.Read(ref activeCircuits),
                    ConnectedCircuits = Interlocked.Read(ref connectedCircuits),
                    WorkingSetBytes = Environment.WorkingSet,
                    AllocatedBytes = GC.GetTotalAllocatedBytes(precise: false)
                        - allocatedAtPhaseStart,
                    Gen0Collections = GC.CollectionCount(0) - gen0AtPhaseStart,
                    Gen1Collections = GC.CollectionCount(1) - gen1AtPhaseStart,
                    Gen2Collections = GC.CollectionCount(2) - gen2AtPhaseStart,
                    RenderSamples = values.Length,
                    RenderP50Ms = Percentile(values, 0.50),
                    RenderP95Ms = Percentile(values, 0.95),
                    RenderSamplesCapped = values.Length >= MaxRenderSamples,
                    PublishedInstruments = discovered,
                };
                await File.AppendAllTextAsync(
                    outputPath,
                    JsonSerializer.Serialize(snapshot) + Environment.NewLine,
                    stoppingToken
                );
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Normal teardown after the Playwright run.
        }
    }

    private void RecordRender(double seconds)
    {
        lock (sync)
        {
            if (renderSamples.Count < MaxRenderSamples)
            {
                renderSamples.Add(seconds * 1000);
            }
        }
    }

    private static bool IsRenderDuration(string name)
    {
        return name == "aspnetcore.components.render_diff"
            || name == "aspnetcore.components.render_diff.duration";
    }

    private static bool ShouldObserve(Instrument instrument)
    {
        if (instrument.Meter.Name == CircuitMeter)
        {
            return instrument.Name == "aspnetcore.components.circuit.active"
                || instrument.Name == "aspnetcore.components.circuit.connected";
        }

        return instrument.Meter.Name == LifecycleMeter && IsRenderDuration(instrument.Name);
    }

    private string ReadPhase()
    {
        try
        {
            return File.Exists(phasePath) ? File.ReadAllText(phasePath).Trim() : "setup";
        }
        catch (IOException)
        {
            return phase;
        }
    }

    private static double? Percentile(double[] values, double quantile)
    {
        if (values.Length == 0)
        {
            return null;
        }

        return values[(int)Math.Ceiling(quantile * values.Length) - 1];
    }

    public override void Dispose()
    {
        listener.Dispose();
        base.Dispose();
    }
}
