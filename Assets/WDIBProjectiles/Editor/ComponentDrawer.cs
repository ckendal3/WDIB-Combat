using UnityEditor;
using UnityEngine;

namespace WDIB.Editor
{
    [CustomPropertyDrawer(typeof(ComponentDataStruct))]
    public class ComponentStructDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUILayout.BeginHorizontal();

            var enumProperty = property.FindPropertyRelative("componentType");
            var intProperty = property.FindPropertyRelative("intValue");
            var floatProperty = property.FindPropertyRelative("floatValue");

            var enumValue = enumProperty.enumValueIndex;
            var enumNames = enumProperty.enumNames;

            enumProperty.enumValueIndex = EditorGUILayout.Popup(enumValue, enumNames);

            Rect contentPosition = EditorGUI.PrefixLabel(position, label);

            switch ((EComponentType)enumValue)
            {
                case EComponentType.HeadShot:
                    floatProperty.floatValue = EditorGUILayout.Slider("Multiplier", floatProperty.floatValue, 1, 5);
                    break;
                case EComponentType.MultiHit:
                    intProperty.intValue = EditorGUILayout.IntSlider("Max Hit Count", intProperty.intValue, 1, 40);
                    break;
                case EComponentType.EMP:
                    Debug.LogWarning("EMP component is not implemented");
                    break;
                case EComponentType.SuperCombine:
                    intProperty.intValue = EditorGUILayout.IntSlider("Hits To Combine", intProperty.intValue, 1, 40);
                    break;
                case EComponentType.Tracking:
                    intProperty.intValue = EditorGUILayout.IntSlider("Player ID", intProperty.intValue, 0, 40);
                    break;
                case EComponentType.Explosive:
                    intProperty.intValue = EditorGUILayout.IntSlider("Explosive ID", intProperty.intValue, 0, 40);
                    break;
                case EComponentType.NotImplemented:
                    Debug.LogWarning("Component is not implemented");
                    break;
            }

            GUILayout.EndHorizontal();

        }
    }
}