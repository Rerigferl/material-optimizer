using System;
using System.Collections.Generic;
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

    internal Component? settings;
}

public abstract class MaterialOptimizerModule<TSettings> : MaterialOptimizerModule where TSettings : MaterialOptimizerSettingsBase
{
    protected TSettings Settings => (settings as TSettings)!;
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class MaterialOptimizerModuleAttribute : Attribute { }
