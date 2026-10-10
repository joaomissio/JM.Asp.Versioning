using Asp.Versioning;
using JM.Asp.Versioning.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JM.Asp.Versioning.Internal
{
    internal sealed class ApiVersions
    {
        internal ApiVersion Start { get; }

        internal ApiVersion Current { get; }

        internal IReadOnlyList<ApiVersion> AllVersions { get; }

        internal ApiVersions(Assembly assembly)
            : this(GetLoadableTypes(assembly ?? throw new ArgumentNullException(nameof(assembly))), assembly.GetName().Name)
        {
        }

        internal ApiVersions(IEnumerable<Type> types, string? assemblyName = null)
        {
            var allVersions = GetAllApiVersions(types);

            if (allVersions.Count == 0)
            {
                var source = assemblyName is null ? "the given types" : $"assembly '{assemblyName}'";

                throw new InvalidOperationException(
                    $"No API version found in {source}. Apply [{nameof(ApiVersionIncludeAttribute)}] to at least one controller or action, " +
                    $"or set {nameof(ApiVersionOptions)}.{nameof(ApiVersionOptions.Assembly)} to the assembly that contains the controllers.");
            }

            Start = allVersions[0];
            Current = allVersions[allVersions.Count - 1];
            AllVersions = allVersions;
        }

        internal ApiVersions(int startApiVersion, int currentApiVersion)
        {
            if (currentApiVersion < startApiVersion) throw new ArgumentException($"{nameof(currentApiVersion)} must be >= {nameof(startApiVersion)}");

            Start = new ApiVersion(startApiVersion, 0);
            Current = new ApiVersion(currentApiVersion, 0);

            var allVersions = new List<ApiVersion>();

            for (var i = startApiVersion; i <= currentApiVersion; i++)
            {
                allVersions.Add(new ApiVersion(i, 0));
            }

            AllVersions = allVersions;
        }

        // A removal defines an API version too: the version in which the endpoint no longer exists.
        private static List<ApiVersion> GetAllApiVersions(IEnumerable<Type> types)
        {
            return types
               .SelectMany(x => x.GetMethods().Cast<MemberInfo>().Append(x))
               .SelectMany(x => x.GetCustomAttributes<ApiVersionIncludeAttribute>().Select(a => a.Version)
                   .Concat(x.GetCustomAttributes<ApiVersionRemoveAttribute>().Select(a => a.Version)))
               .Distinct().OrderBy(o => o).ToList();
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t is not null)!;
            }
        }

        internal ApiVersion[] GetSupportedVersions(ApiVersion? includedVersion, ApiVersion? removedVersion = null)
        {
            if (includedVersion is null || includedVersion > Current)
                return Array.Empty<ApiVersion>();
            else if (removedVersion is null)
                return new[] { Current };
            else if (includedVersion > removedVersion)
                throw new InvalidOperationException($"Cannot remove an API version ({removedVersion}) before it has been introduced ({includedVersion}).");
            else if (includedVersion == removedVersion)
                throw new InvalidOperationException($"Cannot remove an API version ({removedVersion}) in the same version it has been introduced ({includedVersion}).");
            else if (removedVersion > Current)
                return new[] { Current };
            else
                return Array.Empty<ApiVersion>();
        }

        internal ApiVersion[] GetDeprecatedVersions(ApiVersion? includedVersion, ApiVersion? removedVersion = null)
        {
            if (includedVersion == null)
                throw new ArgumentException($"{nameof(includedVersion)} cannot be null.");

            // The current version is never deprecated, even if a removal is scheduled for a later version.
            var limit = removedVersion is null || removedVersion > Current ? Current : removedVersion;

            return AllVersions.Where(v =>
                v >= includedVersion
                && v < limit).ToArray();
        }
    }
}
