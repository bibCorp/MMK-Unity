using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Globalization;

namespace bibCorp
{
    public class LivePreview : MonoBehaviour
    {
        static public LivePreview Instance { get; private set; }

        [SerializeField] GameObject m_Warning = default;
        [SerializeField] MusicTrack m_SelectedTrack = default;
        [SerializeField] MusicTrack m_PreviewTrack = default;
        [SerializeField] Text m_PreviewName = default;
        [SerializeField] UI_MusicChannel m_Prefab_MusicChannel = default;
        [SerializeField] UI_ConfirmationDialogue m_Prefab_PopupMessage = default;
        [SerializeField] Transform m_ChannelsTransform = default;
        [SerializeField] Toggle m_LoopToggle = default;
        [SerializeField] Image m_PlayImage = default;
        [SerializeField] Image m_PauseImage = default;
        [SerializeField] Slider m_ProgressSlider = default;
        [SerializeField] Slider m_TrackVolumeSlider = default;
        [SerializeField] Text m_TrackVolume = default;
        [SerializeField] Text m_CurrentTime = default;
        [SerializeField] Text m_TotalTime = default;

#pragma warning disable 0414
        [SerializeField] bool m_ShowTrackSelector = false;
#pragma warning restore 0414

        private Canvas m_Canvas;
        private List<UI_MusicChannel> m_Channels = new List<UI_MusicChannel>();

        private void Awake()
        {
            Instance = this;

            m_CurrentTime.text = FormatTimeString(0f);
            m_TotalTime.text = FormatTimeString(0f);
            UpdateUIButtons();

            m_Canvas = GetComponentInChildren<Canvas>();

            // Has a track been assigned for preview in the settings?
            m_SelectedTrack = MusicSettings.Instance.PreSelectedTrack;
            MusicSettings.Instance.PreSelectedTrack = null;

            // If so, preview it right away
            if (m_SelectedTrack != null)
                SetTrack(m_SelectedTrack);

            m_Warning.SetActive(false);
        }

        private void Update()
        {
            if (MusicPlayer.IsPlaying)
            {
                var _currentTime = MusicPlayer.GetCurrentTime();
                m_ProgressSlider.value = _currentTime;
            }

            m_CurrentTime.text = FormatTimeString(m_ProgressSlider.value);

            if (m_PreviewTrack)
            {
                m_PreviewTrack.SetTrackVolume(m_TrackVolumeSlider.value);
                m_TrackVolume.text = m_PreviewTrack.TrackVolume.ToString("N1");
                MusicPlayer.UpdateVolume();
            }
        }

        public void OpenTrackSelector()
        {
#if UNITY_EDITOR
            UnityEditor.Selection.activeGameObject = this.gameObject;
            m_ShowTrackSelector = true;
#endif
        }

        public void NavigateToMusicTrack()
        {
#if UNITY_EDITOR
            if (m_SelectedTrack == null)
                OpenTrackSelector();
            else
                UnityEditor.Selection.activeObject = m_SelectedTrack;
#endif
        }

        public void TrySaveCurrentTrack()
        {
            if (m_SelectedTrack == null)
                return;

            var _popup = Instantiate(m_Prefab_PopupMessage, m_Canvas.transform);
            _popup.Set("Save changes to:\n" + m_SelectedTrack.name + "?", () => { SaveCurrentTrack(); });
        }

        public void TryDiscardCurrentTrack()
        {
            if (m_SelectedTrack == null)
                return;

            var _popup = Instantiate(m_Prefab_PopupMessage, m_Canvas.transform);
            _popup.Set("Discard changes to:\n" + m_SelectedTrack.name + "?", () => { DiscardCurrentTrack(); });
        }

        public void SetTrack(MusicTrack pTrack)
        {
            if (pTrack == null)
                return;
            if (pTrack.TrackChannels.Length == 0)
            {
                Debug.LogWarningFormat(pTrack, "Unable to load Music Track - Track is empty: " + pTrack.name);
                return;
            }

            m_SelectedTrack = pTrack;
            m_PreviewTrack = pTrack.Copy();
            m_PreviewName.text = pTrack.name;

            StopPreview();
            ClearTrackChannels();

            for (int i = 0; i < m_PreviewTrack.TrackChannels.Length; i++)
            {
                var _channel = Instantiate(m_Prefab_MusicChannel, m_ChannelsTransform);
                _channel.AssignChannel(m_PreviewTrack.TrackChannels[i],
                    "Ch. " + (i + 1) + ": " + m_PreviewTrack.TrackChannels[i].AudioClip.name);
                m_Channels.Add(_channel);
            }

            var _totalTime = m_PreviewTrack.TrackChannels[0].AudioClip.length;
            m_ProgressSlider.value = 0f;
            m_ProgressSlider.maxValue = _totalTime;
            m_CurrentTime.text = FormatTimeString(0f);
            m_TotalTime.text = FormatTimeString(_totalTime);

            m_TrackVolume.text = m_PreviewTrack.TrackVolume.ToString("N2");
            m_TrackVolumeSlider.value = m_PreviewTrack.TrackVolume;
            
            m_LoopToggle.enabled = true;
            m_LoopToggle.isOn = m_PreviewTrack.Loop;
            m_LoopToggle.onValueChanged.AddListener((bool enabled) => { m_PreviewTrack.SetLooping(enabled); });
        }

        public void ClearTrackChannels()
        {
            for (int i = m_Channels.Count - 1; i >= 0; i--)
                Destroy(m_Channels[i].gameObject);
            m_Channels.Clear();

            m_LoopToggle.onValueChanged.RemoveAllListeners();
            m_LoopToggle.enabled = false;
            m_LoopToggle.isOn = false;
        }

        public void PlayPreview()
        {
            if (MusicPlayer.IsPlaying)
            {
                MusicPlayer.Stop(true);
                m_ProgressSlider.interactable = true;
            }
            else if (m_PreviewTrack != null)
            {
                MusicPlayer.TrackFinishedPlaying += OnMusicPlayerFinishedPlaying;
                MusicPlayer.PlayMusicTrack(m_PreviewTrack, m_ProgressSlider.value);
                m_ProgressSlider.interactable = false;
            }

            UpdateUIButtons();
        }

        public void StopPreview()
        {
            if (MusicPlayer.IsPlaying)
            {
                MusicPlayer.Stop(true);
                m_ProgressSlider.interactable = true;
            }

            m_ProgressSlider.value = 0f;
            UpdateUIButtons();
        }

        private void OnMusicPlayerFinishedPlaying()
        {
            MusicPlayer.TrackFinishedPlaying -= OnMusicPlayerFinishedPlaying;
            m_ProgressSlider.value = 0f;
            PlayPreview();
        }

        private string FormatTimeString(float pTime) =>
            string.Format("{0:0}:{1:00}", Mathf.Floor(pTime / 60), pTime % 60);

        private void UpdateUIButtons()
        {
            m_PlayImage.enabled = !MusicPlayer.IsPlaying;
            m_PauseImage.enabled = MusicPlayer.IsPlaying;
        }

        private void SaveCurrentTrack()
        {
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.CreateAsset(m_PreviewTrack,
                UnityEditor.AssetDatabase.GetAssetPath(m_SelectedTrack));
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
#endif
        }

        private void DiscardCurrentTrack()
        {
            SetTrack(m_SelectedTrack);
        }
    }
}