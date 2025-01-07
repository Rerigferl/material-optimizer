using UnityEngine;
using VRC.SDKBase;

namespace Numeira.MaterialOptimizer.API
{
    [AddComponentMenu("")]
    [RequireComponent(typeof(MaterialOptimizerComponent))]
    public abstract class MaterialOptimizerSettingsBase : MonoBehaviour, IEditorOnly
    {
        private void OnEnable() { }

        [SerializeField]
        [HideInInspector]
        internal bool isExpanded = true;
    }
}