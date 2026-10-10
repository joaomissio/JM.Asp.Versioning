using Asp.Versioning;
using System;
using static System.AttributeTargets;

namespace JM.Asp.Versioning.Attributes
{
    /// <summary>
    /// Apply this attribute to a controller or action to determine
    /// the version in which it will be removed.
    /// </summary>
    [AttributeUsage(Class | Method, AllowMultiple = false, Inherited = false)]
    public sealed class ApiVersionRemoveAttribute : Attribute
    {
        /// <summary>
        /// Gets the version in which the endpoint is removed.
        /// </summary>
        public ApiVersion Version { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionRemoveAttribute"/> class.
        /// </summary>
        /// <param name="version">
        /// Version as a number, for example 2.1. A minor version greater than 9 cannot be
        /// written this way (2.10 is the same number as 2.1). Use the (major, minor) constructor instead.
        /// </param>
        public ApiVersionRemoveAttribute(double version)
        {
            Version = new ApiVersion(version);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionRemoveAttribute"/> class.
        /// </summary>
        /// <param name="majorVersion">Major version.</param>
        /// <param name="minorVersion">Minor version.</param>
        public ApiVersionRemoveAttribute(int majorVersion, int minorVersion)
        {
            Version = new ApiVersion(majorVersion, minorVersion);
        }
    }
}
