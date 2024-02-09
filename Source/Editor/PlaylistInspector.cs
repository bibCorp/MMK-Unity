using UnityEngine;
using UnityEditor;

namespace bibCorp
{
    [CustomEditor(typeof(Playlist))]
    public class PlaylistInspector : Editor
    {
        SerializedProperty m_Loop;
        SerializedProperty m_FiletypeIsMusicTrack;
        SerializedProperty m_MusicTracks;
        SerializedProperty m_AudioFiles;

        Texture m_bibCorpLogo;
        GUIStyle m_Label;

        private void OnEnable()
        {
            InitializeProperties();
            m_bibCorpLogo = Resources.Load<Texture>("bibCorp Audio Logo");
        }
        private void InitializeProperties()
        {
            m_Loop = serializedObject.FindProperty("m_Loop");
            m_FiletypeIsMusicTrack = serializedObject.FindProperty("m_FiletypeIsMusicTrack");
            m_MusicTracks = serializedObject.FindProperty("m_MusicTracks");
            m_AudioFiles = serializedObject.FindProperty("m_AudioFiles");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            m_Label = new GUIStyle(EditorStyles.boldLabel);
            m_Label.alignment = TextAnchor.MiddleCenter;
            m_Label.fontSize = 22;

            GUILayout.Space(10f);
            EditorGUILayout.LabelField("Playlist", m_Label, GUILayout.Height(25f));

            GUILayout.Space(10f);
            GUILayout.Box(m_bibCorpLogo, GUILayout.ExpandWidth(true), GUILayout.Height(125f));
            EditorGUILayout.HelpBox("The 'Modular Music Kit' asset package is created by bibCorp.", MessageType.None);

            GUILayout.Space(25f);
            GUILayout.Box(Texture2D.whiteTexture, GUILayout.ExpandWidth(true), GUILayout.Height(2));

            GUILayout.Space(20f);
            EditorGUILayout.LabelField(serializedObject.targetObject.name, m_Label, GUILayout.Height(25f));

            GUILayout.Space(20f);
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.PropertyField(m_Loop, GUIContent.none, GUILayout.Width(20f));
                EditorGUILayout.LabelField("Loop Playlist");
            }
            EditorGUILayout.EndHorizontal();

            if (m_Loop.boolValue)
                EditorGUILayout.HelpBox("Playlist will loop." +
                    "\nEach MusicTrack will be played only once, even if it's own settings also has looping enabled.", MessageType.Info);

            GUILayout.Space(20f);

            EditorGUILayout.LabelField("Track list:", EditorStyles.boldLabel, GUILayout.Height(25f));

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Change File Type"))
                    m_FiletypeIsMusicTrack.boolValue = !m_FiletypeIsMusicTrack.boolValue;

                EditorGUILayout.HelpBox("Filetype: " + (m_FiletypeIsMusicTrack.boolValue ? "'Music Track'" : "'Audio File'"), MessageType.None);
            }
            EditorGUILayout.EndHorizontal();

            if (m_FiletypeIsMusicTrack.boolValue)
                EditorGUILayout.HelpBox("A 'Music Track' is a custom music file that can be created by right-clicking in the assets folders and selecting:" +
                                        "\nCreate > bibCorp > Music > Music Track", MessageType.Info);
            else
                EditorGUILayout.HelpBox("An 'Audio File' is a regular music file (e.g. .mp3 / .wav)", MessageType.Info);

            if (m_FiletypeIsMusicTrack.boolValue)
            {
                EditorGUILayout.PropertyField(m_MusicTracks);
                if (m_MusicTracks.arraySize == 0)
                    EditorGUILayout.HelpBox("Playlist is empty!", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.PropertyField(m_AudioFiles);
                if (m_AudioFiles.arraySize == 0)
                    EditorGUILayout.HelpBox("Playlist is empty!", MessageType.Warning);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}