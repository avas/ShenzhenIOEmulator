using FluentAssertions;
using Moq;
using ShenzhenIO.Emulator.Core.IO;
using ShenzhenIO.Emulator.Core.Language;
using ShenzhenIO.Emulator.Implementation.Language;
using System.Collections.Generic;
using Xunit;

namespace ShenzhenIO.Emulator.Tests.Language
{
    public class CommandParameterResolverTests
    {
        private static readonly IRegister _acc = Mock.Of<IRegister>();
        private static readonly IRegister _dat = Mock.Of<IRegister>();
        private static readonly IRegister _null = Mock.Of<IRegister>();

        private static readonly IAnalogPort _p0 = Mock.Of<IAnalogPort>();
        private static readonly IAnalogPort _p1 = Mock.Of<IAnalogPort>();

        private static readonly IXBusPort _x0 = Mock.Of<IXBusPort>();
        private static readonly IXBusPort _x1 = Mock.Of<IXBusPort>();
        private static readonly IXBusPort _x2 = Mock.Of<IXBusPort>();
        private static readonly IXBusPort _x3 = Mock.Of<IXBusPort>();

        public static object[][] ReadableResolvingTestCases =
        {
            new object[] { BuildContextForMC4000(), "dat", false, null, "Invalid or unavailable register: dat" },
            new object[] { BuildContextForMC4000(), "x0", true, _x0, null },
            new object[] { BuildContextForMC4000(), "x1", true, _x1, null },
            new object[] { BuildContextForMC4000(), "x2", false, null, "Invalid or unavailable register: x2" },
            new object[] { BuildContextForMC4000(), "x3", false, null, "Invalid or unavailable register: x3" },
            new object[] { BuildContextForMC4000(), "foo", false, null, "Invalid or unavailable register: foo" },

            new object[] { BuildContextForMC4000X(), "p0", false, null, "Invalid or unavailable register: p0" },
            new object[] { BuildContextForMC4000X(), "p1", false, null, "Invalid or unavailable register: p1" },
            new object[] { BuildContextForMC4000X(), "x0", true, _x0, null },
            new object[] { BuildContextForMC4000X(), "x1", true, _x1, null },
            new object[] { BuildContextForMC4000X(), "x2", true, _x2, null },
            new object[] { BuildContextForMC4000X(), "x3", true, _x3, null },

            new object[] { BuildContextForMC6000(), "x0", true, _x0, null },
            new object[] { BuildContextForMC6000(), "x1", true, _x1, null },
            new object[] { BuildContextForMC6000(), "x2", true, _x2, null },
            new object[] { BuildContextForMC6000(), "x3", true, _x3, null },
        };

        [Theory]
        [MemberData(nameof(ReadableResolvingTestCases))]
        public void TestResolvingXBusPortsAsReadables(CommandFactoryContext context, string argument, bool expectedResult, IReadable expectedReadable, string expectedErrorMessage)
        {
            // Arrange
            var testContext = BuildCommandParameterResolver();
            var resolver = testContext.CommandParameterResolver;

            // Act
            var actualResult = resolver.TryGetReadable(argument, context, out var actualReadable, out var actualErrorMessage);

            // Assert
            actualResult.Should().Be(expectedResult);
            actualReadable.Should().BeSameAs(expectedReadable);
            actualErrorMessage.Should().Be(expectedErrorMessage);

            testContext.ReadableWrapperFactoryMock.Verify(x => x.Wrap(It.IsAny<ISyncReadable>()), Times.Never());
        }

        public static object[][] WrappedReadableResolvingTestCases =
        {
            new object[] { BuildContextForMC4000(), "acc", true, _acc, null },
            new object[] { BuildContextForMC4000(), "null", true, _null, null },
            new object[] { BuildContextForMC4000(), "p0", true, _p0, null },
            new object[] { BuildContextForMC4000(), "p1", true, _p1, null },
            new object[] { BuildContextForMC6000(), "dat", true, _dat, null },
            new object[] { BuildContextForMC6000(), "p0", true, _p0, null },
            new object[] { BuildContextForMC6000(), "p1", true, _p1, null },
        };

        [Theory]
        [MemberData(nameof(WrappedReadableResolvingTestCases))]
        public void TestResolvingRegistersAndAnalogPortsAsReadables(CommandFactoryContext context, string argument, bool expectedResult, ISyncReadable expectedWrappedReadable, string expectedErrorMessage)
        {
            // Arrange
            var testContext = BuildCommandParameterResolver();

            var wrappedReadable = Mock.Of<IReadable>();
            testContext.ReadableWrapperFactoryMock.Setup(x => x.Wrap(expectedWrappedReadable)).Returns(wrappedReadable);

            var resolver = testContext.CommandParameterResolver;

            // Act
            var actualResult = resolver.TryGetReadable(argument, context, out var actualReadable, out var actualErrorMessage);

            // Assert
            actualResult.Should().Be(expectedResult);
            actualReadable.Should().BeSameAs(wrappedReadable);
            actualErrorMessage.Should().Be(expectedErrorMessage);

            testContext.ReadableWrapperFactoryMock.Verify(x => x.Wrap(It.IsAny<ISyncReadable>()), Times.Once());
        }

        public static object[][] LiteralResolvingTestCases =
        {
            new object[] { BuildContextForMC4000(), "-999" },
            new object[] { BuildContextForMC4000(), "0" },
            new object[] { BuildContextForMC4000(), "000" },
            new object[] { BuildContextForMC4000(), "011" },
            new object[] { BuildContextForMC4000(), "100" },
            new object[] { BuildContextForMC4000(), "999" },
        };

        [Theory]
        [MemberData(nameof(LiteralResolvingTestCases))]
        public void TestSuccessfullyResolvingLiterals(CommandFactoryContext context, string argument)
        {
            // Arrange
            var testContext = BuildCommandParameterResolver();

            var readable = Mock.Of<IReadable>();
            string errorMessage = null;
            testContext.IntegerLiteralFactoryMock.Setup(x => x.TryCreateReadable(argument, out readable, out errorMessage)).Returns(true);

            var resolver = testContext.CommandParameterResolver;

            // Act
            var actualResult = resolver.TryGetReadable(argument, context, out var actualReadable, out var actualErrorMessage);

            // Assert
            actualResult.Should().BeTrue();
            actualReadable.Should().BeSameAs(readable);
            actualErrorMessage.Should().BeNull();
        }

        public static object[][] FailedLiteralResolvingTestCases =
        {
            new object[] { BuildContextForMC4000(), "-1000", "Value too small: -1000" },
            new object[] { BuildContextForMC4000(), "1000", "Value too large: 1000" },
            new object[] { BuildContextForMC4000(), "0", "Using 0 today is not a good idea" },
        };

        [Theory]
        [MemberData(nameof(FailedLiteralResolvingTestCases))]
        public void TestFailingToResolveLiterals(CommandFactoryContext context, string argument, string expectedErrorMessage)
        {
            // Arrange
            var testContext = BuildCommandParameterResolver();

            IReadable readable = null;
            testContext.IntegerLiteralFactoryMock.Setup(x => x.TryCreateReadable(argument, out readable, out expectedErrorMessage)).Returns(false);

            var resolver = testContext.CommandParameterResolver;

            // Act
            var actualResult = resolver.TryGetReadable(argument, context, out var actualReadable, out var actualErrorMessage);

            // Assert
            actualResult.Should().BeFalse();
            actualReadable.Should().BeNull();
            actualErrorMessage.Should().Be(expectedErrorMessage);

        }

        public static object[][] WritableResolvingTestCases =
        {
            new object[] { BuildContextForMC4000(), "dat", false, null, "Invalid or unavailable register: dat" },
            new object[] { BuildContextForMC4000(), "x0", true, _x0, null },
            new object[] { BuildContextForMC4000(), "x1", true, _x1, null },
            new object[] { BuildContextForMC4000(), "x2", false, null, "Invalid or unavailable register: x2" },
            new object[] { BuildContextForMC4000(), "x3", false, null, "Invalid or unavailable register: x3" },
            new object[] { BuildContextForMC4000(), "-999", false, null, "Invalid or unavailable register: -999" },
            new object[] { BuildContextForMC4000(), "0", false, null, "Invalid or unavailable register: 0" },
            new object[] { BuildContextForMC4000(), "100", false, null, "Invalid or unavailable register: 100" },
            new object[] { BuildContextForMC4000(), "foo", false, null, "Invalid or unavailable register: foo" },

            new object[] { BuildContextForMC4000X(), "dat", false, null, "Invalid or unavailable register: dat" },
            new object[] { BuildContextForMC4000X(), "p0", false, null, "Invalid or unavailable register: p0" },
            new object[] { BuildContextForMC4000X(), "p1", false, null, "Invalid or unavailable register: p1" },
            new object[] { BuildContextForMC4000X(), "x0", true, _x0, null },
            new object[] { BuildContextForMC4000X(), "x1", true, _x1, null },
            new object[] { BuildContextForMC4000X(), "x2", true, _x2, null },
            new object[] { BuildContextForMC4000X(), "x3", true, _x3, null },

            new object[] { BuildContextForMC6000(), "x0", true, _x0, null },
            new object[] { BuildContextForMC6000(), "x1", true, _x1, null },
            new object[] { BuildContextForMC6000(), "x2", true, _x2, null },
            new object[] { BuildContextForMC6000(), "x3", true, _x3, null },
        };

        [Theory]
        [MemberData(nameof(WritableResolvingTestCases))]
        public void TestResolvingWritables(CommandFactoryContext context, string argument, bool expectedResult, IWritable expectedWritable, string expectedErrorMessage)
        {
            // Arrange
            var testContext = BuildCommandParameterResolver();
            var resolver = testContext.CommandParameterResolver;

            // Act
            var actualResult = resolver.TryGetWritable(argument, context, out var actualWritable, out var actualErrorMessage);

            // Assert
            actualResult.Should().Be(expectedResult);
            actualWritable.Should().BeSameAs(expectedWritable);
            actualErrorMessage.Should().Be(expectedErrorMessage);

            testContext.WritableWrapperFactoryMock.Verify(x => x.Wrap(It.IsAny<ISyncWritable>()), Times.Never());
        }

        public static object[][] WrappedWritableResolvingTestCases =
        {
            new object[] { BuildContextForMC4000(), "acc", true, _acc, null },
            new object[] { BuildContextForMC4000(), "null", true, _null, null },
            new object[] { BuildContextForMC4000(), "p0", true, _p0, null },
            new object[] { BuildContextForMC4000(), "p1", true, _p1, null },
            new object[] { BuildContextForMC6000(), "dat", true, _dat, null },
            new object[] { BuildContextForMC6000(), "p0", true, _p0, null },
            new object[] { BuildContextForMC6000(), "p1", true, _p1, null },
        };

        [Theory]
        [MemberData(nameof(WrappedWritableResolvingTestCases))]
        public void TestResolvingRegistersAndAnalogPortsAsWritables(CommandFactoryContext context, string argument, bool expectedResult, ISyncWritable expectedWrappedWritable, string expectedErrorMessage)
        {
            // Arrange
            var testContext = BuildCommandParameterResolver();

            var writable = Mock.Of<IWritable>();
            testContext.WritableWrapperFactoryMock.Setup(x => x.Wrap(expectedWrappedWritable)).Returns(writable);

            var resolver = testContext.CommandParameterResolver;

            // Act
            var actualResult = resolver.TryGetWritable(argument, context, out var actualWritable, out var actualErrorMessage);

            // Assert
            actualResult.Should().Be(expectedResult);
            actualWritable.Should().BeSameAs(writable);
            actualErrorMessage.Should().Be(expectedErrorMessage);

            testContext.WritableWrapperFactoryMock.Verify(x => x.Wrap(It.IsAny<ISyncWritable>()), Times.Once());
        }

        public static object[][] AnalogPortResolvingTestCases =
        {
            new object[] { BuildContextForMC4000(), "acc", false, null, "Invalid or unavailable analog port: acc" },
            new object[] { BuildContextForMC4000(), "null", false, null, "Invalid or unavailable analog port: null" },
            new object[] { BuildContextForMC4000(), "p0", true, _p0, null },
            new object[] { BuildContextForMC4000(), "p1", true, _p1, null },
            new object[] { BuildContextForMC4000(), "x0", false, null, "Invalid or unavailable analog port: x0" },
            new object[] { BuildContextForMC4000(), "-999", false, null, "Invalid or unavailable analog port: -999" },
            new object[] { BuildContextForMC4000(), "0", false, null, "Invalid or unavailable analog port: 0" },
            new object[] { BuildContextForMC4000(), "100", false, null, "Invalid or unavailable analog port: 100" },

            new object[] { BuildContextForMC4000X(), "p0", false, null, "Invalid or unavailable analog port: p0" },
            new object[] { BuildContextForMC4000X(), "p1", false, null, "Invalid or unavailable analog port: p1" },

            new object[] { BuildContextForMC6000(), "dat", false, null, "Invalid or unavailable analog port: dat" },
            new object[] { BuildContextForMC6000(), "p0", true, _p0, null },
            new object[] { BuildContextForMC6000(), "p1", true, _p1, null },
            new object[] { BuildContextForMC6000(), "x3", false, null, "Invalid or unavailable analog port: x3" },
        };

        [Theory]
        [MemberData(nameof(AnalogPortResolvingTestCases))]
        public void TestResolvingAnalogPorts(CommandFactoryContext context, string argument, bool expectedResult, IAnalogPort expectedAnalogPort, string expectedErrorMessage)
        {
            // Arrange
            var testContext = BuildCommandParameterResolver();
            var resolver = testContext.CommandParameterResolver;

            // Act
            var actualResult = resolver.TryGetAnalogPort(argument, context, out var actualAnalogPort, out var actualErrorMessage);

            // Assert
            actualResult.Should().Be(expectedResult);
            actualAnalogPort.Should().BeSameAs(expectedAnalogPort);
            actualErrorMessage.Should().Be(expectedErrorMessage);
        }

        public static object[][] XBusPortResolvingTestCases =
        {
            new object[] { BuildContextForMC4000(), "acc", false, null, "Invalid or unavailable XBus port: acc" },
            new object[] { BuildContextForMC4000(), "null", false, null, "Invalid or unavailable XBus port: null" },
            new object[] { BuildContextForMC4000(), "p0", false, null, "Invalid or unavailable XBus port: p0" },
            new object[] { BuildContextForMC4000(), "p1", false, null, "Invalid or unavailable XBus port: p1" },
            new object[] { BuildContextForMC4000(), "x0", true, _x0, null },
            new object[] { BuildContextForMC4000(), "x1", true, _x1, null },
            new object[] { BuildContextForMC4000(), "x2", false, null, "Invalid or unavailable XBus port: x2" },
            new object[] { BuildContextForMC4000(), "x3", false, null, "Invalid or unavailable XBus port: x3" },
            new object[] { BuildContextForMC4000(), "-999", false, null, "Invalid or unavailable XBus port: -999" },
            new object[] { BuildContextForMC4000(), "0", false, null, "Invalid or unavailable XBus port: 0" },
            new object[] { BuildContextForMC4000(), "100", false, null, "Invalid or unavailable XBus port: 100" },

            new object[] { BuildContextForMC4000X(), "dat", false, null, "Invalid or unavailable XBus port: dat" },
            new object[] { BuildContextForMC4000X(), "x0", true, _x0, null },
            new object[] { BuildContextForMC4000X(), "x1", true, _x1, null },
            new object[] { BuildContextForMC4000X(), "x2", true, _x2, null },
            new object[] { BuildContextForMC4000X(), "x3", true, _x3, null },

            new object[] { BuildContextForMC6000(), "dat", false, null, "Invalid or unavailable XBus port: dat" },
            new object[] { BuildContextForMC6000(), "x0", true, _x0, null },
            new object[] { BuildContextForMC6000(), "x1", true, _x1, null },
            new object[] { BuildContextForMC6000(), "x2", true, _x2, null },
            new object[] { BuildContextForMC6000(), "x3", true, _x3, null },
        };

        [Theory]
        [MemberData(nameof(XBusPortResolvingTestCases))]
        public void TestResolvingXBusPorts(CommandFactoryContext context, string argument, bool expectedResult, IXBusPort expectedXBusPort, string expectedErrorMessage)
        {
            // Arrange
            var testContext = BuildCommandParameterResolver();
            var resolver = testContext.CommandParameterResolver;

            // Act
            var actualResult = resolver.TryGetXBusPort(argument, context, out var actualXBusPort, out var actualErrorMessage);

            // Assert
            actualResult.Should().Be(expectedResult);
            actualXBusPort.Should().BeSameAs(expectedXBusPort);
            actualErrorMessage.Should().BeEquivalentTo(expectedErrorMessage);
        }


        private static CommandFactoryContext BuildContextForMC4000()
        {
            var result = BuildGenericContext();

            result.AnalogPorts.Add("p0", _p0);
            result.AnalogPorts.Add("p1", _p1);

            return result;
        }

        private static CommandFactoryContext BuildContextForMC4000X()
        {
            var result = BuildGenericContext();

            result.XBusPorts.Add("x2", _x2);
            result.XBusPorts.Add("x3", _x3);

            return result;
        }

        private static CommandFactoryContext BuildContextForMC6000()
        {
            var result = BuildGenericContext();

            result.Registers.Add("dat", _dat);

            result.AnalogPorts.Add("p0", _p0);
            result.AnalogPorts.Add("p1", _p1);

            result.XBusPorts.Add("x2", _x2);
            result.XBusPorts.Add("x3", _x3);

            return result;
        }

        private static CommandFactoryContext BuildGenericContext()
        {
            return new CommandFactoryContext
            {
                Accumulator = _acc,

                Registers = new Dictionary<string, IRegister>
                {
                    { "acc", _acc },
                    { "null", _null },
                },
                XBusPorts = new Dictionary<string, IXBusPort>
                {
                    { "x0", _x0 },
                    { "x1", _x1 },
                },
            };
        }

        private static CommandParameterResolverTestContext BuildCommandParameterResolver()
        {
            var result = new CommandParameterResolverTestContext
            {
                ReadableWrapperFactoryMock = new Mock<IReadableWrapperFactory>(),
                WritableWrapperFactoryMock = new Mock<IWritableWrapperFactory>(),
                IntegerLiteralFactoryMock = new Mock<IIntegerLiteralFactory>(),
            };

            result.ReadableWrapperFactoryMock.Setup(x => x.Wrap(It.IsAny<ISyncReadable>())).Returns((IReadable)null);
            result.WritableWrapperFactoryMock.Setup(x => x.Wrap(It.IsAny<ISyncWritable>())).Returns((IWritable)null);

            IReadable readable = null;
            string errorMessage = null;
            result.IntegerLiteralFactoryMock.Setup(x => x.TryCreateReadable(It.IsAny<string>(), out readable, out errorMessage)).Returns(false);

            result.CommandParameterResolver = new CommandParameterResolver(
                result.ReadableWrapperFactoryMock.Object,
                result.WritableWrapperFactoryMock.Object,
                result.IntegerLiteralFactoryMock.Object);

            return result;
        }


        private class CommandParameterResolverTestContext
        {
            public Mock<IReadableWrapperFactory> ReadableWrapperFactoryMock { get; set; }
            public Mock<IWritableWrapperFactory> WritableWrapperFactoryMock { get; set; }
            public Mock<IIntegerLiteralFactory> IntegerLiteralFactoryMock { get; set; }

            public CommandParameterResolver CommandParameterResolver { get; set; }
        }
    }
}