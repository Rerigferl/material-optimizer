﻿using System.Runtime.CompilerServices;
using System;
using UnityEngine;
using System.Runtime.Remoting.Contexts;

namespace Numeira.MaterialOptimizer.API.Internal;

public static class MaterialOptimizerContextMarshal
{
    public static Material[] GetMaterialsArray(MaterialOptimizerContext context)
    {
        var (buffer, _) = context.materials;
        return buffer;
    }

    public static string[] GetAnimatedPropertiesArray(MaterialOptimizerContext context)
    {
        var (buffer, _) = context.animatedProperties;
        return buffer;
    }
}
