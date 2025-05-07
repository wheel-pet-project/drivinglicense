using Infrastructure.Adapters.ImageValidators;
using Xunit;

namespace UnitTests.Infrastructure.ImageValidators;

public class ImageValidatorShould
{
    private readonly List<byte> _jpegBytes = [255, 216, 255, 224];
    private readonly List<byte> _pngBytes = [137, 80, 78, 71, 13, 10, 26, 10];

    [Fact]
    public void ReturnTrueForJpegFormat()
    {
        // Arrange
        var validator = new ImageValidator();

        // Act
        var actual = validator.IsSupportedFormat(_jpegBytes);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void ReturnFalseForPngFormat()
    {
        // Arrange
        var validator = new ImageValidator();

        // Act
        var actual = validator.IsSupportedFormat(_pngBytes);

        // Assert
        Assert.False(actual);
    }
    
    [Fact]
    public void ReturnTrueIfImageSizeIsLessThan1Mb()
    {
        // Arrange
        var size = 999_999;
        var validator = new ImageValidator();

        // Act
        var actual = validator.IsSupportedSize(size);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void ReturnFalseIfImageSizeIsLargerThan1Mb()
    {
        // Arrange
        var size = 1_000_001;
        var validator = new ImageValidator();

        // Act
        var actual = validator.IsSupportedSize(size);

        // Assert
        Assert.False(actual);
    }
}