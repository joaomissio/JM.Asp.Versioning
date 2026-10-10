using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
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
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

        services.AddSwaggerGen(c =>
        {
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
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

/// <summary>
/// Creates one swagger document per API version.
/// </summary>
public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigureSwaggerOptions"/> class.
    /// </summary>
    /// <param name="provider">api version provider.</param>
    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }

    /// <inheritdoc />
    public void Configure(SwaggerGenOptions options)
    {
        var assemblyDetails = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyProductAttribute>();

        foreach (var description in _provider.ApiVersionDescriptions)
        {
            var descDeprecated = description.IsDeprecated ? " - <b>This API version has been deprecated</b>" : string.Empty;

            options.SwaggerDoc(description.GroupName, new OpenApiInfo()
            {
                Title = $"{assemblyDetails?.Product} {description.ApiVersion}",
                Version = description.ApiVersion.ToString(),
                Description = $"JM.Asp.Versioning.Example {descDeprecated}"
            });
        }
    }
}
