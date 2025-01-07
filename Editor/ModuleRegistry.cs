using System.Reflection;
using System.Runtime.CompilerServices;
using Numeira.MaterialOptimizer.API;
using UnityEditor;
using UnityEngine;

namespace Numeira.MaterialOptimizer;

[InitializeOnLoad]
internal static class ModuleRegistry
{

    static ModuleRegistry()
    {
        MaterialOptimizerModuleRegistry.RegisterProxy = default(object).Register;
        MaterialOptimizerModuleRegistry.RegisterWithSettingProxy = default(object).RegisterWithSettings;

        MaterialOptimizerComponent.AddSettingsComponents = default(object).AddSettingsComponent;

        foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).Where(x => x.GetCustomAttribute<MaterialOptimizerModuleAttribute>() != null))
        {
            RuntimeHelpers.RunClassConstructor(type.TypeHandle);
        }
    }

    private static readonly List<MaterialOptimizerModule> modules = new();
    private static readonly Dictionary<string, Type> settingsTypeMap = new();

    private static void Register(this object? _, MaterialOptimizerModule module) => InternalRegister(module);

    private static void RegisterWithSettings(this object? _, MaterialOptimizerModule module, Type settingsType)
    {
        if (!InternalRegister(module))
            return;
        settingsTypeMap.TryAdd(module.QualifiedName, settingsType);
    }

    private static bool InternalRegister(MaterialOptimizerModule module)
    {
        foreach (var m in modules.AsSpan())
        {
            if (module.QualifiedName == m.QualifiedName)
                return false;
        }
        modules.Add(module);

        return true;
    }

    private static void AddSettingsComponent(this object? __, MaterialOptimizerComponent component)
    {
        foreach(var type in settingsTypeMap.Values)
        {
            if (!typeof(Component).IsAssignableFrom(type))
                continue;

            if (!component.TryGetComponent(type, out _))
                component.gameObject.AddComponent(type);
        }
    }

    private static int previousModulesVersion = -1;
    public static ReadOnlySpan<MaterialOptimizerModule> Modules
    {
        get
        {
            var list = modules;
            var currentVersion = list.GetVersion();
            if (previousModulesVersion != currentVersion)
            {
                list.Sort((x, y) => x.ExecutionOrder.CompareTo(y.ExecutionOrder));
                previousModulesVersion = currentVersion;
            }
            return list.AsSpan();
        }
    }

    public static bool TryGetSettingsType(MaterialOptimizerModule module, out Type result) => settingsTypeMap.TryGetValue(module.QualifiedName, out result);
}
