using FluentAssertions;
using JM.Asp.Versioning.Attributes;
using JM.Asp.Versioning.ConventionBuilder;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Reflection;

namespace JM.Asp.Versioning.Tests.ConventionBuilder
{
    [TestClass]
    public class LifeCycleApiVersionConventionBuilderTests
    {
        [ApiVersionInclude(2.0)]
        [ApiVersionRemove(2.0)]
        private class ControllerIncludedAndRemovedInSameVersion
        {
            public void Action() { }
        }

        [ApiVersionInclude(2.0)]
        [ApiVersionRemove(1.0)]
        private class ControllerRemovedBeforeIncluded
        {
            public void Action() { }
        }

        [ApiVersionInclude(2.0)]
        private class ActionIncludedBeforeController
        {
            [ApiVersionInclude(1.0)]
            public void Action() { }
        }

        [ApiVersionInclude(1.0)]
        [ApiVersionRemove(2.0)]
        private class ActionIncludedAfterControllerRemoved
        {
            [ApiVersionInclude(3.0)]
            public void Action() { }
        }

        [ApiVersionInclude(2.0)]
        private class ActionRemovedInIncludedVersion
        {
            [ApiVersionRemove(2.0)]
            public void Action() { }
        }

        [ApiVersionInclude(2.0)]
        private class ActionRemovedBeforeIncluded
        {
            [ApiVersionRemove(1.0)]
            public void Action() { }
        }

        [ApiVersionInclude(1.0)]
        [ApiVersionRemove(2.0)]
        private class ActionRemovedAfterControllerRemoved
        {
            [ApiVersionRemove(3.0)]
            public void Action() { }
        }

        [TestMethod]
        [DataRow(typeof(ControllerIncludedAndRemovedInSameVersion), "*cannot be included and removed in the same version*")]
        [DataRow(typeof(ControllerRemovedBeforeIncluded), "*cannot be removed before it is included*")]
        [DataRow(typeof(ActionIncludedBeforeController), "*Action*cannot be included before controller*")]
        [DataRow(typeof(ActionIncludedAfterControllerRemoved), "*Action*cannot be included after controller*")]
        [DataRow(typeof(ActionRemovedInIncludedVersion), "*Action*cannot be included and removed in the same version*")]
        [DataRow(typeof(ActionRemovedBeforeIncluded), "*Action*cannot be removed before it is included*")]
        [DataRow(typeof(ActionRemovedAfterControllerRemoved), "*Action*cannot be removed after controller*")]
        public void ApplyTo_InvalidVersions_Throws(Type controllerType, string expectedMessage)
        {
            var builder = new LifeCycleApiVersionConventionBuilder(1, 3);

            var act = () => builder.ApplyTo(CreateModel(controllerType));

            act.Should().Throw<InvalidOperationException>().WithMessage(expectedMessage);
        }

        private static ControllerModel CreateModel(Type type)
        {
            var controller = new ControllerModel(type.GetTypeInfo(), type.GetCustomAttributes(inherit: false))
            {
                ControllerName = type.Name
            };

            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                controller.Actions.Add(new ActionModel(method, method.GetCustomAttributes(inherit: false))
                {
                    ActionName = method.Name,
                    Controller = controller
                });
            }

            return controller;
        }
    }
}
