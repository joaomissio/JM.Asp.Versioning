using Asp.Versioning;
using Asp.Versioning.Conventions;
using JM.Asp.Versioning.Extensions;
using JM.Asp.Versioning.Internal;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
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

        public LifeCycleApiVersionConventionBuilder(Assembly assembly)
            : this(new ApiVersions(assembly), new ApiVersionConventionBuilder())
        {
        }

        public LifeCycleApiVersionConventionBuilder(int startVersion, int currentVersion)
            : this(new ApiVersions(startVersion, currentVersion), new ApiVersionConventionBuilder())
        {
        }

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

                ValidateActionModel(controller, includedVersion, actionModelIntroduced, actionModel);

                var actionIntroducedVersion = actionModelIntroduced ?? includedVersion;
                var actionRemovedVersion = actionModel.GetRemovedVersion() ?? removedVersion;

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

        private static void ValidateActionModel(ControllerModel controller, ApiVersion? controllerIncludedVersion, ApiVersion? actionIncludedVersion, ActionModel action)
        {
            if (actionIncludedVersion is not null && actionIncludedVersion < controllerIncludedVersion)
            {
                throw new InvalidOperationException($"Action ({action.ActionName}) version cannot be included before controller ({controller.ControllerName}) version.");
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