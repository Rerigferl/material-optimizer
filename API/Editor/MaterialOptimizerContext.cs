using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using nadena.dev.ndmf;
using Numeira.MaterialOptimizer.API.Internal;
using UnityEditor;
using UnityEngine;

namespace Numeira.MaterialOptimizer.API;

public sealed class MaterialOptimizerContext
{
    internal MaterialOptimizerContext(List<Material> materials, Dictionary<Material, List<string>> animatedPropertiesMap, UnityEngine.Object assetContainer)
    {
        this.materials = materials;
        this.animatedPropertiesMap = animatedPropertiesMap;
        AssetContainer = assetContainer;
    }

    internal List<Material> materials;
    internal Dictionary<Material, List<string>> animatedPropertiesMap;

    public ReadOnlySpan<Material> Materials => materials.AsSpan();
    public UnityEngine.Object AssetContainer { get; }

    public bool TryGetAnimatedProperties(Material material, out ReadOnlySpan<string> animatedProperties)
    {
        if (animatedPropertiesMap.TryGetValue(material, out var list))
        {
            animatedProperties = list.AsSpan();
            return true;
        }
        animatedProperties = default;
        return false;
    }

    public T RegisterObjectCloned<T>(T original, T cloned) where T : UnityEngine.Object
    {
        AssetDatabase.AddObjectToAsset(cloned, AssetContainer);
        ObjectRegistry.RegisterReplacedObject(original, cloned);
        return cloned;
    }
}
