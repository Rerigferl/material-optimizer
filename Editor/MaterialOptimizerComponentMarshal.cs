using nadena.dev.ndmf.runtime;
using Numeira.MaterialOptimizer.API;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Numeira.MaterialOptimizer;

[InitializeOnLoad]
internal static class MaterialOptimizerComponentMarshal
{
    static MaterialOptimizerComponentMarshal()
    {
        MaterialOptimizerComponent.InternalOnValidate = default(object).OnValidate;
        MaterialOptimizerComponent.InternalReset = default(object).Reset;

        MaterialOptimizerComponentProxy.InternalTryAddMaterialOptimizerToAvatar = default(object).TryAddMaterialOptimizerToAvatar;
    }

    private static void OnValidate(this object? __, MaterialOptimizerComponent @this)
    {
        var avatar = RuntimeUtil.FindAvatarInParents(@this.transform);
        if (avatar == null)
            return;

        foreach(var module in ModuleRegistry.Modules)
        {
            ModuleRegistry.GetOrAddModuleSettings(@this, module);
        }
    }

    private static void Reset(this object? __, MaterialOptimizerComponent @this)
    {
        if (@this == null)
            return;

        foreach(var settings in @this.GetComponents<MaterialOptimizerSettingsBase>())
        {
            Object.DestroyImmediate(settings);
        }
    }

    private static bool TryAddMaterialOptimizerToAvatar(this object? __, Component @this)
    {
        var avatar = RuntimeUtil.FindAvatarInParents(@this.transform);
        if (avatar == null)
            return false;

        Installer.Install(avatar.gameObject);
        return true;
    }
}
