using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Core.Mediator.Registration;
using Reflex.Core;

namespace Core.DependencyInjection
{
    /// <summary>
    /// Extends the package composition root without creating a second mediator.
    /// Consumer-owned generated registrations implement this interface.
    /// </summary>
    public interface ICoreRegistrationContributor
    {
        void RegisterBindings(ContainerBuilder builder);
        void RegisterMediator(MediatorBuilder builder);
    }

    public static class CoreRegistrationContributors
    {
        public static IReadOnlyList<ICoreRegistrationContributor> Discover()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => !assembly.IsDynamic && !IsTestAssembly(assembly))
                .SelectMany(GetLoadableTypes)
                .Where(type => type != null &&
                               !type.IsAbstract &&
                               !type.IsInterface &&
                               typeof(ICoreRegistrationContributor).IsAssignableFrom(type) &&
                               type.GetConstructor(Type.EmptyTypes) != null)
                .OrderBy(type => type.FullName, StringComparer.Ordinal)
                .Select(CreateContributor)
                .Where(contributor => contributor != null)
                .ToArray();
        }

        public static void ApplyBindings(
            ContainerBuilder builder,
            IEnumerable<ICoreRegistrationContributor> contributors)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (contributors == null)
                throw new ArgumentNullException(nameof(contributors));

            foreach (var contributor in contributors)
                contributor.RegisterBindings(builder);
        }

        public static void ApplyMediator(
            MediatorBuilder builder,
            IEnumerable<ICoreRegistrationContributor> contributors)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (contributors == null)
                throw new ArgumentNullException(nameof(contributors));

            foreach (var contributor in contributors)
                contributor.RegisterMediator(builder);
        }

        private static ICoreRegistrationContributor CreateContributor(Type type)
        {
            try
            {
                return (ICoreRegistrationContributor)Activator.CreateInstance(type);
            }
            catch
            {
                return null;
            }
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                return exception.Types.Where(type => type != null);
            }
            catch
            {
                return Array.Empty<Type>();
            }
        }

        private static bool IsTestAssembly(Assembly assembly)
        {
            var name = assembly.GetName().Name;
            return name.Contains("Tests") ||
                   name.Contains("Test") ||
                   name.Contains("nunit") ||
                   name.Contains("NSubstitute");
        }
    }
}
