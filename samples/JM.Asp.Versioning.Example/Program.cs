using Asp.Versioning.ApiExplorer;
using JM.Asp.Versioning.Example.Configurations;
using JM.Asp.Versioning.Extensions;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddApiVersioningLifeCycle(x =>
{
    x.Assembly = Assembly.GetExecutingAssembly();
    x.AutoDetectApiVersions = true;
    x.ReportApiVersions = true;
    x.AssumeDefaultVersionWhenUnspecified = true;
    x.DefaultApiVersion = new Asp.Versioning.ApiVersion(3.0);
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwagger();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerPage(app.Services.GetService<IApiVersionDescriptionProvider>()!);
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
