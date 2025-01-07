using UnityEngine;
using VRC.SDKBase;

namespace Numeira.MaterialOptimizer.API
{
    [DisallowMultipleComponent]
    public abstract class MaterialOptimizerComponent : MonoBehaviour, IEditorOnly
    {
        internal MaterialOptimizerComponent() { }
    }
}