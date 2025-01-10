using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using lilToon;
using Numeira.MaterialOptimizer.API;
using Numeira.MaterialOptimizer.API.Internal;
using Numeira.MaterialOptimizer.API.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Numeira.MaterialOptimizer.Modules;

/*
 * Some of the code is quoted and edited from lilToon.
 * https://github.com/lilxyzw/lilToon/blob/ae6a81da053e8cd001f089fd02cc6402afab5701/Assets/lilToon/Editor/lilInspector.cs
 * 
 * MIT License
 * Copyright (c) 2020-2024 lilxyzw
 * 
 */

[MaterialOptimizerModule]
internal sealed class LilToonModule : MaterialOptimizerModule<LilToonSettings>
{
    public override string QualifiedName => "builtin.lilToon";
    public override string DisplayName => "lilToon";

    static LilToonModule() 
        => MaterialOptimizerModuleRegistry.Register<LilToonModule, LilToonSettings>(new());

    protected override bool OnFilterMaterial(Material material)
    {
        return lilMaterialUtils.CheckShaderIslilToon(material);
    }

    protected override void Run(MaterialOptimizerContext context)
    {
        Dictionary<Texture, int> textureUsageCount = new();
        foreach (var material in context.Materials)
        {
            var animatedProperties = MaterialOptimizerContextMarshal.GetAnimatedPropertiesArray(context, material) ?? Array.Empty<string>();
            OptimizeMaterial(context, material, animatedProperties);
        }
        
    }

    public static void OptimizeMaterial(MaterialOptimizerContext context, Material material, string[] animatedProperties)
    {
        lilMaterialUtils.RemoveUnusedTexture(material, material.shader.name.Contains("Lite"), animatedProperties);
        BakeTexture(context, material, animatedProperties);
        lilMaterialUtils.RemoveUnusedTextureOnly(material, material.shader.name.Contains("Lite"), animatedProperties);
    }

    public static void RemoveUnusedTexture(Material material, string[] animatedProperties)
    {
        lilMaterialUtils.RemoveUnusedTextureOnly(material, material.shader.name.Contains("Lite"), animatedProperties);
    }

    public static void BakeTexture(MaterialOptimizerContext context, Material material, string[] animatedProperties)
    {
        var ltsBaker = Shader.Find("Hidden/ltsother_baker");
        if (ltsBaker == null || material.mainTexture is not { } mainTex)
            return;

        // MainTexture関連をアニメーションしてたら焼き込み自体をやめる
        foreach (var x in animatedProperties)
        {
            if (x == null) continue;
            if (x == "_Color" || x.StartsWith("_MainTex") || x.StartsWith("_MainGradation"))
                return;
        }

        // MainTexを他の場所(Outline以外)で使ってたらやめる
        foreach (var name in material.GetTexturePropertyNames())
        {
            if (name == "_BaseMap" || name == "_BaseColor" || name == "_BaseColorMap" || name == MaterialProperties.MainTex.Name || name == MaterialProperties.OutlineTex.Name)
                continue;

            if (material.GetTexture(name) == mainTex)
                return;
        }

        var baker = new Material(ltsBaker);
        try
        {
            MaterialPropertyInfo.UpdateMaterial(material);

            if (!TrySetBakeMainTex(material, baker, animatedProperties) &&
                !TrySetBakeMain2ndTex(material, baker, animatedProperties) &&
                !TrySetBakeMain3rdTex(material, baker, animatedProperties))
                return;

            foreach (var property in MaterialProperties.MainBlock)
            {
                property.CopyTo(baker);
                property.Reset();
            }

            var format = (mainTex as Texture2D)?.format ?? TextureFormat.DXT1;
            var size = new Vector2Int(mainTex.width, mainTex.height);

            foreach(var id in baker.GetTexturePropertyNameIDs())
            {
                var texture = baker.GetTexture(id);
                if (texture == null || texture is not Texture2D tex2d) continue;
                texture = TextureUtil.LoadOriginalTexture(tex2d);
                baker.SetTexture(id, texture);
            }

            var baked = TextureUtil.TextureBake(baker, size: size);
            baked.name = $"{mainTex.name} Baked";
            context.RegisterObjectCloned(mainTex, baked);
            EditorUtility.CompressTexture(baked, format, TextureCompressionQuality.Normal);
            TextureUtil.ReplaceTextures(material, mainTex, baked);
        }
        finally
        {
            Object.DestroyImmediate(baker);
        }
    }

    private static bool TrySetBakeMainTex(Material material, Material bakeMat, string[] animatedProperties)
    {
        if (!MaterialProperties.MainBlock.Where(x => x.PropertyType != UnityEngine.Rendering.ShaderPropertyType.Texture).Any(x => x.IsValueChanged))
            return false;

        return true;
    }

    private static bool TrySetBakeMain2ndTex(Material material, Material bakeMat, string[] animatedProperties)
    {
        if (animatedProperties.ContainsAny("Main2nd") || animatedProperties.Contains("_Color2nd") || material.GetFloat("_UseMain2ndTex") == 0)
            return false;

        foreach(var property in MaterialProperties.Main2ndBlock)
        {
            property.CopyTo(bakeMat);
        }

        bakeMat.SetTextureOffset(MaterialProperties.Main2ndTex.Name, material.GetTextureOffset(MaterialProperties.Main2ndTex.Name));
        bakeMat.SetTextureScale (MaterialProperties.Main2ndTex.Name, material.GetTextureScale (MaterialProperties.Main2ndTex.Name));

        bakeMat.SetTextureOffset(MaterialProperties.Main2ndBlendMask.Name, material.GetTextureOffset(MaterialProperties.Main2ndBlendMask.Name));
        bakeMat.SetTextureScale (MaterialProperties.Main2ndBlendMask.Name, material.GetTextureScale (MaterialProperties.Main2ndBlendMask.Name));

        MaterialProperties.UseMain2ndTex.Value = 0;

        return true;
    }

    private static bool TrySetBakeMain3rdTex(Material material, Material bakeMat, string[] animatedProperties)
    {
        if (animatedProperties.ContainsAny("Main2nd") || animatedProperties.Contains("_Color2nd") || material.GetFloat("_UseMain2ndTex") == 0)
            return false;

        foreach (var property in MaterialProperties.Main2ndBlock)
        {
            property.CopyTo(bakeMat);
        }

        bakeMat.SetTextureOffset(MaterialProperties.Main2ndTex.Name, material.GetTextureOffset(MaterialProperties.Main2ndTex.Name));
        bakeMat.SetTextureScale(MaterialProperties.Main2ndTex.Name, material.GetTextureScale(MaterialProperties.Main2ndTex.Name));

        bakeMat.SetTextureOffset(MaterialProperties.Main2ndBlendMask.Name, material.GetTextureOffset(MaterialProperties.Main2ndBlendMask.Name));
        bakeMat.SetTextureScale(MaterialProperties.Main2ndBlendMask.Name, material.GetTextureScale(MaterialProperties.Main2ndBlendMask.Name));

        MaterialProperties.UseMain2ndTex.Value = 0;

        return true;
    }
}

internal static class Ext
{
    public static bool ContainsAny(this string[] values, string value, StringComparison comparison = StringComparison.Ordinal)
        => ((ReadOnlySpan<string>)values.AsSpan()).ContainsAny(value, comparison);

    public static bool ContainsAny(this ReadOnlySpan<string> values, string value, StringComparison comparison = StringComparison.Ordinal)
    {
        foreach(var x in values)
        {
            if (x == null)
                continue;
            
            if (x.Contains(value, comparison))
            {
                return true;
            }
        }
        return false;
    }
}