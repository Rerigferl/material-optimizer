using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Anatawa12.AvatarOptimizer;
using nadena.dev.ndmf;
using Unity.Collections;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.Rendering;

namespace Numeira.MaterialOptimizer.API.Utils;

public static class TextureUtil
{
    [MenuItem("Tests/Load PSD Image")]
    public static void Test()
    {
    }

    public static Texture2D LoadOriginalTexture(Texture2D texture)
    {
        if (texture == null || AssetDatabase.GetAssetPath(texture) is not { } path)
            return texture!;

        var ext = Path.GetExtension(path.AsSpan()).TrimStart('.'); // \('.' )/
        if (ext.Equals("psd", StringComparison.OrdinalIgnoreCase))
        {
            var result = PsdUtil.LoadImage(path);
            return result!;
        }

        if (!IsUnityTexture2DLoadableExtension(ext))
            return texture;

        {
            var result = new Texture2D(4096, 4096);
            result.LoadImage(File.ReadAllBytes(path), true);
            return result;
        }
    }

    public static bool IsLoadabableExtension(ReadOnlySpan<char> extension)
    {
        return extension.Equals("psd", StringComparison.OrdinalIgnoreCase) ||
               IsUnityTexture2DLoadableExtension(extension);
    }

    private static bool IsUnityTexture2DLoadableExtension(ReadOnlySpan<char> extension)
    {
        return extension.Equals("png", StringComparison.OrdinalIgnoreCase) ||
               extension.Equals("jpg", StringComparison.OrdinalIgnoreCase) ||
               extension.Equals("jpeg", StringComparison.OrdinalIgnoreCase);
    }

    public static Texture2D TextureBake(Material material, Texture? targetTexture = null, Vector2Int? size = null)
    {
        targetTexture ??= material.mainTexture;
        if (targetTexture == null)
            targetTexture = Texture2D.whiteTexture;

        var (width, height) = size is { } s ? (s.x, s.y) : (targetTexture.width, targetTexture.height);
        
        var result = new Texture2D(width, height, TextureFormat.RGBA32, false);

        var activeRT = RenderTexture.active;
        var tempRT = RenderTexture.GetTemporary(width, height);
        try
        {
            Graphics.Blit(targetTexture, tempRT, material);
            var request = AsyncGPUReadback.Request(tempRT, 0, TextureFormat.RGBA32);
            request.WaitForCompletion();
            using var data = request.GetData<Color>();
            result.LoadRawTextureData(data);
            result.Apply();
        }
        finally
        {
            RenderTexture.active = activeRT;
            RenderTexture.ReleaseTemporary(tempRT);
        }

        return result;
    }

    public static bool ReplaceTextures(Material material, Texture source, Texture destination)
    {
        using (var so = new SerializedObject(material))
        {
            foreach (var prop in so.ObjectReferenceProperties())
            {
                if (prop.objectReferenceValue == source)
                    prop.objectReferenceValue = destination;
            }

            return so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}