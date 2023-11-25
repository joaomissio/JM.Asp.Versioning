using Asp.Versioning;
using System;
using static System.AttributeTargets;

namespace JM.Asp.Versioning.Attributes
{
    /// <summary>
    /// Apply this attribute to a controller or action to determine
    /// the version in which it will be removed.
    /// </summary>
    [AttributeUsage(Class | Method, AllowMultiple = true, Inherited = false)]
    public class ApiVersionRemoveAttribute : Attribute
    {
        public ApiVersion Version { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersion"/> class.
        /// </summary>
        /// <param name="version">version.</param>
        public ApiVersionRemoveAttribute(double version)
        {
            Version = new ApiVersion(version);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersion"/> class.
        /// </summary>
        /// <param name="majorVersion">Major version.</param>
        /// <param name="minorVersion">Minor version.</param>
        public ApiVersionRemoveAttribute(int majorVersion, int? minorVersion)
        {
            Version = new ApiVersion(majorVersion, minorVersion);
        }
    }
}