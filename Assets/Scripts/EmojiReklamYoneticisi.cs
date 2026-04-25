using UnityEngine;

/// <summary>
/// Efsanevi emojiler için reklam izleme ilerlemesini yönetir
/// PlayerPrefs ile kalıcı veri saklar
/// </summary>
public class EmojiReklamYoneticisi : MonoBehaviour
{
    private static EmojiReklamYoneticisi _instance;
    public static EmojiReklamYoneticisi Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject("EmojiReklamYoneticisi");
                _instance = obj.AddComponent<EmojiReklamYoneticisi>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }

    private AdsManager _adsManager;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // AdsManager'ı bul
        _adsManager = FindObjectOfType<AdsManager>();
        if (_adsManager != null)
        {
            // Reklam tamamlandığında tetiklenecek eventi dinle
            _adsManager.OnRewardedAdCompleted += OnRewardedAdCompleted;
        }
        else
        {
            Debug.LogError("AdsManager bulunamadı!");
        }
    }

    /// <summary>
    /// Belirli bir emoji için izlenen reklam sayısını döndürür
    /// </summary>
    public int GetWatchedAdCount(string emojiID)
    {
        return PlayerPrefs.GetInt("LegendaryAd_" + emojiID, 0);
    }

    /// <summary>
    /// Reklam izlendiğinde sayacı artırır
    /// </summary>
    public void IncrementAdCount(string emojiID)
    {
        int currentCount = GetWatchedAdCount(emojiID);
        currentCount++;
        PlayerPrefs.SetInt("LegendaryAd_" + emojiID, currentCount);
        PlayerPrefs.Save();
        
        Debug.Log($"Emoji {emojiID} için {currentCount} reklam izlendi.");
    }

    /// <summary>
    /// Gerekli reklam sayısına ulaşılıp ulaşılmadığını kontrol eder
    /// </summary>
    public bool IsUnlocked(string emojiID, int requiredAds)
    {
        return GetWatchedAdCount(emojiID) >= requiredAds;
    }

    /// <summary>
    /// Rewarded ad tamamlandığında tetiklenir
    /// </summary>
    private void OnRewardedAdCompleted(string emojiID)
    {
        Debug.Log($"Rewarded ad tamamlandı: {emojiID}");
        IncrementAdCount(emojiID);
    }

    /// <summary>
    /// Reklam izlemeyi başlat (AdsManager'a yönlendirir)
    /// </summary>
    public void ShowRewardedAdForEmoji(string emojiID)
    {
        if (_adsManager != null)
        {
            _adsManager.ShowRewardedAd(emojiID);
        }
        else
        {
            Debug.LogError("AdsManager bulunamadı! Reklam gösterilemiyor.");
        }
    }

    void OnDestroy()
    {
        if (_adsManager != null)
        {
            _adsManager.OnRewardedAdCompleted -= OnRewardedAdCompleted;
        }
    }
}
