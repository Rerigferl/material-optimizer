using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Numeira.MaterialOptimizer.API;

public abstract class MaterialPropertyInfo
{
    protected static Material? targetMaterial;
    public static void UpdateMaterial(Material material) => targetMaterial = material;

    public string Name { get; }
    public ShaderPropertyType PropertyType { get; }

    public MaterialPropertyInfo(string name, ShaderPropertyType propertyType)
    {
        Name = name;
        PropertyType = propertyType;
    }

    public void CopyTo(Material material, string? name = null)
    {
        if (targetMaterial == null)
            return;

        name ??= Name;

        switch (PropertyType)
        {
            case ShaderPropertyType.Float or ShaderPropertyType.Range:
                material.SetFloat(name, targetMaterial.GetFloat(Name));
                break;
            case ShaderPropertyType.Int:
                material.SetInt(name, targetMaterial.GetInt(Name));
                break;
            case ShaderPropertyType.Color:
                material.SetColor(name, targetMaterial.GetColor(Name));
                break;
            case ShaderPropertyType.Vector:
                material.SetVector(name, targetMaterial.GetVector(Name));
                break;
            case ShaderPropertyType.Texture:
                material.SetTexture(name, targetMaterial.GetTexture(Name));
                break;
        }

    }

    public void Reset()
    {
        switch ((this, PropertyType))
        {
            case (MaterialPropertyInfo<float> x, ShaderPropertyType.Float or ShaderPropertyType.Range):
                x.Value = x.DefaultValue;
                break;
            case (MaterialPropertyInfo<int> x, ShaderPropertyType.Int):
                x.Value = x.DefaultValue;
                break;
            case (MaterialPropertyInfo<Color> x, ShaderPropertyType.Color):
                x.Value = x.DefaultValue;
                break;
            case (MaterialPropertyInfo<Vector4> x, ShaderPropertyType.Vector):
                x.Value = x.DefaultValue;
                break;
            case (MaterialPropertyInfo<Texture> x, ShaderPropertyType.Texture):
                x.Value = null;
                break;
        }
    }

    public bool IsValueChanged => (this, PropertyType) switch
    {
        (MaterialPropertyInfo<float> x, ShaderPropertyType.Float or ShaderPropertyType.Range) => x.Value != x.DefaultValue,
        (MaterialPropertyInfo<int> x, ShaderPropertyType.Int) => x.Value != x.DefaultValue,
        (MaterialPropertyInfo<Color> x, ShaderPropertyType.Color) => x.Value != x.DefaultValue,
        (MaterialPropertyInfo<Vector4> x, ShaderPropertyType.Vector) => x.Value != x.DefaultValue,
        (MaterialPropertyInfo<Texture> x, ShaderPropertyType.Texture) => x.Value != x.DefaultValue,
        _ => false,
    };
}

public sealed class MaterialPropertyInfo<T> : MaterialPropertyInfo
{
    public MaterialPropertyInfo(string name) : base(name, DefaultPropertyType)
    {
    }

    public static ShaderPropertyType DefaultPropertyType =>
        typeof(T) == typeof(float)   ? ShaderPropertyType.Float :
        typeof(T) == typeof(int)     ? ShaderPropertyType.Int :
        typeof(T) == typeof(Color)   ? ShaderPropertyType.Color :
        typeof(T) == typeof(Vector4) ? ShaderPropertyType.Vector :
        typeof(T) == typeof(Texture) ? ShaderPropertyType.Texture :
        throw new ArgumentException($"Type {typeof(T)} is not supported.");

    public T? Value
    {
        get
        {
            if (targetMaterial == null)
                return default;

            if (typeof(T) == typeof(float) && (PropertyType is ShaderPropertyType.Float or ShaderPropertyType.Range))
            {
                return (T)(object)targetMaterial.GetFloat(Name);
            }
            if (typeof(T) == typeof(int))
            {
                return (T)(object)targetMaterial.GetInt(Name);
            }
            if (typeof(T) == typeof(Color))
            {
                return (T)(object)targetMaterial.GetColor(Name);
            }
            if (typeof(T) == typeof(Vector4))
            {
                return (T)(object)targetMaterial.GetVector(Name);
            }
            if (typeof(T) == typeof(Texture))
            {
                return (T)(object)targetMaterial.GetTexture(Name);
            }

            return default;
        }
        set
        {
            if (targetMaterial == null)
                return;

            switch((value, PropertyType))
            {
                case (float x, ShaderPropertyType.Range or ShaderPropertyType.Float):
                    targetMaterial.SetFloat(Name, x);
                    break;

                case (int x, ShaderPropertyType.Int):
                    targetMaterial.SetInt(Name, x);
                    break;

                case (Color x, ShaderPropertyType.Color):
                    targetMaterial.SetColor(Name, x);
                    break;

                case (Vector4 x, ShaderPropertyType.Vector):
                    targetMaterial.SetVector(Name, x);
                    break;

                case (Texture x, ShaderPropertyType.Texture):
                    targetMaterial.SetTexture(Name, x);
                    break;
            }
        }
    }

    public T? DefaultValue
    {
        get
        {
            if (targetMaterial == null || targetMaterial.shader == null)
                return default;

            var shader = targetMaterial.shader;
            var id = shader.FindPropertyIndex(Name);
            if (id == -1)
                return default;

            if (typeof(T) == typeof(float) && (PropertyType is ShaderPropertyType.Float or ShaderPropertyType.Range))
            {
                return (T)(object)shader.GetPropertyDefaultFloatValue(id);
            }
            if (typeof(T) == typeof(int) && PropertyType is ShaderPropertyType.Int)
            {
                return (T)(object)shader.GetPropertyDefaultIntValue(id);
            }
            if (typeof(T) == typeof(Color) && PropertyType is ShaderPropertyType.Color)
            {
                return (T)(object)(Color)shader.GetPropertyDefaultVectorValue(id);
            }
            if (typeof(T) == typeof(Vector4) && PropertyType is ShaderPropertyType.Vector)
            {
                return (T)(object)shader.GetPropertyDefaultVectorValue(id);
            }
            if (typeof(T) == typeof(Texture) && PropertyType is ShaderPropertyType.Texture)
            {
                return default;
                /*
                var textureName = shader.GetPropertyTextureDefaultName(id);
                return (T)(object)(textureName switch
                {
                    "white" => Texture2D.whiteTexture,
                    "black" => Texture2D.blackTexture,
                    "red" => Texture2D.redTexture,
                    _ => Texture2D.grayTexture,
                });
                */
            }

            return default;
        }
    }
}