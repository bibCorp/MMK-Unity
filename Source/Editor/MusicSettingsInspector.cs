using UnityEngine;
using UnityEditor;

namespace bibCorp
{
    [CustomEditor(typeof(MusicSettings))]
    public class MusicSettingsInspector : Editor
    {
        SerializedProperty DefaultMusicPlayerPrefab;
        
        Texture m_bibCorpLogo;
        GUIStyle m_Label;

        private void OnEnable()
        {
            InitializeProperties();
            m_bibCorpLogo = Resources.Load<Texture>("bibCorp Audio Logo");
        }
        private void InitializeProperties()
        {
            DefaultMusicPlayerPrefab = serializedObject.FindProperty("DefaultMusicPlayerPrefab");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            m_Label = new GUIStyle(EditorStyles.boldLabel);
            m_Label.alignment = TextAnchor.MiddleCenter;
            m_Label.fontSize = 22;

            GUILayout.Space(10f);
            EditorGUILayout.LabelField("Settings", m_Label, GUILayout.Height(25f));

            GUILayout.Space(10f);
            GUILayout.Box(m_bibCorpLogo, GUILayout.ExpandWidth(true), GUILayout.Height(125f));
            EditorGUILayout.HelpBox("The 'Modular Music Kit' asset package is created by bibCorp.", MessageType.None);

            GUILayout.Space(25f);
            GUILayout.Box(Texture2D.whiteTexture, GUILayout.ExpandWidth(true), GUILayout.Height(2));

            GUILayout.Space(20f);
            GUILayout.Label("Default Music Player prefab:", EditorStyles.label, GUILayout.ExpandWidth(true));
            EditorGUILayout.PropertyField(DefaultMusicPlayerPrefab, GUIContent.none);

            EditorGUILayout.HelpBox("This is the default Music Player that will be automatically created if you call one of the" +
                " 'MusicPlayer.Play' functions without first explicitly instantiating a Music Player." +
                "\n\nThis exists to make the code even more simple, by not forcing users to worry about creating/destroying any music players." +
                "\n\nIf you have made your own Music Player prefabs and have placed them directly into your scenes, or if you are instantiating them with code, then you can ignore this setting.", MessageType.Info);

            serializedObject.ApplyModifiedProperties();
        }
    }
}