# JM.Asp.Versioning

[![NuGet](https://img.shields.io/nuget/v/JM.Asp.Versioning)](https://www.nuget.org/packages/JM.Asp.Versioning)
[![Downloads](https://img.shields.io/nuget/dt/JM.Asp.Versioning)](https://www.nuget.org/packages/JM.Asp.Versioning)
[![CI](https://github.com/joaomissio/JM.Asp.Versioning/actions/workflows/ci.yml/badge.svg)](https://github.com/joaomissio/JM.Asp.Versioning/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/joaomissio/JM.Asp.Versioning/blob/main/licenses/LICENSE.txt)

A simple convention for versioning ASP.NET Core web APIs. It extends [Asp.Versioning](https://github.com/dotnet/aspnet-api-versioning).

With plain Asp.Versioning, each new API version needs a new `[ApiVersion]` on every endpoint that still exists, or a copy of the controller. This package controls the **lifecycle** of the endpoints instead. You say in which version an endpoint was **included** and, if it applies, in which version it was **removed**. The package does the rest:

- An endpoint is part of every version from its inclusion until its removal.
- The newest version is the only **supported** version. All older versions are reported as **deprecated**.

```csharp
[ApiVersionInclude(1.0)]
[ApiController]
[Route("[controller]")]
public class OrdersController : ControllerBase
{
    [HttpGet]                    // v1 and all later versions
    public IActionResult Get() => Ok();

    [ApiVersionRemove(2.0)]      // v1 only
    [HttpGet("legacy")]
    public IActionResult Legacy() => Ok();

    [ApiVersionInclude(2.0)]     // v2 and all later versions
    [HttpGet("summary")]
    public IActionResult Summary() => Ok();
}
```

## Compatibility

| Target framework | Asp.Versioning.Mvc |
|---|---|
| .NET 10 | 10.2.x |
| .NET 9 | 8.1.x |
| .NET 8 | 8.1.x |
| .NET 7 | 7.1.x |
| .NET 6 | 6.4.x |

## Installation

```bash
dotnet add package JM.Asp.Versioning
```

## Getting started

Register the package in `Program.cs`. `AddApiVersioningLifeCycle` replaces the call to `AddApiVersioning`.

```csharp
using JM.Asp.Versioning.Extensions;
using System.Reflection;

builder.Services.AddControllers();

builder.Services.AddApiVersioningLifeCycle(x =>
{
    x.Assembly = Assembly.GetExecutingAssembly();
    x.ReportApiVersions = true;
    x.AssumeDefaultVersionWhenUnspecified = true;
    x.DefaultApiVersion = new Asp.Versioning.ApiVersion(3, 0);
});
```

The method returns the `IApiVersioningBuilder` of Asp.Versioning. Chain `AddApiExplorer` to get one Swagger/OpenAPI document per version:

```csharp
builder.Services.AddApiVersioningLifeCycle(x => { /* ... */ })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });
```

Clients select the version the same way as in Asp.Versioning. The default is the `api-version` query string: `GET /Orders?api-version=2.0`.

## Options

`ApiVersionOptions` has all the options of `ApiVersioningOptions` from Asp.Versioning (`DefaultApiVersion`, `ReportApiVersions`, `ApiVersionReader`, and so on) plus the options below.

| Option | Default | Description |
|---|---|---|
| `AutoDetectApiVersions` | `true` | The package finds the API versions from the attributes in the assembly. |
| `Assembly` | entry assembly | The assembly that contains the controllers. Used when `AutoDetectApiVersions` is `true`. |
| `StartApiVersion` | `0` | First major version. Required when `AutoDetectApiVersions` is `false`. Must be 1 or more. |
| `CurrentApiVersion` | `0` | Current major version. Required when `AutoDetectApiVersions` is `false`. Must not be less than `StartApiVersion`. |

### Automatic detection (default)

The list of API versions is each distinct version found in `[ApiVersionInclude]` and `[ApiVersionRemove]` attributes. The lowest version is the start version. The highest version is the current version.

### Manual range

Set the range yourself if you do not want the attributes to define it. Only major versions are created (`1.0`, `2.0`, `3.0`).

```csharp
builder.Services.AddApiVersioningLifeCycle(x =>
{
    x.AutoDetectApiVersions = false;
    x.StartApiVersion = 1;
    x.CurrentApiVersion = 3;
});
```

## Attributes

Apply the attributes to a controller class or to an action method. Each attribute can be applied one time per class or method.

| Attribute | Meaning |
|---|---|
| `[ApiVersionInclude(version)]` | The controller or action exists from this version. |
| `[ApiVersionRemove(version)]` | The controller or action does not exist from this version. |

Each attribute has two constructors:

```csharp
[ApiVersionInclude(2.1)]     // number
[ApiVersionInclude(2, 10)]   // major, minor
```

> As a number, `2.10` is equal to `2.1`. Use the `(major, minor)` constructor for a minor version greater than 9.

## Lifecycle rules

- A controller needs `[ApiVersionInclude]` to take part in the lifecycle. A controller without it is not changed by this package (Asp.Versioning gives it the default API version). Attributes on its actions are ignored.
- An action without attributes uses the versions of its controller.
- An action can narrow the range of its controller: it can be included later and removed earlier.
- `[ApiVersionRemove]` also creates a version. `[ApiVersionRemove(4.0)]` creates v4, even if no endpoint is included in v4.

For an endpoint included in version `I` and removed in version `R`, with current version `C`:

| Requested version `V` | Result |
|---|---|
| `V` < `I` | Not available (HTTP 400) |
| `I` <= `V` < `R` and `V` < `C` | Available, version reported as deprecated |
| `V` = `C` and the endpoint is not removed | Available, version reported as supported |
| `V` >= `R` | Not available (HTTP 400) |

## Validation errors

The application throws `InvalidOperationException` at startup if the attributes are not consistent.

| Message | Cause |
|---|---|
| `No API version found in assembly ...` | No attribute was found. Add `[ApiVersionInclude]` or set `Assembly` to the correct assembly. |
| `... cannot be included and removed in the same version.` | `[ApiVersionInclude]` and `[ApiVersionRemove]` have the same version. |
| `... cannot be removed before it is included.` | The remove version is lower than the include version. |
| `Action (...) version cannot be included before controller (...) version.` | The action include version is lower than the controller include version. |
| `Action (...) version cannot be included after controller (...) is removed.` | The action include version is not lower than the controller remove version. |
| `Action (...) version cannot be removed after controller (...) is removed.` | The action remove version is higher than the controller remove version. |
| `ApiVersionOptions.StartApiVersion must be >= 1 ...` | Manual range without `StartApiVersion`. |

## Example

The [samples](https://github.com/joaomissio/JM.Asp.Versioning/tree/main/samples) folder contains a complete project with Swagger. Its controllers:

```csharp
[ApiVersionInclude(1.0)]
public class FirstController : ControllerBase
{
    public IActionResult A_IncludeOnV1() => Ok("A");

    [ApiVersionRemove(2.0)]
    public IActionResult B_RemoveOnV2() => Ok("B");

    [ApiVersionInclude(2.1)]
    public IActionResult C_IncludeOnV2_1() => Ok("C");
}

[ApiVersionInclude(2.0)]
public class SecondController : ControllerBase
{
    public IActionResult A_IncludeOnV2() => Ok("A");

    [ApiVersionRemove(3.0)]
    public IActionResult B_RemoveOnV3() => Ok("B");

    [ApiVersionInclude(3.0)]
    public IActionResult C_IncludeOnV3() => Ok("C");
}
```

The detected versions are `1.0`, `2.0`, `2.1` and `3.0`. Version `3.0` is supported. The other versions are deprecated.

| Endpoint | v1 | v2 | v2.1 | v3 |
|---|:-:|:-:|:-:|:-:|
| `First/A_IncludeOnV1` | ✔ | ✔ | ✔ | ✔ |
| `First/B_RemoveOnV2` | ✔ | | | |
| `First/C_IncludeOnV2_1` | | | ✔ | ✔ |
| `Second/A_IncludeOnV2` | | ✔ | ✔ | ✔ |
| `Second/B_RemoveOnV3` | | ✔ | ✔ | |
| `Second/C_IncludeOnV3` | | | | ✔ |

### v1
![Example_V1](https://raw.githubusercontent.com/joaomissio/JM.Asp.Versioning/main/docs/example_v1.png)

### v2
![Example_V2](https://raw.githubusercontent.com/joaomissio/JM.Asp.Versioning/main/docs/example_v2.png)

### v2.1
![Example_V2.1](https://raw.githubusercontent.com/joaomissio/JM.Asp.Versioning/main/docs/example_v2_1.png)

### v3
![Example_V3](https://raw.githubusercontent.com/joaomissio/JM.Asp.Versioning/main/docs/example_v3.png)

## Migrating from 1.x to 2.0

- New targets: `net9.0` and `net10.0`. The Asp.Versioning dependencies were updated (see [Compatibility](#compatibility)).
- `[ApiVersionRemove]` now creates an API version in automatic detection. In 1.x only `[ApiVersionInclude]` did. If you have a remove version that no include uses, a new API version appears.
- The attribute constructor `(int majorVersion, int? minorVersion)` is now `(int majorVersion, int minorVersion)`. The old constructor could not be used in attribute syntax.
- The attributes are `sealed` and can be applied only one time per class or method. Two attributes of the same type caused a runtime exception in 1.x.
- The current version is no longer reported as deprecated when a removal is scheduled for a later version.
- Manual range: `StartApiVersion` must be 1 or more. In 1.x, an unset range silently created version `0.0`.
- Actions are validated against the remove version of the controller (see [Validation errors](#validation-errors)).

## License

[MIT](https://github.com/joaomissio/JM.Asp.Versioning/blob/main/licenses/LICENSE.txt)
