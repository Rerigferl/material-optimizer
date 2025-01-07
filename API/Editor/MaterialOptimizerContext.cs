using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Numeira.MaterialOptimizer.API.Internal;
using UnityEngine;

namespace Numeira.MaterialOptimizer.API;

public sealed class MaterialOptimizerContext
{
    internal MaterialOptimizerContext(List<Material> materials, Dictionary<Material, List<string>> animatedPropertiesMap)
    {
        this.materials = materials;
        this.animatedPropertiesMap = animatedPropertiesMap;

    }

    internal List<Material> materials;
    internal Dictionary<Material, List<string>> animatedPropertiesMap;

    public ReadOnlySpan<Material> Materials => materials.AsSpan();

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
}
