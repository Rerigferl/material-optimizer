using System;

namespace Numeira.MaterialOptimizer.API;

public static class MaterialOptimizerModuleRegistry
{
    public static void Register(MaterialOptimizerModule module)
    {
        RegisterProxy?.Invoke(module);
    }

    public static void RegisterWithSettings<TSettings>(MaterialOptimizerModule<TSettings> module) where TSettings : MaterialOptimizerSettingsBase
    {
        RegisterWithSettingProxy?.Invoke(module, typeof(TSettings));
    }

    internal static Action<MaterialOptimizerModule>? RegisterProxy;
    internal static Action<MaterialOptimizerModule, Type>? RegisterWithSettingProxy;
}