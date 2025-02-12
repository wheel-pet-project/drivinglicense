using Application.Ports.ImageValidators;

namespace Infrastructure.Adapters.ImageValidators;

public class ImageSizeValidator : IImageSizeValidator
{
    private readonly int _megabyteSize = 1_000_000;

    public bool IsSupportedSize(int imageSize)
    {
        return imageSize <= _megabyteSize;
    }
}