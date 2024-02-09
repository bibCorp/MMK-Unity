using UnityEngine;
using UnityEditor;

namespace bibCorp
{
    [CustomEditor(typeof(MusicPlayer))]
    public class MusicPlayerInspector : Editor
    {
        SerializedProperty m_AutoPlayMode;
        SerializedProperty m_CurrentPlaylist;
        SerializedProperty m_CurrentMusicTrack;
        SerializedProperty m_AudioFile;
        SerializedProperty m_AudioFileLoop;
        SerializedProperty m_AudioFileVolume;
        SerializedProperty m_CustomMixer;

        Texture m_bibCorpLogo;
        GUIStyle m_Label;

        private void OnEnable()
        {
            InitializeProperties();
            m_bibCorpLogo = Resources.Load<Texture>("bibCorp Audio Logo");
        }
        private void InitializeProperties()
        {
            m_AutoPlayMode = serializedObject.FindProperty("m_AutoPlayMode");
            m_CurrentPlaylist = serializedObject.FindProperty("m_CurrentPlaylist");
            m_CurrentMusicTrack = serializedObject.FindProperty("m_CurrentMusicTrack");
            m_AudioFile = serializedObject.FindProperty("m_AudioFile");
            m_AudioFileLoop = serializedObject.FindProperty("m_AudioFileLoop");
            m_AudioFileVolume = serializedObject.FindProperty("m_AudioFileVolume");
            m_CustomMixer = serializedObject.FindProperty("m_CustomMixer");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            m_Label = new GUIStyle(EditorStyles.boldLabel);
            m_Label.alignment = TextAnchor.MiddleCenter;
            m_Label.fontSize = 22;

            GUILayout.Space(10f);
            EditorGUILayout.LabelField("Music Player", m_Label, GUILayout.Height(25f));

            GUILayout.Space(10f);
            GUILayout.Box(m_bibCorpLogo, GUILayout.ExpandWidth(true), GUILayout.Height(125f));
            EditorGUILayout.HelpBox("The 'Modular Music Kit' asset package is created by bibCorp.", MessageType.None);

            GUILayout.Space(25f);
            GUILayout.Box(Texture2D.whiteTexture, GUILayout.ExpandWidth(true), GUILayout.Height(2));

            GUILayout.Space(20);
            EditorGUILayout.PropertyField(m_AutoPlayMode);

            switch ((MusicPlayer.AutoPlayMode)m_AutoPlayMode.enumValueIndex)
            {
                case MusicPlayer.AutoPlayMode.AutoplayDisabled:
                    EditorGUILayout.HelpBox("A music file or playlist can be assigned to auto play when the music player is instantiated or enabled.", MessageType.None);
                    break;

                case MusicPlayer.AutoPlayMode.MusicTrack:
                    EditorGUILayout.PropertyField(m_CurrentMusicTrack);
                    if (m_CurrentMusicTrack.objectReferenceValue == null)
                        EditorGUILayout.HelpBox("No Music Track selected", MessageType.Warning);
                    break;

                case MusicPlayer.AutoPlayMode.AudioFile:
                    EditorGUILayout.PropertyField(m_AudioFile);
                    EditorGUILayout.PropertyField(m_AudioFileLoop);
                    m_AudioFileVolume.floatValue = EditorGUILayout.Slider("Audio File Volume", m_AudioFileVolume.floatValue, 0f, 2f);
                    if (m_AudioFile.objectReferenceValue == null)
                        EditorGUILayout.HelpBox("No Audio File selected", MessageType.Warning);
                    break;

                case MusicPlayer.AutoPlayMode.Playlist:
                    EditorGUILayout.PropertyField(m_CurrentPlaylist);
                    EditorGUILayout.HelpBox("A 'Playlist' can be created in your assets folder by right-clicking and selecting:" +
                        "\nCreate > bibCorp > Music > Playlist.", MessageType.Info);
                    if (m_CurrentPlaylist.objectReferenceValue == null)
                        EditorGUILayout.HelpBox("No Playlist selected", MessageType.Warning);
                    break;
            }

            GUILayout.Space(20);
            EditorGUILayout.PropertyField(m_CustomMixer);
            EditorGUILayout.HelpBox("If you want this Music Player to use one of your custom Mixers you can assign it here." +
                "\nShould be empty by default.", MessageType.Info);

            serializedObject.ApplyModifiedProperties();
        }
    }
}