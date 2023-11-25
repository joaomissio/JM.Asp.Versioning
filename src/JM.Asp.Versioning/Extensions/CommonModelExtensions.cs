using Asp.Versioning;
using JM.Asp.Versioning.Attributes;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Linq;

namespace JM.Asp.Versioning.Extensions
{
    internal static class CommonModelExtensions
    {
        internal static ApiVersion? GetIncludedVersion(this ICommonModel model)
        {
            return model.Attributes
                .OfType<ApiVersionIncludeAttribute>()
                .Select(a => a.Version)
                .SingleOrDefault();
        }

        internal static ApiVersion? GetRemovedVersion(this ICommonModel model)
        {
            return model.Attributes
                .OfType<ApiVersionRemoveAttribute>()
                .Select(a => a.Version)
                .SingleOrDefault();
        }
    }
}
