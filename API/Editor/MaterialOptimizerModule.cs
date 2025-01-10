using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Numeira.MaterialOptimizer.API;

public abstract class MaterialOptimizerModule 
{
    public virtual string QualifiedName => GetType().FullName;
    public virtual string DisplayName => QualifiedName;
    public virtual int ExecutionOrder => 0;

    protected internal virtual bool OnFilterMaterial(Material material)
    {
        return false;
    }

    protected internal virtual void Run(MaterialOptimizerContext context) { }

    protected internal virtual void OnGUI(SerializedObject serializedObject) 
    {
        var iter = serializedObject.GetIterator();
        iter.NextVisible(true);
        int count = 0;
        while (iter.NextVisible(false))
        {
            EditorGUILayout.PropertyField(iter, true);
            count++;
        }
        if (count == 0)
        {
            EditorGUILayout.LabelField("No settings available.");
        }
    }

    internal MonoBehaviour? settings;
}

public abstract class MaterialOptimizerModule<TSettings> : MaterialOptimizerModule, IMaterialOptimizerModuleWithSettings where TSettings : MaterialOptimizerSettingsBase
{
    protected TSettings Settings => (settings as TSettings)!;

    Type IMaterialOptimizerModuleWithSettings.SettingsType => typeof(TSettings);
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class MaterialOptimizerModuleAttribute : Attribute { }

internal interface IMaterialOptimizerModuleWithSettings 
{
    Type SettingsType { get; }
}
