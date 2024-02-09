using UnityEngine;
using UnityEngine.UI;

namespace bibCorp
{
    public class UI_MusicChannel : MonoBehaviour
    {
        [SerializeField] Toggle m_Toggle = default;
        [SerializeField] Slider m_Slider = default;
        [SerializeField] Text m_ChannelName = default;
        [SerializeField] Text m_VolumeValue = default;
        [SerializeField] Image m_ActiveIndicator = default;

        private MusicChannel m_Channel;
        private float[] m_ClipSampleData = new float[1024];

        private void FixedUpdate()
        {
            if (m_Channel == null)
                Destroy(this.gameObject);

            m_ActiveIndicator.enabled = false;

            if (m_Channel != null)
                if (m_Channel.Enabled)
                {
                    if (MusicPlayer.IsPlaying)
                    {
                        var _audioSource = MusicPlayer.GetAudioSource(m_Channel);
                        _audioSource.clip.GetData(m_ClipSampleData, _audioSource.timeSamples); // TODO catch this error
                        var _clipLoudness = 0f;
                        foreach (var sample in m_ClipSampleData)
                            _clipLoudness += Mathf.Abs(sample);
                        _clipLoudness /= 1024;
                        m_ActiveIndicator.enabled = _clipLoudness > 0;
                    }
                }
        }

        public void AssignChannel(MusicChannel pChannel, string pChannelName)
        {
            m_Channel = pChannel;
            m_ChannelName.text = pChannelName;

            m_Toggle.isOn = m_Channel.Enabled;
            m_Toggle.onValueChanged.AddListener((bool enabled) =>
            {
                m_Channel.SetEnabled(enabled);
                MusicPlayer.UpdateVolume();
            });

            m_Slider.value = m_Channel.ChannelVolume;
            m_Slider.onValueChanged.AddListener((float value) =>
            {
                m_Channel.SetVolume(value);
                m_VolumeValue.text = value.ToString("N1");
                MusicPlayer.UpdateVolume();
            });
            m_VolumeValue.text = m_Slider.value.ToString("N1");
        }
    }
}