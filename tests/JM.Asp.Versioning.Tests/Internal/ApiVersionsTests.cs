using Asp.Versioning;
using FluentAssertions;
using JM.Asp.Versioning.Attributes;
using JM.Asp.Versioning.Internal;
using JM.Asp.Versioning.Tests.Api.Controllers;

namespace JM.Asp.Versioning.Tests.Internal
{
    [TestClass]
    public class ApiVersionsTests
    {
        private static readonly ApiVersion V1 = new(1, 0);
        private static readonly ApiVersion V2 = new(2, 0);
        private static readonly ApiVersion V2_1 = new(2, 1);
        private static readonly ApiVersion V3 = new(3, 0);
        private static readonly ApiVersion V4 = new(4, 0);

        [ApiVersionInclude(1.0)]
        private class IncludeOnly
        {
            [ApiVersionInclude(2, 1)]
            public void Action() { }
        }

        [ApiVersionInclude(1.0)]
        private class RemoveWithoutInclude
        {
            [ApiVersionRemove(4.0)]
            public void Action() { }
        }

        private class NoAttributes
        {
            public void Action() { }
        }

        [TestMethod]
        public void Assembly_DetectsVersionsInOrder()
        {
            var versions = new ApiVersions(typeof(FirstController).Assembly);

            versions.AllVersions.Should().Equal(V1, V2, V2_1, V3);
            versions.Start.Should().Be(V1);
            versions.Current.Should().Be(V3);
        }

        [TestMethod]
        public void Types_DetectsControllerAndActionVersions()
        {
            var versions = new ApiVersions(new[] { typeof(IncludeOnly) });

            versions.AllVersions.Should().Equal(V1, V2_1);
        }

        [TestMethod]
        public void Types_RemoveAttributeCreatesVersion()
        {
            var versions = new ApiVersions(new[] { typeof(RemoveWithoutInclude) });

            versions.AllVersions.Should().Equal(V1, V4);
            versions.Current.Should().Be(V4);
        }

        [TestMethod]
        public void Types_WithoutAttributes_Throws()
        {
            var act = () => new ApiVersions(new[] { typeof(NoAttributes) });

            act.Should().Throw<InvalidOperationException>().WithMessage("No API version found*");
        }

        [TestMethod]
        public void Manual_CreatesMajorVersionRange()
        {
            var versions = new ApiVersions(1, 3);

            versions.AllVersions.Should().Equal(V1, V2, V3);
            versions.Start.Should().Be(V1);
            versions.Current.Should().Be(V3);
        }

        [TestMethod]
        public void Manual_CurrentLowerThanStart_Throws()
        {
            var act = () => new ApiVersions(3, 1);

            act.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void Supported_NotRemoved_IsCurrentVersion()
        {
            new ApiVersions(1, 3).GetSupportedVersions(V1).Should().Equal(V3);
        }

        [TestMethod]
        public void Supported_RemovedUntilCurrent_IsEmpty()
        {
            new ApiVersions(1, 3).GetSupportedVersions(V1, V2).Should().BeEmpty();
            new ApiVersions(1, 3).GetSupportedVersions(V1, V3).Should().BeEmpty();
        }

        [TestMethod]
        public void Supported_RemovedAfterCurrent_IsCurrentVersion()
        {
            new ApiVersions(1, 3).GetSupportedVersions(V1, V4).Should().Equal(V3);
        }

        [TestMethod]
        public void Supported_IncludedAfterCurrent_IsEmpty()
        {
            new ApiVersions(1, 3).GetSupportedVersions(V4).Should().BeEmpty();
        }

        [TestMethod]
        public void Supported_RemovedBeforeOrAtIncluded_Throws()
        {
            var versions = new ApiVersions(1, 3);

            var removedBefore = () => versions.GetSupportedVersions(V2, V1);
            var removedAtSame = () => versions.GetSupportedVersions(V2, V2);

            removedBefore.Should().Throw<InvalidOperationException>();
            removedAtSame.Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void Deprecated_NotRemoved_IsEveryVersionBeforeCurrent()
        {
            new ApiVersions(1, 3).GetDeprecatedVersions(V1).Should().Equal(V1, V2);
            new ApiVersions(1, 3).GetDeprecatedVersions(V2).Should().Equal(V2);
            new ApiVersions(1, 3).GetDeprecatedVersions(V3).Should().BeEmpty();
        }

        [TestMethod]
        public void Deprecated_Removed_StopsBeforeRemovedVersion()
        {
            new ApiVersions(1, 3).GetDeprecatedVersions(V1, V2).Should().Equal(V1);
            new ApiVersions(1, 3).GetDeprecatedVersions(V1, V3).Should().Equal(V1, V2);
        }

        [TestMethod]
        public void Deprecated_RemovedAfterCurrent_DoesNotContainCurrentVersion()
        {
            new ApiVersions(1, 3).GetDeprecatedVersions(V1, V4).Should().Equal(V1, V2);
        }
    }
}
