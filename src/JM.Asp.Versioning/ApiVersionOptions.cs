using Asp.Versioning;
using System.Reflection;

namespace JM.Asp.Versioning
{
    /// <summary>
    /// Options for API versioning with endpoint lifecycle control.
    /// Extends the options of Asp.Versioning.
    /// </summary>
    public class ApiVersionOptions : ApiVersioningOptions
    {
        /// <summary>
        /// Gets or sets if automatically detect API versions. Default is true.
        /// </summary>
        public bool AutoDetectApiVersions { get; set; } = true;

        /// <summary>
        /// Gets or sets the assembly that contains controllers. Used only if AutoDetectApiVersions equals true.
        /// Default is the entry assembly.
        /// </summary>
        public Assembly? Assembly { get; set; }

        /// <summary>
        ///  Gets or sets start api version (major). Use only if AutoDetectApiVersions equals false.
        /// </summary>
        public int StartApiVersion { get; set; }

        /// <summary>
        ///  Gets or sets current api version (major). Use only if AutoDetectApiVersions equals false.
        /// </summary>
        public int CurrentApiVersion { get; set; }
    }
}
