using Asp.Versioning.ApiExplorer;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace JM.Asp.Versioning.Example.Configurations;

/// <summary>
/// Swagger configuration.
/// </summary>
public static class SwaggerConfiguration
{
    /// <summary>
    /// Add swagger dependency.
    /// </summary>
    /// <param name="services">app service collection.</param>
    public static void AddSwagger(this IServiceCollection services)
    {
        var startupAssembly = Assembly.GetEntryAssembly()!;

        services.AddSwaggerGen(c =>
        {
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);

            var provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();

            foreach (var description in provider.ApiVersionDescriptions)
            {
                var assemblyDetails = startupAssembly.GetCustomAttribute<AssemblyProductAttribute>();

                var descDeprecated = description.IsDeprecated ? " - <b>This API version has been deprecated</b>" : string.Empty;

                c.SwaggerDoc(description.GroupName, new OpenApiInfo()
                {
                    Title = $"{assemblyDetails?.Product} {description.ApiVersion}",
                    Version = description.ApiVersion.ToString(),
                    Description = $"JM.Asp.Versioning.Example {descDeprecated}"
                });
            }
        });
    }

    /// <summary>
    /// Configure swagger in pipeline.
    /// </summary>
    /// <param name="app">instance of app.</param>
    /// <param name="provider">api version provider.</param>
    public static void UseSwaggerPage(this IApplicationBuilder app, IApiVersionDescriptionProvider provider)
    {
        app.UseSwagger();
        app.UseSwaggerUI(sw =>
        {
            foreach (var description in provider.ApiVersionDescriptions.Select(s => s.GroupName))
            {
                sw.SwaggerEndpoint($"./swagger/{description}/swagger.json", $"Example - {description.ToUpperInvariant()}");
            }

            sw.RoutePrefix = string.Empty;
        });

    }
}
