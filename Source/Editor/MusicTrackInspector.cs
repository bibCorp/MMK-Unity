using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace bibCorp
{
    [CustomEditor(typeof(MusicTrack))]
    public class MusicTrackInspector : Editor
    {
        enum Mode { EditChannels, ModifyMusic }

        SerializedProperty m_Loop;
        SerializedProperty m_TrackVolume;
        SerializedProperty m_TrackChannels;

        Mode m_Mode = Mode.ModifyMusic;
        bool m_ExplainMusicTrack;
        bool m_ExplainLivePreview;

        SceneAsset m_LivePreviewScene;
        Texture m_bibCorpLogo;
        GUIStyle m_Label;

        private void OnEnable()
        {
            InitializeProperties();

            if (m_TrackChannels.arraySize == 0)
                m_Mode = Mode.EditChannels;

            m_ExplainMusicTrack = false;
            m_ExplainLivePreview = false;
            m_LivePreviewScene = Resources.Load<SceneAsset>("Live Preview Scene");
            m_bibCorpLogo = Resources.Load<Texture>("bibCorp Audio Logo");
        }
        private void InitializeProperties()
        {
            m_Loop = serializedObject.FindProperty("m_Loop");
            m_TrackVolume = serializedObject.FindProperty("m_TrackVolume");
            m_TrackChannels = serializedObject.FindProperty("m_TrackChannels");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            m_Label = new GUIStyle(EditorStyles.boldLabel);
            m_Label.alignment = TextAnchor.MiddleCenter;
            m_Label.fontSize = 22;

            GUILayout.Space(10f);
            EditorGUILayout.LabelField("Music Track", m_Label, GUILayout.Height(25f));

            GUILayout.Space(10f);
            GUILayout.Box(m_bibCorpLogo, GUILayout.ExpandWidth(true), GUILayout.Height(125f));
            EditorGUILayout.HelpBox("The 'Modular Music Kit' asset package is created by bibCorp.", MessageType.None);

            if (GUILayout.Button("What is a 'Music Track'?"))
                m_ExplainMusicTrack = !m_ExplainMusicTrack;

            if (m_ExplainMusicTrack)
                EditorGUILayout.HelpBox("A new Music Track can be created either by duplicating an existing asset, or by right-clicking in the assets folders and selecting:" +
                                        "\nCreate > bibCorp > Music > Music Track" +
                                        "\n\nA 'Music Track' can be used to customize multichannel soundtracks." +
                                        "\nIn order for this to work, the music must have been exported as separate audio files by the creator." +
                                        "\nFor example, by exporting one instrument per audio file, and then re-combining the soundtrack as separate music channels inside a Music Track, you will be able to adjust each channel in order to modify the soundtrack." +
                                        "\n\nFor more information and support links, please read the included Documentation PDF.",
                                        MessageType.Info);

            GUILayout.Space(25f);
            GUILayout.Box(Texture2D.whiteTexture, GUILayout.ExpandWidth(true), GUILayout.Height(2));

            GUILayout.Space(20f);
            EditorGUILayout.LabelField(serializedObject.targetObject.name, m_Label, GUILayout.Height(25f));

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Master Volume:", GUILayout.Width(90f));
                m_TrackVolume.floatValue = EditorGUILayout.Slider(m_TrackVolume.floatValue, 0.1f, 1f);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Loop:", GUILayout.Width(35f));
                EditorGUILayout.PropertyField(m_Loop, GUIContent.none, GUILayout.Width(20f));
            }
            EditorGUILayout.EndHorizontal();

            #region Track Channels

            m_Label.fontSize = 18;
            EditorGUILayout.LabelField("Music Channels", m_Label, GUILayout.Height(25f));

            {
                switch (m_Mode)
                {
                    case Mode.EditChannels:
                        {
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button("Modify", GUILayout.ExpandWidth(true)))
                                m_Mode = Mode.ModifyMusic;
                            GUILayout.Box("Add / Remove", GUILayout.ExpandWidth(true));
                            EditorGUILayout.EndHorizontal();
                        }
                        break;

                    case Mode.ModifyMusic:
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Box("Modify", GUILayout.ExpandWidth(true));
                            if (GUILayout.Button("Add / Remove", GUILayout.ExpandWidth(true)))
                                m_Mode = Mode.EditChannels;
                            EditorGUILayout.EndHorizontal();

                            if (m_LivePreviewScene != null)
                            {
                                if (GUILayout.Button("Live Preview"))
                                    m_ExplainLivePreview = !m_ExplainLivePreview;

                                if (m_ExplainLivePreview)
                                {
                                    EditorGUILayout.HelpBox("It's possible to make live modifications while listening to the soundtrack using the 'Preview Scene'", MessageType.Info);

                                    bool _sceneIsDirty = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().isDirty;
                                    EditorGUI.BeginDisabledGroup(_sceneIsDirty);
                                    {
                                        if (GUILayout.Button("Open Scene (takes a few seconds)"))
                                        {
                                            MusicSettings.Instance.PreSelectedTrack = (MusicTrack)serializedObject.targetObject;

                                            if (EditorApplication.isPlaying)
                                            {
                                                UnityEngine.SceneManagement.SceneManager.LoadScene(AssetDatabase.GetAssetPath(m_LivePreviewScene));
                                            }
                                            else
                                            {
                                                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(m_LivePreviewScene));
                                                EditorApplication.EnterPlaymode();
                                            }
                                            return;
                                        }
                                    }
                                    EditorGUI.EndDisabledGroup();

                                    if (_sceneIsDirty)
                                        EditorGUILayout.HelpBox("Please save your current scene first", MessageType.Warning);
                                }
                            }
                        }
                        break;

                    default:
                        Debug.LogError("Missing case: " + m_Mode);
                        break;
                }
            }

            if (m_Mode == Mode.ModifyMusic)
                if (m_TrackChannels.arraySize == 0)
                {
                    GUILayout.Space(5f);
                    EditorGUILayout.LabelField("No channels added", EditorStyles.boldLabel);
                }

            GUILayout.Space(5f);
            for (int i = 0; i < m_TrackChannels.arraySize; i++)
            {
                var _audioClip = m_TrackChannels.GetArrayElementAtIndex(i).FindPropertyRelative("m_AudioClip");
                if (_audioClip.objectReferenceValue == null)
                {
                    m_TrackChannels.DeleteArrayElementAtIndex(i);
                    break;
                }

                switch (m_Mode)
                {
                    case Mode.EditChannels:
                        {
                            EditorGUILayout.BeginHorizontal();
                            {
                                if (GUILayout.Button("X", GUILayout.Width(20f)))
                                {
                                    m_TrackChannels.DeleteArrayElementAtIndex(i--);
                                    serializedObject.ApplyModifiedProperties();
                                    return;
                                }
                                EditorGUILayout.PropertyField(_audioClip, GUIContent.none, true, GUILayout.ExpandWidth(true));
                            }
                            EditorGUILayout.EndHorizontal();

                            GUILayout.Space(5f);
                        }
                        break;

                    case Mode.ModifyMusic:
                        {
                            EditorGUILayout.BeginHorizontal();
                            {
                                var _enabled = m_TrackChannels.GetArrayElementAtIndex(i).FindPropertyRelative("m_Enabled");
                                EditorGUILayout.PropertyField(_enabled, GUIContent.none, true, GUILayout.Width(20f));

                                EditorGUI.BeginDisabledGroup(true);
                                EditorGUILayout.PropertyField(_audioClip, GUIContent.none, true, GUILayout.ExpandWidth(true));
                                EditorGUI.EndDisabledGroup();
                            }
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.BeginHorizontal();
                            {
                                EditorGUILayout.LabelField("Volume", GUILayout.Width(50f));
                                var _volume = m_TrackChannels.GetArrayElementAtIndex(i).FindPropertyRelative("m_ChannelVolume");
                                _volume.floatValue = EditorGUILayout.Slider(_volume.floatValue, 0f, 1f);
                            }
                            EditorGUILayout.EndHorizontal();

                            GUILayout.Space(10f);
                        }
                        break;

                    default:
                        Debug.LogError("Missing case: " + m_Mode);
                        break;
                }
            }

            if (m_Mode == Mode.EditChannels)
            {
                if (GUILayout.Button("Delete All Channels"))
                    m_TrackChannels.ClearArray();
                GUILayout.Space(5f);
                DrawDropAreaGUI();
            }

            #endregion   

            serializedObject.ApplyModifiedProperties();
        }

        public void DrawDropAreaGUI()
        {
            Event _event = Event.current;
            Rect _dropArea = GUILayoutUtility.GetRect(0.0f, 80.0f, GUILayout.ExpandWidth(true));
            GUI.Box(_dropArea, "\n\nDrop an audio file or folder here to add new channels");

            switch (_event.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    {
                        if (!_dropArea.Contains(_event.mousePosition))
                            return;

                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                        if (_event.type == EventType.DragPerform)
                        {
                            DragAndDrop.AcceptDrag();

                            foreach (Object _draggedObject in DragAndDrop.objectReferences)
                            {
                                if (_draggedObject is AudioClip _draggedClip)
                                {
                                    TryAddChannelForClip(_draggedClip);
                                }
                                else if (_draggedObject is DefaultAsset _folder)
                                {
                                    string _path = AssetDatabase.GetAssetPath(_folder);
                                    var _assetFiles = GetFiles(_path).Where(s => s.Contains(".meta") == false);
                                    foreach (string f in _assetFiles)
                                    {
                                        var _clip = ((AudioClip)AssetDatabase.LoadAssetAtPath(f, typeof(AudioClip)));
                                        TryAddChannelForClip(_clip);
                                    }
                                }
                            }
                        }
                        break;
                    }
            }
        }

        private void AddChannel(AudioClip pClip = null)
        {
            m_TrackChannels.arraySize++;

            int _i = m_TrackChannels.arraySize - 1;
            m_TrackChannels.GetArrayElementAtIndex(_i).FindPropertyRelative("m_Enabled").boolValue = true;
            m_TrackChannels.GetArrayElementAtIndex(_i).FindPropertyRelative("m_AudioClip").objectReferenceValue = pClip;
            m_TrackChannels.GetArrayElementAtIndex(_i).FindPropertyRelative("m_ChannelVolume").floatValue = 1f;
        }
        private void TryAddChannelForClip(AudioClip pClip)
        {
            if (m_TrackChannels.arraySize == 0)
                AddChannel(pClip);
            else
                for (int i = 0; i < m_TrackChannels.arraySize; i++)
                {
                    var _channel = m_TrackChannels.GetArrayElementAtIndex(i);
                    var _channelClip = _channel.FindPropertyRelative("m_AudioClip").objectReferenceValue;
                    if (pClip == _channelClip)
                        break;

                    // if this was last loop
                    if (i == m_TrackChannels.arraySize - 1)
                        AddChannel(pClip);
                }
        }
        private void DeleteEmptyChannels()
        {
            for (int i = m_TrackChannels.arraySize - 1; i >= 0; i--)
            {
                var _channel = m_TrackChannels.GetArrayElementAtIndex(i);
                if (_channel.FindPropertyRelative("m_AudioClip").objectReferenceValue == null)
                    m_TrackChannels.DeleteArrayElementAtIndex(i);
            }
        }

        /// <summary> Recursively gather all files under the given path including all its subfolders. </summary>
        static IEnumerable<string> GetFiles(string pPath)
        {
            Queue<string> queue = new Queue<string>();
            queue.Enqueue(pPath);
            while (queue.Count > 0)
            {
                pPath = queue.Dequeue();
                try
                {
                    foreach (string subDir in Directory.GetDirectories(pPath))
                    {
                        queue.Enqueue(subDir);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError(ex.Message);
                }
                string[] files = null;
                try
                {
                    files = Directory.GetFiles(pPath);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError(ex.Message);
                }
                if (files != null)
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        yield return files[i];
                    }
                }
            }
        }
    }
}