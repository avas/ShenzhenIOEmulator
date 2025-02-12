using FluentAssertions;
using ShenzhenIO.Emulator.Implementation.IO;
using Xunit;

namespace ShenzhenIO.Emulator.Tests.IO
{
    public class IntegerLiteralFactoryTests
    {
        [Theory]
        [InlineData("-1000", false, 0, "Value too small: -1000")]
        [InlineData("-999", true, -999, null)]
        [InlineData("-100", true, -100, null)]
        [InlineData("0", true, 0, null)]
        [InlineData("11", true, 11, null)]
        [InlineData("011", true, 11, null)]
        [InlineData("100", true, 100, null)]
        [InlineData("1000", false, 0, "Value too large: 1000")]
        public void TestCreatingLiterals(string argument, bool expectedResult, int expectedValue, string? expectedErrorMessage)
        {
            // Arrange
            var factory = new IntegerLiteralFactory();

            // Act
            var actualResult = factory.TryCreateReadable(argument, out var readable, out var actualErrorMessage);

            // Assert
            actualResult.Should().Be(expectedResult);
            actualErrorMessage.Should().Be(expectedErrorMessage);

            if (!expectedResult)
            {
                readable.Should().BeNull();
            }
            else
            {
                readable.Should().NotBeNull();
                
                var readResult = readable.TryRead(out var actualValue);

                readResult.Should().BeTrue();
                actualValue.Should().Be(expectedValue);
            }
        }
    }
}