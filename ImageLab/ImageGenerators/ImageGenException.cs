namespace ImageLab.Services;

public class ImageGenException : Exception
{
    public ImageGenException(string? message) : base(message) { }
    public ImageGenException(string? message, Exception? innerException) : base(message, innerException) {}
}