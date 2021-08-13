﻿using Moq;
using ShenzhenIO.Emulator.Core.Execution;
using ShenzhenIO.Emulator.Core.IO;
using ShenzhenIO.Emulator.Implementation.Execution;
using ShenzhenIO.Emulator.Tests.Extensions;
using Xunit;

namespace ShenzhenIO.Emulator.Tests.Execution
{
    public class TcpCommandTests
    {
        [Theory]
        [InlineData(0, 0, TestResult.None)]
        [InlineData(100, 0, TestResult.Success)]
        [InlineData(0, 100, TestResult.Failure)]
        [InlineData(100, 100, TestResult.None)]
        [InlineData(100, -100, TestResult.Success)]
        [InlineData(-100, -99, TestResult.Failure)]
        public void TestComparingValuesWithoutBlocks(int firstValue, int secondValue, TestResult expectedTestResult)
        {
            // Arrange

            var firstValueSourceMock = new Mock<IReadable>();
            firstValueSourceMock.Setup(x => x.TryRead(out firstValue)).Returns(true);

            var secondValueSourceMock = new Mock<IReadable>();
            secondValueSourceMock.Setup(x => x.TryRead(out secondValue)).Returns(true);

            var firstValueSource = firstValueSourceMock.Object;
            var secondValueSource = secondValueSourceMock.Object;
            var command = new TcpCommand(firstValueSource, secondValueSource);

            // Act
            var result = command.Execute();

            // Assert
            result.ShouldSetTestResult(expectedTestResult, "when both input values are unlocked");
        }

        [Fact]
        public void TestExecutionWithBlocks()
        {
            // Arrange

            var firstValueSourceMock = new Mock<IReadable>();
            var first = 0;
            firstValueSourceMock.Setup(x => x.TryRead(out first)).Returns(false);

            var secondValueSourceMock = new Mock<IReadable>();
            var second = 0;
            secondValueSourceMock.Setup(x => x.TryRead(out second)).Returns(false);

            var firstValueSource = firstValueSourceMock.Object;
            var secondValueSource = secondValueSourceMock.Object;
            var command = new TcpCommand(firstValueSource, secondValueSource);

            // Series of acts and asserts

            var firstResult = command.Execute();
            firstResult.ShouldCauseABlock("on first value read", "when both values are locked");

            var secondResult = command.Execute();
            secondResult.ShouldCauseABlock("on first value read", "when both values are still locked");

            first = 100;
            firstValueSourceMock.Setup(x => x.TryRead(out first)).Returns(true);

            var thirdResult = command.Execute();
            thirdResult.ShouldCauseABlock("on second value read", "after unlocking the first value");

            second = 100;
            secondValueSourceMock.Setup(x => x.TryRead(out second)).Returns(true);

            var fourthResult = command.Execute();
            fourthResult.ShouldSetTestResult(TestResult.None, "after unlocking both values");

            first = 0;
            firstValueSourceMock.Setup(x => x.TryRead(out first)).Returns(true);

            var fifthResult = command.Execute();
            fifthResult.ShouldSetTestResult(TestResult.Failure, "after changing the first value");

            firstValueSourceMock.Setup(x => x.TryRead(out first)).Returns(false);

            var sixthResult = command.Execute();
            sixthResult.ShouldCauseABlock("on first value read", "after exhausting the first value");
        }
    }
}