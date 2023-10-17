namespace WhatToCook.Application.Domain
{
    public class ImageInfo
    {
        public string Base64Image { get; private set; }
        public string FileNameWithoutExtension { get; private set; }
        public string ImagesDirectory { get; private set; }
        public string FileExtension { get; private set; }

        public ImageInfo(string base64Image, string fileNameWithoutExtension, string imagesDirectory)
        {
            Base64Image = base64Image;
            FileNameWithoutExtension = fileNameWithoutExtension;
            ImagesDirectory = imagesDirectory;

            FileExtension = DetermineImageExtension();
        }
        public byte[] GetImageBytes()
        {
            return Convert.FromBase64String(Base64Image);
        }
        private string DetermineImageExtension()
        {
            byte[] bytes = GetImageBytes();
            if (bytes.Take(3).SequenceEqual(new byte[] { 0xFF, 0xD8, 0xFF }))
            {
                return ".jpg";
            }

            if (bytes.Take(8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }))
            {
                return ".png";
            }

            if (bytes.Take(2).SequenceEqual(new byte[] { 66, 77 }))
            {
                return ".bmp";
            }

            if (bytes.Take(6).SequenceEqual(new byte[] { 71, 73, 70, 56, 55, 97 }) ||
                bytes.Take(6).SequenceEqual(new byte[] { 71, 73, 70, 56, 57, 97 }))
            {
                return ".gif";
            }

            throw new Exception("Unknown or unsupported image format.");
        }
    }
}
