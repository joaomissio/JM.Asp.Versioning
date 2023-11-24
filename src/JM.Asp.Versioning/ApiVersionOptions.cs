using Asp.Versioning;
using System.Reflection;

namespace JM.Asp.Versioning
{
    public class ApiVersionOptions : ApiVersioningOptions
    {
        /// <summary>
        /// Gets or sets if automatically detect API versions.
        /// </summary>
        public bool AutoDetectApiVersions { get; set; } = true;

        /// <summary>
        /// Gets or sets the assembly that contains controllers.
        /// </summary>
        public Assembly? Assembly { get; set; }

        /// <summary>
        ///  Gets or sets start api version. Use only if AutoDetectApiVersions equals false.
        /// </summary>
        public int StartApiVersion { get; set; }

        /// <summary>
        ///  Gets or sets current api version. Use only if AutoDetectApiVersions equals false.
        /// </summary>
        public int CurrentApiVersion { get; set; }
    }
}
