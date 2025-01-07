using System.Reflection;
using Anatawa12.AvatarOptimizer;
using Numeira.MaterialOptimizer;
using Numeira.MaterialOptimizer.API;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using VRC.Dynamics;

[assembly: ExportsPlugin(typeof(MaterialOptimizerPluginDefinition))]

namespace Numeira.MaterialOptimizer;

internal sealed class MaterialOptimizerPluginDefinition : Plugin<MaterialOptimizerPluginDefinition>
{
    public override string QualifiedName => "numeira.material-optimizer";
    public override string DisplayName => "Material Optimizer";

    protected override void Configure()
    {
        var sequence = InPhase(BuildPhase.Optimizing);
        sequence = sequence.Run(Initializer.Instance).Then;
        foreach(var module in ModuleRegistry.Modules)
        {
            sequence = sequence.Run($"{DisplayName}: {module.DisplayName}", context =>
            {
                var state = context.GetState<SharedState>();
                if (state.Component == null || !state.ModulesMap.TryGetValue(module, out var list))
                    return;

                if (ModuleRegistry.TryGetSettingsType(module, out var type))
                {
                    module.settings = state.Component.GetComponent(type) ?? state.Component.gameObject.AddComponent(type);
                }
                
                try
                {
                    module.Run(new MaterialOptimizerContext(list, state.AnimatedProperties));
                }
                catch (Exception exception)
                {
                    Debug.LogError($"[{DisplayName}] Module \"{module.DisplayName}\" throws {exception.GetType().Name}.\n{exception.Message}\n\n{exception.StackTrace}");
                }
            }).Then;
        }
    }

    private sealed class SharedState
    {
        public MaterialOptimizerComponent? Component;
        public Dictionary<Material, Material> ClonedMaterials = new();
        public Dictionary<MaterialOptimizerModule, List<Material>> ModulesMap = new();
        public List<string> AnimatedProperties = new();
        public MaterialOptimizerContext? Context;
    }

    private sealed class Initializer : Pass<Initializer>
    {
        public override string DisplayName => $"{MaterialOptimizerPluginDefinition.Instance.DisplayName}: Initializing";

        protected override void Execute(BuildContext context)
        {
            var state = context.GetState<SharedState>(_ => new());

            if (!TrySetComponentToState(context, state))
                return;

            if (!TryEnumerateAllIncludeAssets(context, state))
                return;
        }

        private static bool TrySetComponentToState(BuildContext context, SharedState state)
        {
            var component = context.AvatarRootObject.GetComponentInChildren<MaterialOptimizerComponent>(true);

            if (component == null || !component.enabled || component.CompareTag("EditorOnly"))
            {
                return false;
            }

            state.Component = component;
            return true;
        }

        private static bool TryEnumerateAllIncludeAssets(BuildContext context, SharedState state)
        {
            var cloner = new DeepCloner(x => x switch
            {
                Material mat => OnCloneMaterial(mat, context, state),
                AnimationClip anim => OnCloneAnimationClip(anim, context, state),
                _ => null,
            });

            var components = context.AvatarRootObject.GetComponentsInChildren<Component>(true);

            foreach (var component in components)
            {
                switch (component)
                {
                    // skip some known unrelated components
                    case Transform:
                    case ParticleSystem:
                    case VRCConstraintBase:
                    case VRCPhysBoneBase:
                    case IConstraint:
                        break;
                    case Renderer renderer:
                        {
                            var mats = renderer.sharedMaterials;
                            foreach (ref var mat in mats.AsSpan())
                            {
                                if (mat != null)
                                    mat = cloner.MapObject(mat);
                            }
                            renderer.sharedMaterials = mats;
                        }
                        break;
                    default:
                        {
                            using var serializedObject = new SerializedObject(component);

                            foreach (var objectReferenceProperty in serializedObject.ObjectReferenceProperties())
                            {
                                objectReferenceProperty.objectReferenceValue = cloner.MapObject(objectReferenceProperty.objectReferenceValue);
                            }

                            serializedObject.ApplyModifiedPropertiesWithoutUndo();

                            break;
                        }
                }
            }

            return true;
        }

        private static Material OnCloneMaterial(Material material, BuildContext context, SharedState state)
        {
            var map = state.ClonedMaterials;
            Material result = material;
            foreach (var module in ModuleRegistry.Modules)
            {
                if (!module.OnFilterMaterial(material))
                    continue;

                if (!map.TryGetValue(material, out var cloned))
                {
                    map.Add(material, cloned = CloneMaterial(material, context.AssetContainer));
                    cloned.name = $"{material.name} (Material Optimizer Cloned)";
                }

                if (!state.ModulesMap.TryGetValue(module, out var list))
                    state.ModulesMap.Add(module, list = new());

                list.Add(cloned);
                result = cloned;
            }
            return result;
        }

        private static AnimationClip? OnCloneAnimationClip(AnimationClip anim, BuildContext context, SharedState state)
        {
            const string MaterialAnimationPrefix = "material.";
            var bindings = AnimationUtility.GetCurveBindings(anim);
            foreach (var binding in bindings)
            {
                var propertyName = binding.propertyName;
                if (!propertyName.StartsWith(MaterialAnimationPrefix))
                    continue;
                state.AnimatedProperties.Add(propertyName[MaterialAnimationPrefix.Length..]);
            }

            return null;
        }

        private static Material CloneMaterial(Material material, Object? assetContainer = null)
        {
            Material newMat;
            using (MaterialEditorReflection.BeginNoApplyMaterialPropertyDrawers())
                newMat = new Material(material);
            newMat.parent = null;
            if (assetContainer != null)
            {
                AssetDatabase.AddObjectToAsset(newMat, assetContainer);
            }
            ObjectRegistry.RegisterReplacedObject(material, newMat);
            return newMat;
        }
    }
}
