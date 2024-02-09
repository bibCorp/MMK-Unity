using UnityEngine;
using UnityEngine.Audio;
using System;
using System.Collections;
using System.Collections.Generic;

// "MusicPlayer.cs" and the "Modular Music Kit" asset package is created by bibCorp.

namespace bibCorp
{
    public class MusicPlayer : MonoBehaviour
    {
        public class MusicFile
        {
            public MusicTrack MusicTrack;
            public AudioClip AudioFile;

            public bool IsMusicTrack => MusicTrack;
            public bool Loop = true;
            public float Volume = 1f;

            public MusicFile(MusicTrack pTrack)
            {
                MusicTrack = pTrack;
                Loop = pTrack.Loop;
                Volume = pTrack.TrackVolume;
            }

            public MusicFile(AudioClip pClip, float pVolume = 1f, bool pLoop = false)
            {
                AudioFile = pClip;
                Loop = pLoop;
                Volume = pVolume;
            }
        }

        static public event Action TrackFinishedPlaying;

        public enum PlaybackMode
        {
            PlaybackStopped,
            Solo,
            Playlist
        }

        public enum AutoPlayMode
        {
            AutoplayDisabled,
            MusicTrack,
            AudioFile,
            Playlist
        }

        [SerializeField] AutoPlayMode m_AutoPlayMode = default;
        [SerializeField] Playlist m_CurrentPlaylist = default;
        [SerializeField] MusicTrack m_CurrentMusicTrack = default;
        [SerializeField] AudioClip m_AudioFile = default;
        [SerializeField] bool m_AudioFileLoop = false;
        [SerializeField] float m_AudioFileVolume = 1f;
        [SerializeField] AudioMixerGroup m_CustomMixer = default;

        /// <summary> TRUE if there exists a music player AND the player has a current track AND that track is playing </summary>
        static public bool IsPlaying => (Exists && m_Instance.m_CurrentMusicFile != null &&
                                         m_Instance.m_AudioSources.Count > 0 &&
                                         m_Instance.m_AudioSources[0].isPlaying);

        static public bool Exists => m_Instance != null;
        static public bool HasActivePlaylist => m_Instance.m_CurrentPlaylist;
        static private MusicPlayer m_Instance;
        static private IEnumerator m_Co_WaitForEndOfTrack;
        static private IEnumerator m_Co_FadeCurrentTrack;
        private static MusicFile m_QueuedMusicTrack;

        private MusicFile m_CurrentMusicFile = default;
        private List<AudioSource> m_AudioSources = new List<AudioSource>();
        private PlaybackMode m_PlaybackMode = PlaybackMode.PlaybackStopped;

        private void OnEnable()
        {
            if (m_Instance == null)
            {
                // This is now the master Music Player
                m_Instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                // There already exists another Music Player
                this.gameObject.SetActive(false);
            }

            switch (m_AutoPlayMode)
            {
                case AutoPlayMode.AutoplayDisabled:
                    break;

                case AutoPlayMode.MusicTrack:
                    if (m_CurrentMusicTrack == null)
                        Debug.LogWarningFormat(this, "Unable to autoplay: no 'Music Track' has been assigned");
                    else
                        PlayMusicFile(new MusicFile(m_CurrentMusicTrack));
                    break;

                case AutoPlayMode.AudioFile:
                    if (m_AudioFile == null)
                        Debug.LogWarningFormat(this, "Unable to autoplay: no 'Audio File' has been assigned");
                    else
                        PlayMusicFile(new MusicFile(m_AudioFile, m_AudioFileVolume, m_AudioFileLoop));
                    break;

                case AutoPlayMode.Playlist:
                    if (m_CurrentPlaylist == null)
                        Debug.LogWarningFormat(this, "Unable to autoplay: no 'Playlist' has been assigned");
                    else
                        PlayPlaylist(m_CurrentPlaylist);
                    break;
            }
        }

        private void OnDisable()
        {
            if (m_Instance == this)
            {
                Stop(true);
                m_Instance = null;
            }
        }

        /// <returns> TRUE on success </returns>
        static public bool Create()
        {
            if (MusicPlayer.Exists)
                return true;

            if (MusicSettings.Instance == null)
                return false;
            else if (MusicSettings.Instance.DefaultMusicPlayerPrefab == null)
            {
                Debug.LogError("No default player has been assigned in the Music Settings file");
                return false;
            }

            Instantiate(MusicSettings.Instance.DefaultMusicPlayerPrefab);
            return true;
        }

        /// <Summary> If no Music Player exists yet, one will automatically be created </Summary>
        static public void PlayMusicTrack(MusicTrack pTrack, float pStartTime = 0f)
        {
            if (pTrack == null)
            {
                Debug.LogError("Null track");
                return;
            }

            PlayMusicFile(new MusicFile(pTrack), pStartTime, PlaybackMode.Solo);
        }

        /// <Summary> If no Music Player exists yet, one will automatically be created </Summary>
        static public void PlayAudioFile(AudioClip pAudioFile, float pStartTime = 0f)
        {
            if (pAudioFile == null)
            {
                Debug.LogError("Null file");
                return;
            }

            PlayMusicFile(new MusicFile(pAudioFile), pStartTime, PlaybackMode.Solo);
        }

        /// <Summary> If no Music Player exists yet, one will automatically be created </Summary>
        static public void PlayPlaylist(Playlist pPlaylist)
        {
            if (pPlaylist == null)
            {
                Debug.LogError("Null playlist");
                return;
            }

            Create();
            m_Instance.m_PlaybackMode = PlaybackMode.Playlist;
            m_Instance.m_CurrentPlaylist = pPlaylist;
            PlayMusicFile(pPlaylist.GetFirstTrack(), 0f, PlaybackMode.Playlist);
        }

        /// <summary> Main play function. All other play functions use this. </summary>
        /// <param name="pMusic"></param>
        /// <param name="pStartTime"></param>
        /// <param name="pPlaybackMode"></param>
        static private void PlayMusicFile(MusicFile pMusic, float pStartTime = 0f,
            PlaybackMode pPlaybackMode = PlaybackMode.Solo)
        {
            Create();

            // Validate the PlaybackMode
            switch (pPlaybackMode)
            {
                case PlaybackMode.Solo:
                    m_Instance.m_CurrentPlaylist = null;
                    break;

                case PlaybackMode.Playlist:
                    if (!HasActivePlaylist)
                    {
                        Debug.LogError(
                            "Requested to play Music Track as part of a playlist, but no playlist is active");
                        return;
                    }

                    break;

                default:
                    Debug.LogError("Invalid parameter: " + pPlaybackMode);
                    return;
            }

            m_Instance.m_PlaybackMode = pPlaybackMode;

            // Stop any running coroutines
            if (m_Co_FadeCurrentTrack != null)
            {
                m_Instance.StopCoroutine(m_Co_FadeCurrentTrack);
                m_Co_FadeCurrentTrack = null;
            }

            if (m_Co_WaitForEndOfTrack != null)
            {
                m_Instance.StopCoroutine(m_Co_WaitForEndOfTrack);
                m_Co_WaitForEndOfTrack = null;
            }

            // If music is already playing, fade out and queue new track for later, then return
            if (IsPlaying)
            {
                m_Co_FadeCurrentTrack = m_Instance.FadeCurrentTrack();
                m_Instance.StartCoroutine(m_Co_FadeCurrentTrack);
                m_QueuedMusicTrack = pMusic;
                return;
            }

            // No music is currently playing so we go ahead and finish the playback
            m_Instance.m_CurrentMusicFile = pMusic;

            if (pMusic.IsMusicTrack)
            {
                var _trackChannels = pMusic.MusicTrack.TrackChannels;

                // Check if we currently have enough AudioSources for each channel in the Music Track
                int _dif = _trackChannels.Length - m_Instance.m_AudioSources.Count;
                if (_dif > 0)
                    m_Instance.CreateNewAudioSources(_dif);

                // Go through each track channel and assign it to an audio source
                int _index = 0;
                for (_index = 0; _index < _trackChannels.Length; _index++)
                {
                    // If this channel is disabled, set volume to 0
                    float _volume = (_trackChannels[_index].Enabled)
                        ? _trackChannels[_index].ChannelVolume * pMusic.Volume
                        : 0f;

                    // If there is currently a playlist active, disable looping
                    bool _loop = (HasActivePlaylist) ? false : pMusic.Loop;

                    m_Instance.m_AudioSources[_index].clip = _trackChannels[_index].AudioClip;
                    m_Instance.m_AudioSources[_index].time = pStartTime;
                    m_Instance.m_AudioSources[_index].loop = _loop;
                    m_Instance.m_AudioSources[_index].volume = _volume;
                    m_Instance.m_AudioSources[_index].gameObject.name = _trackChannels[_index].AudioClip.name;
                    m_Instance.m_AudioSources[_index].Play();
                }

                // Continue through any remaining Audio Sources and mark them as empty
                for (int i = _index; i < m_Instance.m_AudioSources.Count; i++)
                    m_Instance.m_AudioSources[_index].gameObject.name = "empty";
            }
            else
            {
                var _audioSource = m_Instance.GetFirstAudioSource();
                _audioSource.clip = pMusic.AudioFile;
                _audioSource.volume = pMusic.Volume * pMusic.Volume;
                _audioSource.loop = pMusic.Loop;
                _audioSource.gameObject.name = pMusic.AudioFile.name;
                _audioSource.Play();
            }

            // Start a Coroutine that will keep track of when the music has finished playing
            m_Co_WaitForEndOfTrack =
                m_Instance.WaitForEndOfTrack(m_Instance.m_AudioSources[0].clip.length - pStartTime);
            m_Instance.StartCoroutine(m_Co_WaitForEndOfTrack);
        }

        static public void Stop(bool pInstantStop = false)
        {
            if (!Exists)
            {
                Debug.LogWarning("MusicPlayer.Stop() called, but no instance exists.");
                return;
            }

            m_Instance.m_PlaybackMode = PlaybackMode.PlaybackStopped;
            TrackFinishedPlaying = null;

            if (m_Co_WaitForEndOfTrack != null)
                m_Instance.StopCoroutine(m_Co_WaitForEndOfTrack);
            m_Co_WaitForEndOfTrack = null;

            if (m_Co_FadeCurrentTrack != null)
                m_Instance.StopCoroutine(m_Co_FadeCurrentTrack);
            m_Co_FadeCurrentTrack = null;


            if (pInstantStop)
            {
                foreach (var _audioSource in m_Instance.m_AudioSources)
                    _audioSource.Stop();
            }
            else
            {
                m_Co_FadeCurrentTrack = m_Instance.FadeCurrentTrack();
                m_Instance.StartCoroutine(m_Co_FadeCurrentTrack);
            }
        }

        static public void UpdateVolume()
        {
            if (!Exists) return;
            if (m_Instance.m_CurrentMusicFile == null) return;

            var _musicTrack = m_Instance.m_CurrentMusicFile.MusicTrack;
            if (m_Instance.m_CurrentMusicFile.IsMusicTrack)
                for (int i = 0; i < _musicTrack.TrackChannels.Length; i++)
                {
                    m_Instance.m_AudioSources[i].volume = (_musicTrack.TrackChannels[i].Enabled)
                        ? _musicTrack.TrackChannels[i].ChannelVolume * _musicTrack.TrackVolume
                        : 0f;
                }
            else
            {
                m_Instance.GetFirstAudioSource().volume = m_Instance.m_CurrentMusicFile.Volume;
            }
        }

        static public float GetCurrentTime()
        {
            if (IsPlaying)
                return m_Instance.GetFirstAudioSource().time;
            
            return 0f;
        }

        static public string GetCurrentTrackInfo()
        {
            string _info = string.Empty;
            if (m_Instance.m_CurrentMusicFile.IsMusicTrack)
                _info = "Music Track: " + m_Instance.m_CurrentMusicFile.MusicTrack.name;
            else
                _info = "Audio File: " + m_Instance.m_AudioFile.name;
            return _info;
        }

        static public AudioSource GetAudioSource(MusicChannel pChannel)
        {
            foreach (var _as in m_Instance.m_AudioSources)
                if (_as.clip == pChannel.AudioClip)
                    return _as;

            return null;
        }

        private AudioSource GetFirstAudioSource()
        {
            if (m_Instance.m_AudioSources.Count == 0)
                CreateNewAudioSources(1);
            return m_Instance.m_AudioSources[0];
        }

        private void CreateNewAudioSources(int pAmount)
        {
            for (int i = 0; i < pAmount; i++)
            {
                // Create a new GameObject with an AudioSource component, and attach it to the MusicPlayer
                GameObject _newGO = new GameObject("Empty Audio Source", typeof(AudioSource));
                _newGO.transform.SetParent(transform);

                AudioSource _newAudioSource = _newGO.GetComponent<AudioSource>();

                // Apply settings
                _newAudioSource.playOnAwake = false;
                _newAudioSource.outputAudioMixerGroup = m_CustomMixer;

                m_AudioSources.Add(_newAudioSource);
            }
        }

        private IEnumerator WaitForEndOfTrack(float pTrackDuration)
        {
            yield return new WaitForSeconds(pTrackDuration);

            while (IsPlaying)
                yield return null;

            OnTrackFinishedPlaying();
        }

        private IEnumerator FadeCurrentTrack()
        {
            bool _fading = true;
            while (_fading)
            {
                _fading = false;
                foreach (var _as in m_Instance.m_AudioSources)
                {
                    _as.volume -= Time.unscaledDeltaTime;
                    if (_as.volume <= 0)
                        _as.Stop();
                    else
                        _fading = true;
                }

                if (_fading)
                    yield return null;
            }

            OnTrackFadeCompleted();
        }

        private void OnTrackFinishedPlaying()
        {
            m_Co_WaitForEndOfTrack = null;
            TrackFinishedPlaying?.Invoke();

            switch (m_PlaybackMode)
            {
                case PlaybackMode.Solo:
                    Stop(true);
                    break;

                case PlaybackMode.Playlist:
                    if (HasActivePlaylist)
                    {
                        // Try to get the next track from the playlist
                        if (m_CurrentPlaylist.SelectNextTrack())
                            PlayMusicFile(m_CurrentPlaylist.GetNextTrack(), 0f, PlaybackMode.Playlist);
                        else
                            m_CurrentPlaylist = null;
                    }
                    else
                        Stop(true);

                    break;

                default:
                    Debug.LogError("Invalid case:" + m_PlaybackMode);
                    break;
            }
        }

        private void OnTrackFadeCompleted()
        {
            m_Co_FadeCurrentTrack = null;

            switch (m_PlaybackMode)
            {
                case PlaybackMode.PlaybackStopped:
                    break;

                case PlaybackMode.Solo:
                    if (m_QueuedMusicTrack != null)
                        PlayMusicFile(m_QueuedMusicTrack);
                    else
                        Stop(true);
                    break;

                case PlaybackMode.Playlist:
                    if (HasActivePlaylist)
                    {
                        if (m_QueuedMusicTrack != null)
                            PlayMusicFile(m_QueuedMusicTrack, 0f, PlaybackMode.Playlist);
                        else
                            Stop(true);
                    }

                    break;
            }
        }
    }
}