using UnityEngine;

namespace bibCorp
{
    [CreateAssetMenu(fileName = "New Playlist", menuName = "bibCorp/Music/Playlist", order = 1)]
    public class Playlist : ScriptableObject
    {
        public bool Loop => m_Loop;

        [SerializeField] bool m_Loop = false;
        [SerializeField] bool m_FiletypeIsMusicTrack = true;
        [SerializeField] MusicTrack[] m_MusicTracks = default;
        [SerializeField] AudioClip[] m_AudioFiles = default;

        private int m_TrackIndex;

        private void OnValidate()
        {
            if (m_MusicTracks.Length == 0)
                Debug.LogWarning("Playlist: '" + this.name + "' is empty");
        }

        /// <returns> FALSE if Playlist is finished and is not set to Loop </returns>
        public bool SelectNextTrack()
        {
            int _length = (m_FiletypeIsMusicTrack) ? m_MusicTracks.Length : m_AudioFiles.Length;
            if (++m_TrackIndex == _length)
            {
                if (Loop)
                    m_TrackIndex = 0;
                else
                    return false;
            }
            return true;
        }

        public MusicPlayer.MusicFile GetFirstTrack() => (m_FiletypeIsMusicTrack) ? new MusicPlayer.MusicFile(m_MusicTracks[m_TrackIndex = 0])
                                                                                 : new MusicPlayer.MusicFile(m_AudioFiles[m_TrackIndex = 0]);

        public MusicPlayer.MusicFile GetNextTrack() => (m_FiletypeIsMusicTrack) ? new MusicPlayer.MusicFile(m_MusicTracks[m_TrackIndex])
                                                                                : new MusicPlayer.MusicFile(m_AudioFiles[m_TrackIndex]);
    }
}