# JM.Asp.Versioning
This is a simple convention for versioning web APIs. It extends [Asp.Versioning].

The objective of this package is to avoid duplicating code to always maintain endpoints from previous versions that are still used in the most recent version of the API. This package basically controls the lifecycle of the endpoints. So as long as a controller or action is not removed it will be contained in the latest version of the api.

# Getting Started

## Nuget Package
[![Nuget](https://img.shields.io/nuget/dt/JM.Asp.Versioning)](https://www.nuget.org/packages/JM.Asp.Versioning)  

## (1) Dependency Injection.

### 2.1) Extended options.
In addition to the options available in "AddApiVersioning" in the Asp.Versioning package, this package provides the following options described below.

- AutoDetectApiVersions: If activated, it allows the package to automatically identify the versions in use in the api.
- Assembly: Necessary to identify where you will search for controllers. Works in conjunction with 'AutoDetectApiVersions' enabled.
- StartApiVersion and CurrentApiVersion: If you want to manually control the versions of your api with this package, simply enter the values in these properties.

### 2.2) Using the package.

```
builder.Services.AddApiVersioningLifeCycle(x =>
{
    x.Assembly = Assembly.GetExecutingAssembly();
    x.AutoDetectApiVersions = true;
    x.ReportApiVersions = true;
    x.AssumeDefaultVersionWhenUnspecified = true;
    x.DefaultApiVersion = new Asp.Versioning.ApiVersion(3.0);
});
```
## (2) Features
The attributes below are available for use in classes and method on controllers.

### 2.1) Attribute ApiVersionInclude
This attribute indicates that method or controller class is being included in the specified version.
```
[ApiVersionInclude(2)]
```

### 2.2) Attribute ApiVersionRemove 
This attribute indicates that method or controller class is being removed in the specified version.
```
[ApiVersionRemove(3.1)]
```

### Example

The "samples" folder contains a project with a complete example of using the package.

### v1
![Example_V1](https://raw.githubusercontent.com/joaomissio/JM.Asp.Versioning/main/docs/example_v1.png)

### v2
![Example_V2](https://raw.githubusercontent.com/joaomissio/JM.Asp.Versioning/main/docs/example_v2.png)

### v2.1
![Example_V2.1](https://raw.githubusercontent.com/joaomissio/JM.Asp.Versioning/main/docs/example_v2_1.png)

### v3
![Example_V3](https://raw.githubusercontent.com/joaomissio/JM.Asp.Versioning/main/docs/example_v3.png)
