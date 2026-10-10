using Asp.Versioning;
using System;
using static System.AttributeTargets;

namespace JM.Asp.Versioning.Attributes
{
    /// <summary>
    /// Apply this attribute to a controller or action to determine
    /// the version in which the endpoint was included.
    /// </summary>
    [AttributeUsage(Class | Method, AllowMultiple = false, Inherited = false)]
    public sealed class ApiVersionIncludeAttribute : Attribute
    {
        /// <summary>
        /// Gets the version in which the endpoint was included.
        /// </summary>
        public ApiVersion Version { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionIncludeAttribute"/> class.
        /// </summary>
        /// <param name="version">
        /// Version as a number, for example 2.1. A minor version greater than 9 cannot be
        /// written this way (2.10 is the same number as 2.1). Use the (major, minor) constructor instead.
        /// </param>
        public ApiVersionIncludeAttribute(double version)
        {
            Version = new ApiVersion(version);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionIncludeAttribute"/> class.
        /// </summary>
        /// <param name="majorVersion">Major version.</param>
        /// <param name="minorVersion">Minor version.</param>
        public ApiVersionIncludeAttribute(int majorVersion, int minorVersion)
        {
            Version = new ApiVersion(majorVersion, minorVersion);
        }
    }
}
