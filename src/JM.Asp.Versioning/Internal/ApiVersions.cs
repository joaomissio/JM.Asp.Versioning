using Asp.Versioning;
using JM.Asp.Versioning.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JM.Asp.Versioning.Internal
{
    internal class ApiVersions
    {
        internal ApiVersion Start => _startVersion;
        internal ApiVersion Current => _currentVersion;

        private readonly ApiVersion _startVersion;

        private readonly ApiVersion _currentVersion;

        internal IReadOnlyList<ApiVersion> AllVersions { get; }

        internal ApiVersions(Assembly assembly)
        {
            var allVersions = GetAllApiVersions(assembly);

            _startVersion = allVersions.First().Version;
            _currentVersion = allVersions.Last().Version;

            AllVersions = allVersions.Select(s => s.Version).ToList();
        }

        internal ApiVersions(int startApiVersion, int currentApiVersion)
        {
            if (currentApiVersion < startApiVersion) throw new ArgumentException($"{nameof(currentApiVersion)} must be >= {nameof(startApiVersion)}");

            _startVersion = new ApiVersion(startApiVersion, 0);
            _currentVersion = new ApiVersion(currentApiVersion, 0);

            var allVersions = new List<ApiVersion>();

            for (var i = startApiVersion; i <= currentApiVersion; i++)
            {
                allVersions.Add(new ApiVersion(i, 0));
            }

            AllVersions = allVersions;
        }

        internal (ApiVersion StartVersion, ApiVersion CurrentVersion) GetRangeApiVersions(Assembly assembly)
        {
            var activeVersions = GetAllApiVersions(assembly);

            var startVersion = activeVersions.FirstOrDefault();
            var currentVersion = activeVersions.LastOrDefault();

            if (startVersion is null || currentVersion is null)
            {
                throw new InvalidOperationException("No api versioning found.");
            }

            return (startVersion.Version, currentVersion.Version);
        }

        internal List<ApiVersionIncludeAttribute> GetAllApiVersions(Assembly assembly)
        {
            var allVersions = assembly.GetTypes()
               .SelectMany(x => x.GetMethods().Cast<MemberInfo>().Append(x))
               .SelectMany(x => x.GetCustomAttributes<ApiVersionIncludeAttribute>())
               .Distinct().OrderBy(o => o.Version).ToList();


            if (allVersions is null || !allVersions.Any())
            {
                throw new InvalidOperationException("No api versioning found.");
            }

            return allVersions;
        }

        internal ApiVersion[] GetSupportedVersions(ApiVersion? includedVersion, ApiVersion? removedVersion = null)
        {
            if (includedVersion is null || includedVersion > _currentVersion)
                return new ApiVersion[0];
            else if (removedVersion is null)
                return new[] { _currentVersion };
            else if (includedVersion > removedVersion)
                throw new InvalidOperationException($"Cannot remove an API version ({removedVersion}) before it has been introduced ({includedVersion}).");
            else if (includedVersion == removedVersion)
                throw new InvalidOperationException($"Cannot remove an API version ({removedVersion}) in the same version it has been introduced ({includedVersion}).");
            else if (removedVersion > _currentVersion)
                return new[] { _currentVersion };
            else
                return new ApiVersion[0];
        }

        internal ApiVersion[] GetDeprecatedVersions(ApiVersion? includedVersion, ApiVersion? removedVersion = null)
        {
            if (includedVersion == null)
                throw new ArgumentException($"{nameof(includedVersion)} cannot be null.");

            if (removedVersion == null)
            {
                return AllVersions.Where(v =>
                    v >= includedVersion
                    && v < _currentVersion).ToArray();
            }

            return AllVersions.Where(v =>
                v >= includedVersion
                && v < removedVersion).ToArray();
        }
    }
}
