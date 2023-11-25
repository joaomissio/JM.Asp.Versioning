using Asp.Versioning;
using System;
using static System.AttributeTargets;

namespace JM.Asp.Versioning.Attributes
{
    /// <summary>
    /// Apply this attribute to a controller or action to determine
    /// the version in which the endpoint was included.
    /// </summary>
    [AttributeUsage(Class | Method, AllowMultiple = true, Inherited = false)]
    public class ApiVersionIncludeAttribute : Attribute
    {
        public ApiVersion Version { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersion"/> class.
        /// </summary>
        /// <param name="version">version.</param>
        public ApiVersionIncludeAttribute(double version)
        {
            Version = new ApiVersion(version);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersion"/> class.
        /// </summary>
        /// <param name="majorVersion">Major version.</param>
        /// <param name="minorVersion">Minor version.</param>
        public ApiVersionIncludeAttribute(int majorVersion, int? minorVersion)
        {
            Version = new ApiVersion(majorVersion, minorVersion);
        }
    }
}