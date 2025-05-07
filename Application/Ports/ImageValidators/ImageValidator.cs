namespace Application.Ports.ImageValidators;

public interface IImageValidator
{
    bool IsSupportedFormat(List<byte> imageBytes);
    
    bool IsSupportedSize(int imageSize);
}