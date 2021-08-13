using System.Collections.Generic;
using FluentAssertions;
using ShenzhenIO.Emulator.Core.Execution;
using ShenzhenIO.Emulator.Core.Language;

namespace ShenzhenIO.Emulator.Tests.Extensions
{
    public static class CommandContainerExtensions
    {
        public static void ShouldDenoteSuccess(this CommandContainer container, TokenizedCommand expectedDescription, ICommand expectedCommand, string because)
        {
            container.Should().NotBeNull(because);

            container.Succeeded.Should().BeTrue(because);
            container.Description.Should().BeSameAs(expectedDescription, because);
            container.Command.Should().BeSameAs(expectedCommand, because);
            container.ErrorMessages.Should().BeEmpty(because);
        }

        public static void ShouldDenoteFailure(this CommandContainer container, TokenizedCommand expectedDescription, IList<string> expectedErrorMessages, string because)
        {
            container.Should().NotBeNull(because);

            container.Succeeded.Should().BeFalse(because);
            container.Description.Should().BeSameAs(expectedDescription, because);
            container.Command.Should().BeNull(because);
            container.ErrorMessages.Should().BeEquivalentTo(expectedErrorMessages, because);
        }
    }
}