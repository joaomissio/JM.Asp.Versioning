using Asp.Versioning;
using JM.Asp.Versioning.ConventionBuilder;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace JM.Asp.Versioning.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add API versioning using endpoint lifecycle control.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static IApiVersioningBuilder AddApiVersioningLifeCycle(this IServiceCollection services, Action<ApiVersionOptions> setupAction)
        {
            var options = new ApiVersionOptions();
            setupAction.Invoke(options);

            LifeCycleApiVersionConventionBuilder apiVersionConvention;

            if (options.AutoDetectApiVersions)
            {
                apiVersionConvention = new LifeCycleApiVersionConventionBuilder(options.Assembly ?? Assembly.GetEntryAssembly()!);
            }
            else
            {
                apiVersionConvention = new LifeCycleApiVersionConventionBuilder(options.StartApiVersion, options.CurrentApiVersion);
            }

            return services.AddApiVersioning(opt =>
            {
                opt.DefaultApiVersion = options.DefaultApiVersion;
                opt.AssumeDefaultVersionWhenUnspecified = options.AssumeDefaultVersionWhenUnspecified;
                opt.ReportApiVersions = options.ReportApiVersions;
                opt.ApiVersionReader = options.ApiVersionReader;
                opt.ApiVersionSelector = options.ApiVersionSelector;
                opt.Policies = options.Policies;
            }).AddMvc(x => x.Conventions = apiVersionConvention);
        }
    }
}
