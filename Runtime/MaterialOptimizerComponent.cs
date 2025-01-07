using System;
using Numeira.MaterialOptimizer.API;
using UnityEngine;

namespace Numeira.MaterialOptimizer
{
    [DisallowMultipleComponent]
    [AddComponentMenu("NDMF/Material Optimizer")]
    [ExecuteInEditMode]
    internal sealed class MaterialOptimizerComponent : API.MaterialOptimizerComponent
    {
        public void OnValidate()
        {
            AddSettingsComponents?.Invoke(this);
        }

        internal static Action<MaterialOptimizerComponent>? AddSettingsComponents;
    }
}