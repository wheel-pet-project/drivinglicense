namespace Application.Ports.ImageValidators;

public interface IImageSizeValidator
{
    bool IsSupportedSize(int imageSize);
}