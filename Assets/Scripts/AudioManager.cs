using UnityEngine;

/// <summary>
/// Ses ayarlarını yöneten singleton manager
/// </summary>
public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<AudioManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("AudioManager");
                    instance = obj.AddComponent<AudioManager>();
                }
            }
            return instance;
        }
    }

    private const string SFX_PREF_KEY = "SFXEnabled";
    private bool sfxEnabled = true;

    public bool SFXEnabled
    {
        get { return sfxEnabled; }
        set
        {
            sfxEnabled = value;
            PlayerPrefs.SetInt(SFX_PREF_KEY, sfxEnabled ? 1 : 0);
            PlayerPrefs.Save();
            Debug.Log("[AudioManager] SFX " + (sfxEnabled ? "Açık" : "Kapalı"));
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            // PlayerPrefs'ten ayarı yükle (varsayılan: açık)
            sfxEnabled = PlayerPrefs.GetInt(SFX_PREF_KEY, 1) == 1;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// AudioSource'u SFX ayarına göre çalar
    /// </summary>
    public void PlaySFX(AudioSource source)
    {
        if (source != null && sfxEnabled)
        {
            source.Play();
        }
    }

    /// <summary>
    /// AudioSource'u SFX ayarına göre durdurur
    /// </summary>
    public void StopSFX(AudioSource source)
    {
        if (source != null && source.isPlaying)
        {
            source.Stop();
        }
    }
}
