namespace WhatToCook.Web.Components.Features.Planning.Components;

public enum MealSlot
{
    Breakfast,
    SecondBreakfast,
    Dinner,
    Supper,
}

public sealed record MealSlotDefinition(MealSlot Slot, string Label, string TimeRange, string Icon);

public static class MealSlotOrdering
{
    private const int SlotRange = 1000;

    public static IReadOnlyList<MealSlotDefinition> Slots { get; } =
    [
        new(MealSlot.Breakfast, "Śniadanie", "06:00-09:00", "☀"),
        new(MealSlot.SecondBreakfast, "II śniadanie", "10:00-12:00", "●"),
        new(MealSlot.Dinner, "Obiad", "13:00-16:00", "♨"),
        new(MealSlot.Supper, "Kolacja", "18:00-21:00", "☾"),
    ];

    public static MealSlot GetSlot(int sortOrder)
    {
        if (sortOrder < SlotRange)
        {
            return MealSlot.Dinner;
        }

        return (sortOrder / SlotRange) switch
        {
            1 => MealSlot.Breakfast,
            2 => MealSlot.SecondBreakfast,
            3 => MealSlot.Dinner,
            4 => MealSlot.Supper,
            _ => MealSlot.Dinner,
        };
    }

    public static int Encode(MealSlot slot, int position) =>
        ((int)slot + 1) * SlotRange + Math.Clamp(position, 0, SlotRange - 1);

    public static MealSlotDefinition GetDefinition(MealSlot slot) =>
        Slots.Single(x => x.Slot == slot);
}
