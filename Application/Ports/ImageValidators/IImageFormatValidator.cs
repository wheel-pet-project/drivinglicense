namespace Application.Ports.ImageValidators;

public interface IImageFormatValidator
{
    bool IsSupportedFormat(List<byte> imageBytes);
}