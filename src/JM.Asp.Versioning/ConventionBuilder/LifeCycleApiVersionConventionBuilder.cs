using Asp.Versioning;
using Asp.Versioning.Conventions;
using JM.Asp.Versioning.Extensions;
using JM.Asp.Versioning.Internal;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System;
using System.Reflection;

namespace JM.Asp.Versioning.ConventionBuilder
{
    /// <summary>
    /// Configure the range of APIs that are available. Only the current API version
    /// will be supported. All other API versions will be marked as deprecated.
    /// </summary>
    public class LifeCycleApiVersionConventionBuilder : ApiVersionConventionBuilder
    {
        private readonly ApiVersion _startVersion;
        private readonly ApiVersion _currentVersion;
        private readonly ApiVersions _allVersions;
        private readonly ApiVersionConventionBuilder _apiVersionConventionBuilder;

        internal ApiVersions AllVersions => _allVersions;

        internal LifeCycleApiVersionConventionBuilder(ApiVersions apiVersions, ApiVersionConventionBuilder apiVersionConventionBuilder)
        {
            _allVersions = apiVersions;
            _startVersion = _allVersions.Start;
            _currentVersion = _allVersions.Current;

            _apiVersionConventionBuilder = apiVersionConventionBuilder;
        }

        /// <summary>
        /// Initializes a new instance that detects the API versions from the attributes found in the assembly.
        /// </summary>
        /// <param name="assembly">The assembly that contains the controllers.</param>
        public LifeCycleApiVersionConventionBuilder(Assembly assembly)
            : this(new ApiVersions(assembly), new ApiVersionConventionBuilder())
        {
        }

        /// <summary>
        /// Initializes a new instance with a fixed range of major API versions.
        /// </summary>
        /// <param name="startVersion">The first (oldest) major version.</param>
        /// <param name="currentVersion">The current (newest) major version.</param>
        public LifeCycleApiVersionConventionBuilder(int startVersion, int currentVersion)
            : this(new ApiVersions(startVersion, currentVersion), new ApiVersionConventionBuilder())
        {
        }

        /// <inheritdoc />
        public override bool ApplyTo(ControllerModel controller)
        {
            var controllerIntroducedInVersion = controller.GetIncludedVersion();
            var controllerRemovedAsOfVersion = controller.GetRemovedVersion();

            ValidateControllerVersions(controller, controllerIntroducedInVersion, controllerRemovedAsOfVersion);

            if (MustUseDefaultApiConvention(controllerIntroducedInVersion, controllerRemovedAsOfVersion))
            {
                return _apiVersionConventionBuilder.ApplyTo(controller);
            }

            var conventionBuilder = _apiVersionConventionBuilder.Controller(controller.ControllerType);

            SetControllerApiVersions(conventionBuilder, controllerIntroducedInVersion, controllerRemovedAsOfVersion);

            SetActionApiVersions(controller, controllerIntroducedInVersion, controllerRemovedAsOfVersion, conventionBuilder);

            return _apiVersionConventionBuilder.ApplyTo(controller);
        }

        private bool MustUseDefaultApiConvention(ApiVersion? includedVersion, ApiVersion? removedVersion)
        {
            return includedVersion is null
                   || includedVersion > _currentVersion
                   || (removedVersion is not null && removedVersion <= _startVersion);
        }

        private static void ValidateControllerVersions(ControllerModel controller, ApiVersion? includedVersion, ApiVersion? removedVersion)
        {
            if (includedVersion is null || removedVersion is null)
            {
                return;
            }

            if (includedVersion == removedVersion)
            {
                throw new InvalidOperationException($"({controller.ControllerType}) ApiVersion cannot be included and removed in the same version.");
            }

            if (removedVersion < includedVersion)
            {
                throw new InvalidOperationException($"({controller.ControllerType}) ApiVersion cannot be removed before it is included.");
            }
        }

        private void SetActionApiVersions(ControllerModel controller, ApiVersion? includedVersion, ApiVersion? removedVersion, IControllerConventionBuilder conventionBuilder)
        {
            foreach (var actionModel in controller.Actions)
            {
                var actionModelIntroduced = actionModel.GetIncludedVersion();
                var actionModelRemoved = actionModel.GetRemovedVersion();

                ValidateActionModel(controller, actionModel, includedVersion, removedVersion, actionModelIntroduced, actionModelRemoved);

                var actionIntroducedVersion = actionModelIntroduced ?? includedVersion;
                var actionRemovedVersion = actionModelRemoved ?? removedVersion;

                SetActionApiVersions(actionModel, conventionBuilder, actionIntroducedVersion, actionRemovedVersion);
            }
        }

        private void SetActionApiVersions(ActionModel action, IControllerConventionBuilder conventionBuilder, ApiVersion? includedVersion, ApiVersion? removedVersion)
        {
            var actionSupportedVersions = _allVersions.GetSupportedVersions(includedVersion, removedVersion);
            var actionDeprecatedVersions = _allVersions.GetDeprecatedVersions(includedVersion, removedVersion);

            var actionConventionBuilder = conventionBuilder.Action(action.ActionMethod);

            actionConventionBuilder.HasApiVersions(actionSupportedVersions);
            actionConventionBuilder.HasDeprecatedApiVersions(actionDeprecatedVersions);
        }

        private static void ValidateActionModel(ControllerModel controller, ActionModel action, ApiVersion? controllerIncludedVersion, ApiVersion? controllerRemovedVersion, ApiVersion? actionIncludedVersion, ApiVersion? actionRemovedVersion)
        {
            var name = $"Action ({controller.ControllerType.Name}.{action.ActionMethod.Name})";

            if (actionIncludedVersion is not null && actionIncludedVersion < controllerIncludedVersion)
            {
                throw new InvalidOperationException($"{name} version cannot be included before controller ({controller.ControllerType.Name}) version.");
            }

            if (actionIncludedVersion is not null && controllerRemovedVersion is not null && actionIncludedVersion >= controllerRemovedVersion)
            {
                throw new InvalidOperationException($"{name} version cannot be included after controller ({controller.ControllerType.Name}) is removed.");
            }

            if (actionRemovedVersion is null)
            {
                return;
            }

            var includedVersion = actionIncludedVersion ?? controllerIncludedVersion;

            if (actionRemovedVersion == includedVersion)
            {
                throw new InvalidOperationException($"{name} ApiVersion cannot be included and removed in the same version.");
            }

            if (actionRemovedVersion < includedVersion)
            {
                throw new InvalidOperationException($"{name} ApiVersion cannot be removed before it is included.");
            }

            if (controllerRemovedVersion is not null && actionRemovedVersion > controllerRemovedVersion)
            {
                throw new InvalidOperationException($"{name} version cannot be removed after controller ({controller.ControllerType.Name}) is removed.");
            }
        }

        private void SetControllerApiVersions(IControllerConventionBuilder conventionBuilder, ApiVersion? includedVersion, ApiVersion? removedVersion)
        {
            var supportedVersions = _allVersions.GetSupportedVersions(includedVersion, removedVersion);
            var deprecatedVersions = _allVersions.GetDeprecatedVersions(includedVersion, removedVersion);

            conventionBuilder.HasApiVersions(supportedVersions);
            conventionBuilder.HasDeprecatedApiVersions(deprecatedVersions);
        }
    }
}
