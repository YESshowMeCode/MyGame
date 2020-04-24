using UnityEditor;
using UnityEngine;

namespace  UGUIExtend
{
	[CustomEditor(typeof(OutLineText),true)]
	[CanEditMultipleObjects]
	public class OutLineTextEditor : UnityEditor.UI.TextEditor
	{
        [MenuItem("GameObject/UI/OutLine Text",false,3)]

        static void CreatePanel()
        {
            GameObject spriteObject = new GameObject("Text");

            if(Selection.activeGameObject != null)
            {
                spriteObject.transform.parent = Selection.activeGameObject.transform;
                spriteObject.layer = Selection.activeGameObject.layer;
            }
            else
            {
                Canvas mainCanvas = GameObject.FindObjectOfType<Canvas>();
                if(mainCanvas != null)
                {
                    spriteObject.transform.parent = Selection.activeTransform.transform;
                    spriteObject.layer = Selection.activeGameObject.layer;
                }
            }

            OutLineText t = spriteObject.AddComponent<OutLineText>();
            t.supportRichText = false;
            t.raycastTarget = false;
            Selection.objects = new Object[] { spriteObject };

        }

        SerializedProperty m_EffectType;
        SerializedProperty m_EffectColor;
        SerializedProperty m_EffectDistance;
        SerializedProperty m_UseGraphicAlpha;
        SerializedProperty m_GradientColor;
        SerializedProperty m_EnabledGradient;
        SerializedProperty imagePathRoot;
        SerializedProperty charOffests;
        SerializedProperty m_Visible;


        protected override void OnEnable()
        {
            base.OnEnable();

            m_EffectType = serializedObject.FindProperty("m_EffectType");
            m_EffectColor = serializedObject.FindProperty("m_EffectColor");
            m_EffectDistance = serializedObject.FindProperty("m_EffectDistance");
            m_UseGraphicAlpha = serializedObject.FindProperty("m_UseGraphicAlpha");
            m_GradientColor = serializedObject.FindProperty("m_GradientColor");
            m_EnabledGradient = serializedObject.FindProperty("m_EnabledGradient");
            imagePathRoot = serializedObject.FindProperty("imagePathRoot");
            charOffests = serializedObject.FindProperty("charOffests");
            m_Visible = serializedObject.FindProperty("m_Visible");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();
            EditorGUILayout.PropertyField(m_Visible);
            EditorGUILayout.PropertyField(m_EffectType);
            EditorGUILayout.PropertyField(m_EnabledGradient);

            if (m_EffectType.enumValueIndex != 0)
            {
                EditorGUILayout.PropertyField(m_EffectColor);
                EditorGUILayout.PropertyField(m_EffectDistance);
                EditorGUILayout.PropertyField(m_UseGraphicAlpha);
            }
            if(m_EnabledGradient.boolValue)
            {
                EditorGUILayout.PropertyField(m_GradientColor);
            }
            EditorGUILayout.PropertyField(imagePathRoot);

            if(GUILayout.Button("clear InlineImage"))
            {
                foreach(Object target in this.targets)
                {
                    OutLineText text = target as OutLineText;
                    if(text != null)
                    {
                        text.ClearUnUsedInlineImage();
                    }
                }
            }
            EditorGUILayout.PropertyField(charOffests, true);
            serializedObject.ApplyModifiedProperties();
        }

    }
	
}
