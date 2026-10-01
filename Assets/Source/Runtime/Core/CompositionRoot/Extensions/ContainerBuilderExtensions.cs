using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Reflex.Core;

namespace Core.DependencyInjection.Extensions
{
    /// <summary>
    /// Extension методы для Reflex ContainerBuilder для автоматической регистрации типов.
    /// </summary>
    public static class ContainerBuilderExtensions
    {
        /// <summary>
        /// Автоматически находит и регистрирует все неабстрактные классы, реализующие TInterface,
        /// из всех загруженных сборок, которые ссылаются на сборку интерфейса, как Singleton.
        /// </summary>
        /// <typeparam name="TInterface">Интерфейс/контракт для поиска реализаций</typeparam>
        /// <param name="builder">Экземпляр ContainerBuilder</param>
        /// <param name="assembly">Опционально: конкретная сборка для сканирования. По умолчанию - все релевантные сборки.</param>
        /// <returns>ContainerBuilder для цепочки вызовов</returns>
        /// <example>
        /// builder.AddAllImplementationsOf&lt;IGameState&gt;();
        /// </example>
        public static ContainerBuilder AddAllImplementationsOf<TInterface>(
            this ContainerBuilder builder,
            Assembly assembly = null)
        {
            return builder.AddAllImplementationsOf(typeof(TInterface), assembly);
        }

        /// <summary>
        /// Автоматически находит и регистрирует все неабстрактные классы, реализующие указанный интерфейс,
        /// из всех загруженных сборок, которые ссылаются на сборку интерфейса, как Singleton.
        /// </summary>
        /// <param name="builder">Экземпляр ContainerBuilder</param>
        /// <param name="interfaceType">Интерфейс/контракт для поиска реализаций</param>
        /// <param name="assembly">Опционально: конкретная сборка для сканирования. По умолчанию - все релевантные сборки.</param>
        /// <returns>ContainerBuilder для цепочки вызовов</returns>
        public static ContainerBuilder AddAllImplementationsOf(
            this ContainerBuilder builder,
            Type interfaceType,
            Assembly assembly = null)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            
            if (interfaceType == null)
                throw new ArgumentNullException(nameof(interfaceType));

            IEnumerable<Type> implementations;
            
            if (assembly != null)
            {
                implementations = FindImplementations(interfaceType, assembly);
            }
            else
            {
                var interfaceAssembly = interfaceType.Assembly;
                var relevantAssemblies = GetRelevantAssemblies(interfaceAssembly);
                implementations = relevantAssemblies
                    .SelectMany(asm => FindImplementations(interfaceType, asm))
                    .Distinct()
                    .ToList();
            }

            foreach (var implementationType in implementations)
            {
                builder.AddSingleton(implementationType, interfaceType);
            }

            return builder;
        }

        /// <summary>
        /// Находит все конкретные (неабстрактные, не-интерфейсы) типы, реализующие указанный интерфейс.
        /// </summary>
        private static IEnumerable<Type> FindImplementations(Type interfaceType, Assembly assembly)
        {
            try
            {
                return assembly.GetTypes()
                    .Where(type => 
                        type.IsClass &&
                        !type.IsAbstract &&
                        !type.IsInterface &&
                        !type.IsGenericTypeDefinition &&
                        interfaceType.IsAssignableFrom(type))
                    .ToList();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types
                    .Where(type => type != null &&
                        type.IsClass &&
                        !type.IsAbstract &&
                        !type.IsInterface &&
                        !type.IsGenericTypeDefinition &&
                        interfaceType.IsAssignableFrom(type))
                    .ToList();
            }
        }

        /// <summary>
        /// Получает все загруженные сборки, которые ссылаются на указанную сборку, включая саму сборку.
        /// </summary>
        private static IEnumerable<Assembly> GetRelevantAssemblies(Assembly interfaceAssembly)
        {
            var interfaceAssemblyName = interfaceAssembly.GetName();
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            
            var relevantAssemblies = new List<Assembly> { interfaceAssembly };
            
            foreach (var assembly in loadedAssemblies)
            {
                if (assembly == interfaceAssembly)
                    continue;
                    
                try
                {
                    var referencedAssemblies = assembly.GetReferencedAssemblies();
                    if (referencedAssemblies.Any(refAsm => 
                        refAsm.Name == interfaceAssemblyName.Name && 
                        refAsm.Version == interfaceAssemblyName.Version))
                    {
                        relevantAssemblies.Add(assembly);
                    }
                }
                catch
                {
                }
            }
            
            return relevantAssemblies;
        }
    }
}
