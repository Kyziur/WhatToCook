namespace WhatToCook.Application.Infrastructure.Images;

public interface IFileSaver
{
    Task SaveAsync(string path, byte[] data);
}
public class FileSaver : IFileSaver
{
    public async Task SaveAsync(string path, byte[] data) => await File.WriteAllBytesAsync(path, data);
}
