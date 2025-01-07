using UnityEditor;
using UnityEngine;

namespace Numeira.MaterialOptimizer;

internal sealed class Installer
{
    private const string PrefabGuid = "0bbb182836b4b48459fe3403236aeeda";

    public static GameObject? Install(GameObject parent)
    {
        if (parent == null)
            return null;

        GameObject? prefab;
        if (parent.GetComponentInChildren<MaterialOptimizerComponent>() is { } component)
        {
            prefab = component.gameObject;
        }
        else
        {
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(PrefabGuid));
            if (prefab == null)
                return null;

            prefab = PrefabUtility.InstantiatePrefab(prefab, parent.transform) as GameObject;
            Undo.RegisterCreatedObjectUndo(prefab, "Material Optimizer");
        }

        EditorGUIUtility.PingObject(prefab);
        Selection.activeObject = prefab;

        return prefab;
    }
}