using System;

namespace Numeira.MaterialOptimizer.API;

public static class MaterialOptimizerModuleRegistry
{
    public static void Register<TModule>(TModule module) 
        where TModule : MaterialOptimizerModule
    {
        if (module is IMaterialOptimizerModuleWithSettings @interface)
        {
            RegisterWithSettingProxy?.Invoke(module, @interface.SettingsType);
            return;
        }
        RegisterProxy?.Invoke(module);
    }

    public static void Register<TModule, TSettings>(TModule module)
        where TModule : MaterialOptimizerModule<TSettings> 
        where TSettings : MaterialOptimizerSettingsBase
    {
        RegisterWithSettingProxy?.Invoke(module, typeof(TSettings));
    }

    internal static Action<MaterialOptimizerModule>? RegisterProxy;
    internal static Action<MaterialOptimizerModule, Type>? RegisterWithSettingProxy;
}