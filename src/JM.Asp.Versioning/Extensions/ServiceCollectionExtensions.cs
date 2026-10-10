using Asp.Versioning;
using JM.Asp.Versioning.ConventionBuilder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace JM.Asp.Versioning.Extensions
{
    /// <summary>
    /// Extension methods to register API versioning with endpoint lifecycle control.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add API versioning using endpoint lifecycle control.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="setupAction">Configures the versioning options.</param>
        /// <returns>The API versioning builder, to chain calls such as AddApiExplorer.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="services"/> is null.</exception>
        /// <exception cref="InvalidOperationException">The options are not valid.</exception>
        public static IApiVersioningBuilder AddApiVersioningLifeCycle(this IServiceCollection services, Action<ApiVersionOptions>? setupAction = null)
        {
            if (services is null) throw new ArgumentNullException(nameof(services));

            var options = new ApiVersionOptions();
            setupAction?.Invoke(options);

            var apiVersionConvention = CreateConvention(options);

            return services.AddApiVersioning(opt =>
            {
                opt.DefaultApiVersion = options.DefaultApiVersion;
                opt.AssumeDefaultVersionWhenUnspecified = options.AssumeDefaultVersionWhenUnspecified;
                opt.ReportApiVersions = options.ReportApiVersions;
                opt.ApiVersionReader = options.ApiVersionReader;
                opt.ApiVersionSelector = options.ApiVersionSelector;
                opt.Policies = options.Policies;
                opt.RouteConstraintName = options.RouteConstraintName;
#if NET7_0_OR_GREATER
                opt.UnsupportedApiVersionStatusCode = options.UnsupportedApiVersionStatusCode;
#endif
            }).AddMvc(x => x.Conventions = apiVersionConvention);
        }

        private static LifeCycleApiVersionConventionBuilder CreateConvention(ApiVersionOptions options)
        {
            if (options.AutoDetectApiVersions)
            {
                var assembly = options.Assembly ?? Assembly.GetEntryAssembly()
                    ?? throw new InvalidOperationException(
                        $"Cannot find the entry assembly. Set {nameof(ApiVersionOptions)}.{nameof(ApiVersionOptions.Assembly)} to the assembly that contains the controllers.");

                return new LifeCycleApiVersionConventionBuilder(assembly);
            }

            if (options.StartApiVersion < 1)
            {
                throw new InvalidOperationException(
                    $"{nameof(ApiVersionOptions)}.{nameof(ApiVersionOptions.StartApiVersion)} must be >= 1 when {nameof(ApiVersionOptions.AutoDetectApiVersions)} is false.");
            }

            if (options.CurrentApiVersion < options.StartApiVersion)
            {
                throw new InvalidOperationException(
                    $"{nameof(ApiVersionOptions)}.{nameof(ApiVersionOptions.CurrentApiVersion)} must be >= {nameof(ApiVersionOptions.StartApiVersion)} when {nameof(ApiVersionOptions.AutoDetectApiVersions)} is false.");
            }

            return new LifeCycleApiVersionConventionBuilder(options.StartApiVersion, options.CurrentApiVersion);
        }
    }
}
