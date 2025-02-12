using Infrastructure.Adapters.ImageValidators;
using JetBrains.Annotations;
using Xunit;

namespace UnitTests.Infrastructure.ImageValidators;

[TestSubject(typeof(ImageFormatValidator))]
public class ImageFormatValidatorShould
{
    private readonly List<byte> _jpegBytes = [255, 216, 255, 224];
    private readonly List<byte> _pngBytes = [137, 80, 78, 71, 13, 10, 26, 10];

    [Fact]
    public void ReturnTrueForJpegFormat()
    {
        // Arrange
        var validator = new ImageFormatValidator();

        // Act
        var actual = validator.IsSupportedFormat(_jpegBytes);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void ReturnFalseForPngFormat()
    {
        // Arrange
        var validator = new ImageFormatValidator();

        // Act
        var actual = validator.IsSupportedFormat(_pngBytes);

        // Assert
        Assert.False(actual);
    }
}