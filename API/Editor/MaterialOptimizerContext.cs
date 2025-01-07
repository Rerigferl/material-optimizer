using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Numeira.MaterialOptimizer.API.Internal;
using UnityEngine;

namespace Numeira.MaterialOptimizer.API;

public sealed class MaterialOptimizerContext
{
    internal MaterialOptimizerContext(List<Material> materials, List<string> animatedProperties)
    {
        this.materials = materials;
        this.animatedProperties = animatedProperties;
    }

    internal List<Material> materials;
    internal List<string> animatedProperties;

    public ReadOnlySpan<Material> Materials => materials.AsSpan();
    public ReadOnlySpan<string> AnimatedProperties => animatedProperties.AsSpan();
}
