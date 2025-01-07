using Numeira.MaterialOptimizer.API;
using UnityEditor;
using UnityEngine;
using static VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control;

namespace Numeira.MaterialOptimizer;

[CustomEditor(typeof(MaterialOptimizerComponent))]
internal sealed class MaterialOptimizerComponentEditor : Editor
{
    private Dictionary<Type, SerializedObject> serializedModuleSettings = new();

    public void OnEnable()
    {
        BuildSerializedModules();
    }

    private void BuildSerializedModules()
    {
        var target = this.target as MaterialOptimizerComponent;
        if (target == null)
            return;

        var components = target.GetComponents<MaterialOptimizerSettingsBase>();
        foreach (var serializedObject in serializedModuleSettings.Values)
        {
            serializedObject.Dispose();
        }
        serializedModuleSettings.Clear();

        foreach (var component in components)
        {
            serializedModuleSettings.TryAdd(component.GetType(), new SerializedObject(component));
        }
    }

    public void OnDestroy()
    {
        foreach (var serializedObject in serializedModuleSettings.Values)
        {
            serializedObject.Dispose();
        }
        serializedModuleSettings.Clear();
    }

    public override void OnInspectorGUI()
    {
        var target = this.target as MaterialOptimizerComponent;
        if (target == null)
            return;

        serializedObject.Update();
        bool needRefleshModules = false;
        foreach (var serializedObject in serializedModuleSettings.Values)
        {
            if (serializedObject.targetObject == null)
            {
                needRefleshModules = true;
                continue;
            }

            serializedObject.ApplyModifiedProperties();
        }

        if (needRefleshModules)
        {
            BuildSerializedModules();
        }

        try
        {
            EditorGUILayout.LabelField("Material Optimizer", largeBoldLabelStyle ??= new(EditorStyles.largeLabel) { fontStyle = FontStyle.Bold });

            EditorGUILayout.Space();

            var modules = ModuleRegistry.Modules;
            if (modules.IsEmpty)
            {
                EditorGUILayout.LabelField("No modules registered.");
                return;
            }

            foreach (var module in modules)
            {
                if (!ModuleRegistry.TryGetSettingsType(module, out var type))
                    continue;

                var serializedObject = serializedModuleSettings.GetValueOrDefault(type);

                using var group = new ModuleGroupHeaderScope(module.DisplayName, serializedObject);
                if (serializedObject == null && (group.Enabled || group.IsExpanded))
                {
                    serializedObject = new(ModuleRegistry.GetOrAddModuleSettings(target, module));
                    serializedModuleSettings.TryAdd(type, serializedObject);
                    serializedObject.FindProperty("m_Enabled").boolValue = group.Enabled;
                    serializedObject.FindProperty("isExpanded").boolValue = group.IsExpanded;
                }

                if (!group.IsExpanded)
                    continue;

                module.OnGUI(serializedObject!);
            }

        }
        finally
        {
            serializedObject.ApplyModifiedProperties();

            foreach (var serializedObject in serializedModuleSettings.Values)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }

    private static GUIStyle? largeBoldLabelStyle;


    internal readonly ref struct ModuleGroupHeaderScope
    {
        private static GUIStyle? InnerBoxStyle;
        public static GUIStyle? TitleStyle;

        private readonly bool NeedEndLayoutGroup;
        public readonly bool IsExpanded;
        public readonly bool Enabled;

        static ModuleGroupHeaderScope()
        {
            InnerBoxStyle = new("HelpBox")
            {
                margin = new RectOffset(),
                padding = new RectOffset(6, 6, 6, 6),
            };
            TitleStyle = new("ShurikenModuleTitle")
            {
                font = EditorStyles.label.font,
                border = new RectOffset(15, 7, 4, 4),
                fixedHeight = 22,
                contentOffset = new Vector2(20f, -2f),
                fontSize = 12
            };
        }

        public ModuleGroupHeaderScope(string title, SerializedObject serializedObject)
        {
            NeedEndLayoutGroup = false;
            (Enabled, IsExpanded) = Foldout(title, serializedObject);
            if (!IsExpanded)
                return;

            Draw(out NeedEndLayoutGroup);
        }

        private void Draw(out bool needEndLayoutGroup)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(EditorGUI.indentLevel * 15f);

            EditorGUILayout.BeginVertical(InnerBoxStyle);
            needEndLayoutGroup = true;
        }

        public void Dispose()
        {
            if (!NeedEndLayoutGroup)
                return;

            EditorGUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private static (bool Enabled, bool IsExpanded) Foldout(string title, SerializedObject serializedObject)
        {
            var titleStyle = TitleStyle ??= new("ShurikenModuleTitle")
            {
                font = EditorStyles.label.font,
                border = new RectOffset(15, 7, 4, 4),
                fixedHeight = 22,
                contentOffset = new Vector2(20f, -2f),
                fontSize = 12
            };

            var expandedProp = serializedObject?.FindProperty(nameof(MaterialOptimizerSettingsBase.isExpanded));
            bool expanded = expandedProp?.boolValue ?? false;
            var enableProp = serializedObject?.FindProperty("m_Enabled");
            bool enabled = enableProp?.boolValue ?? false;

            var rect = EditorGUILayout.GetControlRect(false, 20f, titleStyle);
            rect = EditorGUI.IndentedRect(rect);

            GUI.Box(rect, GUIContent.none, titleStyle);
            var r = rect;
            r.x += EditorStyles.foldout.CalcSize(GUIContent.none).x + 4;
            r.width = EditorStyles.toggle.CalcSize(GUIContent.none).x;
            EditorGUI.BeginChangeCheck();
            var v = EditorGUI.ToggleLeft(r, title, enabled);
            if (EditorGUI.EndChangeCheck())
            {
                enabled = v;
                if (enableProp != null) 
                    enableProp.boolValue = enabled;
            }
            r.x += r.width + 0;
            r.width = EditorStyles.label.CalcSize(L10n.TempContent(title)).x;
            EditorGUI.LabelField(r, title);

            var e = Event.current;
            var toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
            if (e.type == EventType.Repaint)
            {
                EditorStyles.foldout.Draw(toggleRect, false, false, expanded, false);
            }
            else if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                expanded = !expanded;

                if (expandedProp != null)
                    expandedProp.boolValue = expanded;

                e.Use();
                GUI.changed = true;
            }

            return (enabled, expanded);
        }
    }
}

