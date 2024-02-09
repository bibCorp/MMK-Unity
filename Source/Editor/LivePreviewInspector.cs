using UnityEngine;
using UnityEditor;

namespace bibCorp
{
    [CustomEditor(typeof(LivePreview))]
    public class LivePreviewInspector : Editor
    {
        SerializedProperty m_Warning;
        SerializedProperty m_SelectedTrack;
        SerializedProperty m_PreviewTrack;
        SerializedProperty m_Prefab_MusicChannel;
        SerializedProperty m_Prefab_PopupMessage;
        SerializedProperty m_ChannelsTransform;
        SerializedProperty m_PreviewName;
        SerializedProperty m_LoopToggle;
        SerializedProperty m_PlayImage;
        SerializedProperty m_PauseImage;
        SerializedProperty m_ProgressSlider;
        SerializedProperty m_TrackVolumeSlider;
        SerializedProperty m_TrackVolume;
        SerializedProperty m_CurrentTime;
        SerializedProperty m_TotalTime;
        SerializedProperty m_ShowTrackSelector;

        bool m_CustomizeSettings;
        Texture m_bibCorpLogo;
        GUIStyle m_Label;

        private void OnEnable()
        {
            InitializeProperties();
            m_CustomizeSettings = false;
            m_bibCorpLogo = Resources.Load<Texture>("bibCorp Audio Logo");
        }
        private void InitializeProperties()
        {
            m_Warning = serializedObject.FindProperty("m_Warning");
            m_SelectedTrack = serializedObject.FindProperty("m_SelectedTrack");
            m_PreviewTrack = serializedObject.FindProperty("m_PreviewTrack");
            m_Prefab_MusicChannel = serializedObject.FindProperty("m_Prefab_MusicChannel");
            m_Prefab_PopupMessage = serializedObject.FindProperty("m_Prefab_PopupMessage");
            m_ChannelsTransform = serializedObject.FindProperty("m_ChannelsTransform");
            m_PreviewName = serializedObject.FindProperty("m_PreviewName");
            m_LoopToggle = serializedObject.FindProperty("m_LoopToggle");
            m_PlayImage = serializedObject.FindProperty("m_PlayImage");
            m_PauseImage = serializedObject.FindProperty("m_PauseImage");
            m_ProgressSlider = serializedObject.FindProperty("m_ProgressSlider");
            m_TrackVolumeSlider = serializedObject.FindProperty("m_TrackVolumeSlider");
            m_TrackVolume = serializedObject.FindProperty("m_TrackVolume");
            m_CurrentTime = serializedObject.FindProperty("m_CurrentTime");
            m_TotalTime = serializedObject.FindProperty("m_TotalTime");
            m_ShowTrackSelector = serializedObject.FindProperty("m_ShowTrackSelector");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            m_Label = new GUIStyle(EditorStyles.boldLabel);
            m_Label.alignment = TextAnchor.MiddleCenter;
            m_Label.fontSize = 22;

            GUILayout.Space(10f);
            EditorGUILayout.LabelField("Live Preview", m_Label, GUILayout.Height(25f));

            GUILayout.Space(10f);
            GUILayout.Box(m_bibCorpLogo, GUILayout.ExpandWidth(true), GUILayout.Height(125f));
            EditorGUILayout.HelpBox("The 'Modular Music Kit' asset package is created by bibCorp.", MessageType.None);

            GUILayout.Space(20f);
            var _titleLabel = new GUIStyle(EditorStyles.boldLabel);
            _titleLabel.alignment = TextAnchor.MiddleCenter;
            EditorGUILayout.LabelField("- Live Preview -", _titleLabel);
            EditorGUILayout.HelpBox("Run this scene to modify soundtracks in real time." +
                                    "\nThis scene should NOT be included when you build your game.", MessageType.Info);

            if (m_ShowTrackSelector.boolValue == true)
            {
                EditorGUIUtility.ShowObjectPicker<MusicTrack>(m_SelectedTrack.objectReferenceValue, false, "", 0);
                m_ShowTrackSelector.boolValue = false;
            }
            if (Event.current.commandName == "ObjectSelectorUpdated")
            {
                m_SelectedTrack.objectReferenceValue = EditorGUIUtility.GetObjectPickerObject() as MusicTrack;

                if (Application.isPlaying)
                {
                    LivePreview.Instance.StopPreview();
                    if (m_SelectedTrack.objectReferenceValue != null)
                        LivePreview.Instance.SetTrack(m_SelectedTrack.objectReferenceValue as MusicTrack);
                    else
                        LivePreview.Instance.ClearTrackChannels();
                }
            }

            GUILayout.Space(50f);
            if (GUILayout.Button("EDITOR SETTINGS"))
                m_CustomizeSettings = !m_CustomizeSettings;

            if (m_CustomizeSettings)
            {
                EditorGUILayout.HelpBox("These settings should not be modified", MessageType.Warning);
                EditorGUILayout.PropertyField(m_Warning);
                EditorGUILayout.PropertyField(m_Prefab_MusicChannel);
                EditorGUILayout.PropertyField(m_Prefab_PopupMessage);
                EditorGUILayout.PropertyField(m_ChannelsTransform);
                EditorGUILayout.PropertyField(m_PreviewName);
                EditorGUILayout.PropertyField(m_LoopToggle);
                EditorGUILayout.PropertyField(m_PlayImage);
                EditorGUILayout.PropertyField(m_PauseImage);
                EditorGUILayout.PropertyField(m_ProgressSlider);
                EditorGUILayout.PropertyField(m_TrackVolumeSlider);
                EditorGUILayout.PropertyField(m_TrackVolume);
                EditorGUILayout.PropertyField(m_CurrentTime);
                EditorGUILayout.PropertyField(m_TotalTime);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}