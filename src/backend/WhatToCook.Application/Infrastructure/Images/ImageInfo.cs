namespace WhatToCook.Application.Infrastructure.Images;

public record ImageInfo
{
    public string Base64Image { get; private set; }
    public string FileNameWithoutExtension { get; private set; }
    public string FileExtension { get; private set; }

    public ImageInfo(string base64Image, string fileNameWithoutExtension)
    {
        Base64Image = base64Image;
        FileNameWithoutExtension = fileNameWithoutExtension;
        FileExtension = DetermineImageExtension();
    }
    public byte[] GetImageBytes() => Convert.FromBase64String(Base64Image);
    public string GetFileName() => $"{FileNameWithoutExtension}{FileExtension}";
    private string DetermineImageExtension()
    {
        byte[] bytes = GetImageBytes();
        return bytes.Take(3).SequenceEqual(new byte[] { 0xFF, 0xD8, 0xFF })
            ? ".jpg"
            : bytes.Take(8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })
            ? ".png"
            : bytes.Take(2).SequenceEqual(new byte[] { 66, 77 })
            ? ".bmp"
            : bytes.Take(6).SequenceEqual(new byte[] { 71, 73, 70, 56, 55, 97 }) ||
            bytes.Take(6).SequenceEqual(new byte[] { 71, 73, 70, 56, 57, 97 })
            ? ".gif"
            : throw new Exception("Unknown or unsupported image format.");
    }
}
