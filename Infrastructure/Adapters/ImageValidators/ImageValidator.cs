using Application.Ports.ImageValidators;

namespace Infrastructure.Adapters.ImageValidators;

public class ImageValidator : IImageValidator
{
    private readonly List<byte> _jpegFormat = [255, 216, 255, 224];
    private const int MegabyteSize = 1_000_000;

    public bool IsSupportedFormat(List<byte> imageBytes)
    {
        return _jpegFormat.SequenceEqual(imageBytes[.._jpegFormat.Count]);
    }

    public bool IsSupportedSize(int imageSize)
    {
        return imageSize <= MegabyteSize;
    }
}