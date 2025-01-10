using System;
using UnityEngine;
using VRC.SDKBase;

namespace Numeira.MaterialOptimizer
{
    [DisallowMultipleComponent]
    [AddComponentMenu("NDMF/Material Optimizer")]
    [ExecuteInEditMode]
    internal sealed class MaterialOptimizerComponentProxy : MonoBehaviour, IEditorOnly
    {
        public void Awake()
        {
            bool success = InternalTryAddMaterialOptimizerToAvatar?.Invoke(this) ?? false;

            UnityEngine.Object destroyTarget = this;
            if (success && this.GetComponents<Component>().Length <= 2)
                destroyTarget = this.gameObject;

            UnityEngine.Object.DestroyImmediate(destroyTarget);
        }

        internal static Func<Component, bool>? InternalTryAddMaterialOptimizerToAvatar;
    }
}