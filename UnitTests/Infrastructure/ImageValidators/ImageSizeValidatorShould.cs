using Infrastructure.Adapters.ImageValidators;
using JetBrains.Annotations;
using Xunit;

namespace UnitTests.Infrastructure.ImageValidators;

[TestSubject(typeof(ImageSizeValidator))]
public class ImageSizeValidatorShould
{
    [Fact]
    public void ReturnTrueIfImageSizeIsLessThan1Mb()
    {
        // Arrange
        var size = 999_999;
        var validator = new ImageSizeValidator();

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
        var validator = new ImageSizeValidator();

        // Act
        var actual = validator.IsSupportedSize(size);

        // Assert
        Assert.False(actual);
    }
}