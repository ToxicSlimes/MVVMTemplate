using Core.Abstractions.AssetManagement;
using Core.Abstractions.Config;
using Core.Abstractions.MVVM.Config;
using Core.Abstractions.MVVM.Services;
using Core.Infrastructure.MVVM.Config;
using Core.Infrastructure.MVVM.Services;
using Reflex.Core;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Core.DependencyInjection.Bindings
{
    public static class ElementBindings
    {
        public static ContainerBuilder AddElementServices(this ContainerBuilder builder)
        {
            RegisterElementConfig(builder);
            RegisterToolkitElementConfig(builder);
            RegisterUIElementService(builder);
            RegisterToolkitElementService(builder);

            return builder;
        }

        private static void RegisterElementConfig(ContainerBuilder builder)
        {
            builder.AddSingleton<IElementConfig>(container =>
            {
                ElementConfig config = null;

#if UNITY_EDITOR
                config = AssetDatabase.LoadAssetAtPath<ElementConfig>("Assets/Source/00. Configs/ElementConfig.asset");
#endif

                if (config == null)
                {
                    config = Resources.Load<ElementConfig>("ElementConfig");
                }

                if (config == null)
                {
                    var configProvider = container.Resolve<IConfigProvider>();
                    config = configProvider.GetConfig<ElementConfig>();
                }

                if (config == null)
                {
                    Debug.LogError("[DI] ElementConfig not found! Create it via Assets/Create/MVVM/Element Config");
                }

                return config;
            });
        }

        private static void RegisterUIElementService(ContainerBuilder builder)
        {
            builder.AddSingleton<IUIElementService>(container =>
                new UIElementService(
                    container,
                    container.Resolve<IAssetProvider>(),
                    container.Resolve<IElementConfig>()));
        }

        private static void RegisterToolkitElementConfig(ContainerBuilder builder)
        {
            builder.AddSingleton<IToolkitElementConfig>(container =>
            {
                ToolkitElementConfig config = null;

#if UNITY_EDITOR
                config = AssetDatabase.LoadAssetAtPath<ToolkitElementConfig>("Assets/Source/00. Configs/ToolkitElementConfig.asset");
#endif

                if (config == null)
                    config = Resources.Load<ToolkitElementConfig>("ToolkitElementConfig");

                if (config == null)
                {
                    var configProvider = container.Resolve<IConfigProvider>();
                    config = configProvider.GetConfig<ToolkitElementConfig>();
                }

                if (config == null)
                    Debug.LogError("[DI] ToolkitElementConfig not found! Create it via Assets/Create/MVVM/Toolkit Element Config");

                return config;
            });
        }

        private static void RegisterToolkitElementService(ContainerBuilder builder)
        {
            builder.AddSingleton<IToolkitElementService>(container =>
                new ToolkitElementService(
                    container.Resolve<IAssetProvider>(),
                    container.Resolve<IToolkitElementConfig>()));
        }
    }
}
