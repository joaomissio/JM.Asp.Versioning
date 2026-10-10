using FluentAssertions;
using JM.Asp.Versioning.Extensions;
using JM.Asp.Versioning.Tests.Api.Controllers;
using Microsoft.Extensions.DependencyInjection;

namespace JM.Asp.Versioning.Tests.Extensions
{
    [TestClass]
    public class ServiceCollectionExtensionsTests
    {
        [TestMethod]
        public void NullServices_Throws()
        {
            var act = () => ServiceCollectionExtensions.AddApiVersioningLifeCycle(null!);

            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void AutoDetect_WithAssembly_ReturnsBuilder()
        {
            var builder = new ServiceCollection().AddApiVersioningLifeCycle(x => x.Assembly = typeof(FirstController).Assembly);

            builder.Should().NotBeNull();
        }

        [TestMethod]
        public void AutoDetect_AssemblyWithoutVersions_Throws()
        {
            var act = () => new ServiceCollection().AddApiVersioningLifeCycle(x => x.Assembly = typeof(object).Assembly);

            act.Should().Throw<InvalidOperationException>().WithMessage("No API version found*");
        }

        [TestMethod]
        public void Manual_ValidRange_ReturnsBuilder()
        {
            var builder = new ServiceCollection().AddApiVersioningLifeCycle(x =>
            {
                x.AutoDetectApiVersions = false;
                x.StartApiVersion = 1;
                x.CurrentApiVersion = 3;
            });

            builder.Should().NotBeNull();
        }

        [TestMethod]
        public void Manual_VersionsNotSet_Throws()
        {
            var act = () => new ServiceCollection().AddApiVersioningLifeCycle(x => x.AutoDetectApiVersions = false);

            act.Should().Throw<InvalidOperationException>().WithMessage("*StartApiVersion*");
        }

        [TestMethod]
        public void Manual_CurrentLowerThanStart_Throws()
        {
            var act = () => new ServiceCollection().AddApiVersioningLifeCycle(x =>
            {
                x.AutoDetectApiVersions = false;
                x.StartApiVersion = 3;
                x.CurrentApiVersion = 1;
            });

            act.Should().Throw<InvalidOperationException>().WithMessage("*CurrentApiVersion*");
        }
    }
}
