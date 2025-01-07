using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Anatawa12.AvatarOptimizer;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Numeira.MaterialOptimizer;

internal sealed class DeepCloner : DeepCloneHelper
{
    private readonly Func<Object, Object?> onCloneCallback;

    public DeepCloner(Func<Object, Object?> onCloneCallback)
    {
        this.onCloneCallback = onCloneCallback;
    }

    protected override Object? CustomClone(Object o)
    {
        if (o == null)
            return null;

        return onCloneCallback(o);
    }

    protected override ComponentSupport GetComponentSupport(Object o)
    {
        return o switch
        {
            Material or
            Motion or 
            AnimatorController or 
            AnimatorOverrideController or 
            AnimatorState or 
            AnimatorStateMachine or 
            AnimatorTransitionBase or 
            StateMachineBehaviour or 
            AvatarMask => ComponentSupport.Clone,
            _ => ComponentSupport.NoClone,
        };
    }
}

// https://github.com/anatawa12/AvatarOptimizer/blob/d7178a4df250dae5ef33b05ed60c8e35f49879f9/Editor/Processors/DupliacteAssets.cs#L152-L188
// Originally under MIT License
// Copyright(c) 2022 anatawa12
static class MaterialEditorReflection
{
    static MaterialEditorReflection()
    {
        DisableApplyMaterialPropertyDrawersPropertyInfo = typeof(EditorMaterialUtility).GetProperty(
            "disableApplyMaterialPropertyDrawers", BindingFlags.Static | BindingFlags.NonPublic)!;
    }

    public static readonly PropertyInfo DisableApplyMaterialPropertyDrawersPropertyInfo;

    public static DisableApplyMaterialPropertyDisposable BeginNoApplyMaterialPropertyDrawers()
    {
        return new DisableApplyMaterialPropertyDisposable(true);
    }

    public static bool DisableApplyMaterialPropertyDrawers
    {
        get => (bool)DisableApplyMaterialPropertyDrawersPropertyInfo.GetValue(null);
        set => DisableApplyMaterialPropertyDrawersPropertyInfo.SetValue(null, value);
    }

    public readonly struct DisableApplyMaterialPropertyDisposable : IDisposable
    {
        private readonly bool _originalValue;

        public DisableApplyMaterialPropertyDisposable(bool value)
        {
            _originalValue = DisableApplyMaterialPropertyDrawers;
            DisableApplyMaterialPropertyDrawers = value;
        }

        public void Dispose()
        {
            DisableApplyMaterialPropertyDrawers = _originalValue;
        }
    }
}