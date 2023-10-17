namespace WhatToCook.Application.Infrastructure.Repositories
{
    public interface IFileSaver
    {
        Task SaveAsync(string path, byte[] data);
    }
    public class FileSaver : IFileSaver
    {
        public async Task SaveAsync(string path, byte[] data)
        {
           await System.IO.File.WriteAllBytesAsync(path, data);
        }
    }
}
