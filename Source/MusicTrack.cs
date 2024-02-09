using System;
using UnityEngine;

namespace bibCorp
{
    [System.Serializable]
    public class MusicChannel
    {
        public bool Enabled => m_Enabled;
        public AudioClip AudioClip => m_AudioClip;
        public float ChannelVolume => m_ChannelVolume;

        [SerializeField] public bool m_Enabled = true;
        [SerializeField] public AudioClip m_AudioClip = default;
        [SerializeField] public float m_ChannelVolume = 1f;

        public void SetEnabled(bool pEnabled) => m_Enabled = pEnabled;
        public void SetVolume(float pVolume) => m_ChannelVolume = pVolume;
        public MusicChannel Copy()
        {
            var _copy = new MusicChannel();
            _copy.m_Enabled = this.m_Enabled;
            _copy.m_AudioClip = this.m_AudioClip;
            _copy.m_ChannelVolume = this.m_ChannelVolume;
            return _copy;
        }
    }

    [CreateAssetMenu(fileName = "New Track", menuName = "bibCorp/Music/Music Track", order = 0)]
    public class MusicTrack : ScriptableObject
    {
        public bool Loop => m_Loop;
        public float TrackVolume => m_TrackVolume;
        public MusicChannel[] TrackChannels => m_TrackChannels;

        [SerializeField] bool m_Loop = false;
        [SerializeField] float m_TrackVolume = 1f;
        [SerializeField] MusicChannel[] m_TrackChannels = default;

        public void SetLooping(bool pLoop) => m_Loop = pLoop;
        public void SetTrackVolume(float pVolume) => m_TrackVolume = pVolume;
        public MusicTrack Copy()
        {
            var _copy = CreateInstance<MusicTrack>();
            _copy.m_Loop = this.m_Loop;
            _copy.m_TrackVolume = this.m_TrackVolume;
            _copy.m_TrackChannels = new MusicChannel[this.m_TrackChannels.Length];
            for (int i = 0; i < this.m_TrackChannels.Length; i++)
                _copy.m_TrackChannels[i] = this.m_TrackChannels[i].Copy();
            return _copy;
        }
    }
}