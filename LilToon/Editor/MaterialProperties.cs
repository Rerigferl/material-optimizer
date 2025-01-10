using lilToon;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using Numeira.MaterialOptimizer.API;

namespace Numeira.MaterialOptimizer.Modules;


internal static class MaterialProperties
{
    public static readonly MaterialPropertyInfo<Color>   MainColor                               = new("_Color");                 
    public static readonly MaterialPropertyInfo<Texture> MainTex                                 = new("_MainTex");
    public static readonly MaterialPropertyInfo<Vector4> MainTexHSVG                             = new("_MainTexHSVG");
    public static readonly MaterialPropertyInfo<float>   MainGradationStrength                   = new("_MainGradationStrength");
    public static readonly MaterialPropertyInfo<Texture> MainGradationTex                        = new("_MainGradationTex");
    public static readonly MaterialPropertyInfo<Texture> MainColorAdjustMask                     = new("_MainColorAdjustMask");

    public static readonly MaterialPropertyInfo<int>     UseMain2ndTex                           = new("_UseMain2ndTex");
    public static readonly MaterialPropertyInfo<Color>   MainColor2nd                            = new("_Color2nd");
    public static readonly MaterialPropertyInfo<Texture> Main2ndTex                              = new("_Main2ndTex");
    public static readonly MaterialPropertyInfo<float>   Main2ndTexAngle                         = new("_Main2ndTexAngle");
    public static readonly MaterialPropertyInfo<int>     Main2ndTexIsDecal                       = new("_Main2ndTexIsDecal");
    public static readonly MaterialPropertyInfo<int>     Main2ndTexIsLeftOnly                    = new("_Main2ndTexIsLeftOnly");
    public static readonly MaterialPropertyInfo<int>     Main2ndTexIsRightOnly                   = new("_Main2ndTexIsRightOnly");
    public static readonly MaterialPropertyInfo<int>     Main2ndTexShouldCopy                    = new("_Main2ndTexShouldCopy");
    public static readonly MaterialPropertyInfo<int>     Main2ndTexShouldFlipMirror              = new("_Main2ndTexShouldFlipMirror");
    public static readonly MaterialPropertyInfo<int>     Main2ndTexShouldFlipCopy                = new("_Main2ndTexShouldFlipCopy");
    public static readonly MaterialPropertyInfo<int>     Main2ndTexIsMSDF                        = new("_Main2ndTexIsMSDF");
    public static readonly MaterialPropertyInfo<Texture> Main2ndBlendMask                        = new("_Main2ndBlendMask");
    public static readonly MaterialPropertyInfo<int>     Main2ndTexBlendMode                     = new("_Main2ndTexBlendMode");
    
    public static readonly MaterialPropertyInfo<int>     UseMain3rdTex                           = new("_UseMain3rdTex");
    public static readonly MaterialPropertyInfo<Color>   MainColor3rd                            = new("_Color3rd");
    public static readonly MaterialPropertyInfo<Texture> Main3rdTex                              = new("_Main3rdTex");
    public static readonly MaterialPropertyInfo<float>   Main3rdTexAngle                         = new("_Main3rdTexAngle");
    public static readonly MaterialPropertyInfo<int>     Main3rdTexIsDecal                       = new("_Main3rdTexIsDecal");
    public static readonly MaterialPropertyInfo<int>     Main3rdTexIsLeftOnly                    = new("_Main3rdTexIsLeftOnly");
    public static readonly MaterialPropertyInfo<int>     Main3rdTexIsRightOnly                   = new("_Main3rdTexIsRightOnly");
    public static readonly MaterialPropertyInfo<int>     Main3rdTexShouldCopy                    = new("_Main3rdTexShouldCopy");
    public static readonly MaterialPropertyInfo<int>     Main3rdTexShouldFlipMirror              = new("_Main3rdTexShouldFlipMirror");
    public static readonly MaterialPropertyInfo<int>     Main3rdTexShouldFlipCopy                = new("_Main3rdTexShouldFlipCopy");
    public static readonly MaterialPropertyInfo<int>     Main3rdTexIsMSDF                        = new("_Main3rdTexIsMSDF");
    public static readonly MaterialPropertyInfo<Texture> Main3rdBlendMask                        = new("_Main3rdBlendMask");
    public static readonly MaterialPropertyInfo<int>     Main3rdTexBlendMode                     = new("_Main3rdTexBlendMode");

    public static readonly MaterialPropertyInfo<Texture> OutlineTex                              = new("_OutlineTex");

    public static readonly MaterialPropertyInfo[] MainBlock =
    {
        MainColor,
        MainTex,
        MainTexHSVG,
        MainGradationStrength,
        MainGradationTex,
        MainColorAdjustMask,
    };

    public static readonly MaterialPropertyInfo[] Main2ndBlock =
    {
        UseMain2ndTex,
        MainColor2nd,
        Main2ndTex,
        Main2ndTexAngle,
        Main2ndTexIsDecal,
        Main2ndTexIsLeftOnly,
        Main2ndTexIsRightOnly,
        Main2ndTexShouldCopy,
        Main2ndTexShouldFlipMirror,
        Main2ndTexShouldFlipCopy,
        Main2ndTexIsMSDF,
        Main2ndBlendMask,
        Main2ndTexBlendMode,
    };

    public static readonly MaterialPropertyInfo[] Main3rdBlock =
    {
        UseMain3rdTex,
        MainColor3rd,
        Main3rdTex,
        Main3rdTexAngle,
        Main3rdTexIsDecal,
        Main3rdTexIsLeftOnly,
        Main3rdTexIsRightOnly,
        Main3rdTexShouldCopy,
        Main3rdTexShouldFlipMirror,
        Main3rdTexShouldFlipCopy,
        Main3rdTexIsMSDF,
        Main3rdBlendMask,
        Main3rdTexBlendMode,
    };
}