using Application.Ports.ImageValidators;

namespace Infrastructure.Adapters.ImageValidators;

public class ImageFormatValidator : IImageFormatValidator
{
    private readonly List<byte> _jpegFormat = [255, 216, 255, 224];

    public bool IsSupportedFormat(List<byte> imageBytes)
    {
        return _jpegFormat.SequenceEqual(imageBytes[.._jpegFormat.Count]);
    }
}