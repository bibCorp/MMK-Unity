using UnityEngine;
using UnityEditor;

namespace bibCorp
{
    [CreateAssetMenu(fileName = "Music Settings", menuName = "bibCorp/Music/Settings", order = 2)]
    public class MusicSettings : ScriptableObject
    {
        static public MusicSettings Instance
        {
            get
            {
                if (m_Instance == null)
                    LoadInstance();
                return m_Instance;
            }
        }
        static private MusicSettings m_Instance;

        public MusicTrack PreSelectedTrack = default;
        [SerializeField] public MusicPlayer DefaultMusicPlayerPrefab = default;


#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Resources.LoadAll<MusicSettings>("").Length > 1)
                Debug.LogWarningFormat(this, "Duplicate 'Music Settings' found: " + AssetDatabase.GetAssetPath(this) +
                    "\nPlease make sure there is only 1.");
        }
#endif

        static private MusicSettings LoadInstance()
        {
            var _files = Resources.LoadAll<MusicSettings>("");
            if (_files.Length > 0)
                m_Instance = _files[0];

            if (m_Instance == null)
                Debug.LogError("Failed to load instance. Please make sure there exists at least one 'Music Settings' " +
                    "(Right-click > Create > bibCorp > Music > Settings) inside a 'Resources' folder." +
                    "\nBy default there is one located: 'Assets/bibCorp/Tools/Modular Music Kit/Resources'");

            return m_Instance;
        }
    }
}