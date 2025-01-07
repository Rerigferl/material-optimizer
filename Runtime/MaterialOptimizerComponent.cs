using System;
using UnityEngine;

namespace Numeira.MaterialOptimizer
{
    [DisallowMultipleComponent]
    [AddComponentMenu("/Material Optimizer")]
    [ExecuteInEditMode]
    internal sealed class MaterialOptimizerComponent : API.MaterialOptimizerComponent
    {
        public void OnValidate()
        {
            InternalOnValidate?.Invoke(this);
        }

        public void Reset()
        {
            InternalReset?.Invoke(this);
            OnValidate();
        }

        internal static Action<MaterialOptimizerComponent>? InternalOnValidate;
        internal static Action<MaterialOptimizerComponent>? InternalReset;
    }
}