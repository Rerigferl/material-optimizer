using System;
using lilToon;
using Numeira.MaterialOptimizer.API;
using Numeira.MaterialOptimizer.API.Internal;
using UnityEngine;

namespace Numeira.MaterialOptimizer.Modules;

[MaterialOptimizerModule]
internal sealed class LilToonModule : MaterialOptimizerModule<LilToonSettings>
{
    public override string QualifiedName => "builtin.lilToon";
    public override string DisplayName => "lilToon";

    static LilToonModule() 
        => MaterialOptimizerModuleRegistry.RegisterWithSettings(new LilToonModule());

    protected override bool OnFilterMaterial(Material material)
    {
        return lilMaterialUtils.CheckShaderIslilToon(material);
    }

    protected override void Run(MaterialOptimizerContext context)
    {
        foreach (var material in context.Materials)
        {
            var animatedProperties = MaterialOptimizerContextMarshal.GetAnimatedPropertiesArray(context, material) ?? Array.Empty<string>();
            OptimizeMaterial(material, animatedProperties);
        }
    }

    public static void OptimizeMaterial(Material material, string[] animatedProperties)
    {
        RemoveUnusedTexture(material, animatedProperties);
    }

    public static void RemoveUnusedTexture(Material material, string[] animatedProperties)
    {
        lilMaterialUtils.RemoveUnusedTextureOnly(material, material.shader.name.Contains("Lite"), animatedProperties);
    }
}