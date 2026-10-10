using Asp.Versioning;
using FluentAssertions;
using JM.Asp.Versioning.Extensions;
using JM.Asp.Versioning.Tests.Api.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace JM.Asp.Versioning.Tests
{
    [TestClass]
    public class LifeCycleIntegrationTests
    {
        private static WebApplication _app = null!;
        private static HttpClient _client = null!;

        [ClassInitialize]
        public static async Task Initialize(TestContext _)
        {
            var apiAssembly = typeof(FirstController).Assembly;

            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseTestServer();

            builder.Services.AddControllers().AddApplicationPart(apiAssembly);

            builder.Services.AddApiVersioningLifeCycle(x =>
            {
                x.Assembly = apiAssembly;
                x.ReportApiVersions = true;
                x.AssumeDefaultVersionWhenUnspecified = true;
                x.DefaultApiVersion = new ApiVersion(3, 0);
            });

            _app = builder.Build();
            _app.MapControllers();

            await _app.StartAsync();

            _client = _app.GetTestClient();
        }

        [ClassCleanup]
        public static async Task Cleanup()
        {
            _client.Dispose();
            await _app.DisposeAsync();
        }

        [TestMethod]
        [DataRow("First/A_IncludeOnV1", "1.0", true)]
        [DataRow("First/A_IncludeOnV1", "2.0", true)]
        [DataRow("First/A_IncludeOnV1", "2.1", true)]
        [DataRow("First/A_IncludeOnV1", "3.0", true)]
        [DataRow("First/B_RemoveOnV2", "1.0", true)]
        [DataRow("First/B_RemoveOnV2", "2.0", false)]
        [DataRow("First/B_RemoveOnV2", "2.1", false)]
        [DataRow("First/B_RemoveOnV2", "3.0", false)]
        [DataRow("First/C_IncludeOnV2_1", "1.0", false)]
        [DataRow("First/C_IncludeOnV2_1", "2.0", false)]
        [DataRow("First/C_IncludeOnV2_1", "2.1", true)]
        [DataRow("First/C_IncludeOnV2_1", "3.0", true)]
        [DataRow("Second/A_IncludeOnV2", "1.0", false)]
        [DataRow("Second/A_IncludeOnV2", "2.0", true)]
        [DataRow("Second/A_IncludeOnV2", "2.1", true)]
        [DataRow("Second/A_IncludeOnV2", "3.0", true)]
        [DataRow("Second/B_RemoveOnV3", "1.0", false)]
        [DataRow("Second/B_RemoveOnV3", "2.0", true)]
        [DataRow("Second/B_RemoveOnV3", "2.1", true)]
        [DataRow("Second/B_RemoveOnV3", "3.0", false)]
        [DataRow("Second/C_IncludeOnV3", "1.0", false)]
        [DataRow("Second/C_IncludeOnV3", "2.0", false)]
        [DataRow("Second/C_IncludeOnV3", "2.1", false)]
        [DataRow("Second/C_IncludeOnV3", "3.0", true)]
        public async Task Endpoint_IsAvailableOnlyInItsLifeCycle(string path, string version, bool available)
        {
            var response = await _client.GetAsync($"{path}?api-version={version}");

            response.StatusCode.Should().Be(available ? HttpStatusCode.OK : HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task UnspecifiedVersion_UsesDefaultVersion()
        {
            var current = await _client.GetAsync("Second/C_IncludeOnV3");
            var removed = await _client.GetAsync("Second/B_RemoveOnV3");

            current.StatusCode.Should().Be(HttpStatusCode.OK);
            removed.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Response_ReportsSupportedAndDeprecatedVersions()
        {
            var response = await _client.GetAsync("First/A_IncludeOnV1?api-version=3.0");

            string.Join(", ", response.Headers.GetValues("api-supported-versions")).Should().Be("3.0");
            string.Join(", ", response.Headers.GetValues("api-deprecated-versions")).Should().Be("1.0, 2.0, 2.1");
        }
    }
}
