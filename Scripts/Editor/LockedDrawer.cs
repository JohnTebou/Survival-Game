using UnityEditor;
using UnityEngine;

namespace John
{
    [CustomPropertyDrawer(typeof(LockedAttribute))]
    public sealed class LockedDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect fieldRect = EditorGUI.PrefixLabel(position, label);

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUI.PropertyField(fieldRect, property, GUIContent.none, true);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => EditorGUI.GetPropertyHeight(property, label, true);
    }
}