using UnityEngine;
using Unity.Services.Core;
using Unity.Services.LevelPlay;

public class AdsManager : MonoBehaviour
{
    // Panelden aldığımız ID'lerin:
    [SerializeField] private string _androidAppKey = "2525421f5"; 
    [SerializeField] private string _interstitialAdUnitId = "18i6r718skxsf40y";
    [SerializeField] private string _rewardedAdUnitId = "t2flkzcc5xprfn02"; // Buraya rewarded ad unit ID'nizi girin
    [SerializeField] private string _bannerAdUnitId = "6gficvua7sr8btoe"; // Buraya banner ad unit ID'nizi girin
    
    [Header("Banner Ayarları")]
    [SerializeField] private LevelPlayBannerPosition _bannerPosition = LevelPlayBannerPosition.BottomCenter;
    [SerializeField] private bool _displayBannerOnLoad = true; // Yüklenince otomatik göster
    
    [SerializeField] private bool _testMode = true;

    private LevelPlayInterstitialAd _interstitialAd;
    private LevelPlayRewardedAd _rewardedAd;
    private LevelPlayBannerAd _bannerAd;
    
    // Rewarded ad tamamlanma eventi
    public System.Action<string> OnRewardedAdCompleted; // Emoji ID ile tetiklenecek

    async void Awake()
    {
        try
        {
            await UnityServices.InitializeAsync();
            Debug.Log("Unity Services başlatıldı.");

            // Init Eventlerini Dinle
            LevelPlay.OnInitSuccess += OnLevelPlayInitSuccess;
            LevelPlay.OnInitFailed += OnLevelPlayInitFailed;

            if (_testMode)
            {
                LevelPlay.SetMetaData("is_test_suite", "enable");
            }

            // Başlat
            LevelPlay.Init(_androidAppKey);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Unity Servis Hatası: " + e.Message);
        }
    }

    // --- INIT EVENTLERİ ---
    void OnLevelPlayInitSuccess(LevelPlayConfiguration config)
    {
        Debug.Log("LevelPlay Başarıyla Başlatıldı!");
        CreateInterstitialAd();
        CreateRewardedAd();
        CreateBannerAd();
    }

    void OnLevelPlayInitFailed(LevelPlayInitError error)
    {
        Debug.LogError("LevelPlay Init Hatası: " + error.ErrorMessage);
    }

    // --- REKLAM OLUŞTURMA ---
    void CreateInterstitialAd()
    {
        _interstitialAd = new LevelPlayInterstitialAd(_interstitialAdUnitId);

        // Eventleri Bağla
        _interstitialAd.OnAdLoaded += OnAdLoaded;
        _interstitialAd.OnAdLoadFailed += OnAdLoadFailed;
        _interstitialAd.OnAdDisplayed += OnAdDisplayed;
        _interstitialAd.OnAdDisplayFailed += OnAdDisplayFailed;
        _interstitialAd.OnAdClosed += OnAdClosed;

        LoadAd(); 
    }

    public void LoadAd()
    {
        Debug.Log("Reklam Yükleniyor...");
        _interstitialAd.LoadAd(); 
    }

    public void ShowAd()
    {
        Debug.Log("Reklam Gösterilmeye Çalışılıyor...");
        _interstitialAd.ShowAd();
    }

    // --- REKLAM EVENTLERİ ---
    
    void OnAdLoaded(LevelPlayAdInfo info)
    {
        Debug.Log("Reklam YÜKLENDİ! Gösterime Hazır.");
    }

    void OnAdLoadFailed(LevelPlayAdError error)
    {
        Debug.LogError("Reklam Yükleme Hatası: " + error.ErrorMessage);
    }

    void OnAdDisplayed(LevelPlayAdInfo info)
    {
        Debug.Log("Reklam Ekrana Geldi.");
    }

    // DÜZELTİLEN KISIM BURASI: (Önce Info, Sonra Error)
    void OnAdDisplayFailed(LevelPlayAdInfo info, LevelPlayAdError error)
    {
        Debug.LogError("Reklam Gösterme Hatası: " + error.ErrorMessage);
        LoadAd(); 
    }

    void OnAdClosed(LevelPlayAdInfo info)
    {
        Debug.Log("Reklam Kapatıldı.");
        LoadAd(); 
    }

    // --- REWARDED AD METODLARI ---
    
    void CreateRewardedAd()
    {
        _rewardedAd = new LevelPlayRewardedAd(_rewardedAdUnitId);
        
        // Eventleri Bağla
        _rewardedAd.OnAdLoaded += OnRewardedAdLoaded;
        _rewardedAd.OnAdLoadFailed += OnRewardedAdLoadFailed;
        _rewardedAd.OnAdDisplayed += OnRewardedAdDisplayed;
        _rewardedAd.OnAdDisplayFailed += OnRewardedAdDisplayFailed;
        _rewardedAd.OnAdClosed += OnRewardedAdClosed;
        _rewardedAd.OnAdRewarded += OnAdRewarded; // ÖDÜL VERİLDİĞİNDE
        
        LoadRewardedAd();
    }
    
    public void LoadRewardedAd()
    {
        Debug.Log("Rewarded Ad Yükleniyor...");
        _rewardedAd.LoadAd();
    }
    
    private string _currentEmojiID; // Hangi emoji için reklam gösterildiğini takip etmek için
    
    public void ShowRewardedAd(string emojiID)
    {
        _currentEmojiID = emojiID;
        Debug.Log($"Rewarded Ad Gösteriliyor... (Emoji: {emojiID})");
        _rewardedAd.ShowAd();
    }
    
    // --- REWARDED AD EVENTLERİ ---
    
    void OnRewardedAdLoaded(LevelPlayAdInfo info)
    {
        Debug.Log("Rewarded Ad YÜKLENDİ! Gösterime Hazır.");
    }
    
    void OnRewardedAdLoadFailed(LevelPlayAdError error)
    {
        Debug.LogError("Rewarded Ad Yükleme Hatası: " + error.ErrorMessage);
    }
    
    void OnRewardedAdDisplayed(LevelPlayAdInfo info)
    {
        Debug.Log("Rewarded Ad Ekrana Geldi.");
    }
    
    void OnRewardedAdDisplayFailed(LevelPlayAdInfo info, LevelPlayAdError error)
    {
        Debug.LogError("Rewarded Ad Gösterme Hatası: " + error.ErrorMessage);
        LoadRewardedAd();
    }
    
    void OnRewardedAdClosed(LevelPlayAdInfo info)
    {
        Debug.Log("Rewarded Ad Kapatıldı.");
        LoadRewardedAd();
    }
    
    void OnAdRewarded(LevelPlayAdInfo info, LevelPlayReward reward)
    {
        Debug.Log($"ÖDÜL VERİLDİ! Miktar: {reward.Amount}, İsim: {reward.Name}");
        
        // Event'i tetikle - EmojiReklamYoneticisi dinleyecek
        OnRewardedAdCompleted?.Invoke(_currentEmojiID);
    }
    
    // --- BANNER AD METODLARI ---
    
    void CreateBannerAd()
    {
        // Config.Builder pattern ile banner oluştur (LevelPlay dokümantasyonuna göre)
        var configBuilder = new LevelPlayBannerAd.Config.Builder();
        configBuilder.SetSize(LevelPlayAdSize.BANNER);
        configBuilder.SetPosition(_bannerPosition);
        configBuilder.SetDisplayOnLoad(_displayBannerOnLoad);
        var bannerConfig = configBuilder.Build();
        
        _bannerAd = new LevelPlayBannerAd(_bannerAdUnitId, bannerConfig);
        
        // Eventleri Bağla
        _bannerAd.OnAdLoaded += OnBannerAdLoaded;
        _bannerAd.OnAdLoadFailed += OnBannerAdLoadFailed;
        _bannerAd.OnAdDisplayed += OnBannerAdDisplayed;
        _bannerAd.OnAdDisplayFailed += OnBannerAdDisplayFailed;
        _bannerAd.OnAdClicked += OnBannerAdClicked;
        _bannerAd.OnAdCollapsed += OnBannerAdCollapsed;
        _bannerAd.OnAdLeftApplication += OnBannerAdLeftApplication;
        _bannerAd.OnAdExpanded += OnBannerAdExpanded;
        
        LoadBannerAd();
    }
    
    public void LoadBannerAd()
    {
        if (_bannerAd != null)
        {
            Debug.Log("Banner Ad Yükleniyor...");
            _bannerAd.LoadAd();
        }
    }
    
    public void ShowBanner()
    {
        if (_bannerAd != null)
        {
            Debug.Log("Banner gösteriliyor...");
            _bannerAd.ShowAd();
        }
    }
    
    public void HideBanner()
    {
        if (_bannerAd != null)
        {
            Debug.Log("Banner gizleniyor...");
            _bannerAd.HideAd();
        }
    }
    
    public void DestroyBanner()
    {
        if (_bannerAd != null)
        {
            Debug.Log("Banner siliniyor...");
            _bannerAd.DestroyAd();
            _bannerAd = null;
        }
    }
    
    public void PauseBannerAutoRefresh()
    {
        if (_bannerAd != null)
        {
            _bannerAd.PauseAutoRefresh();
        }
    }
    
    public void ResumeBannerAutoRefresh()
    {
        if (_bannerAd != null)
        {
            _bannerAd.ResumeAutoRefresh();
        }
    }
    
    // --- BANNER AD EVENTLERİ ---
    
    void OnBannerAdLoaded(LevelPlayAdInfo adInfo)
    {
        Debug.Log("Banner Ad YÜKLENDİ!");
    }
    
    void OnBannerAdLoadFailed(LevelPlayAdError error)
    {
        Debug.LogError("Banner Ad Yükleme Hatası: " + error.ErrorMessage);
    }
    
    void OnBannerAdDisplayed(LevelPlayAdInfo adInfo)
    {
        Debug.Log("Banner Ad Ekrana Geldi.");
    }
    
    void OnBannerAdDisplayFailed(LevelPlayAdInfo adInfo, LevelPlayAdError error)
    {
        Debug.LogError("Banner Ad Gösterme Hatası: " + error.ErrorMessage);
    }
    
    void OnBannerAdClicked(LevelPlayAdInfo adInfo)
    {
        Debug.Log("Banner Ad Tıklandı.");
    }
    
    void OnBannerAdCollapsed(LevelPlayAdInfo adInfo)
    {
        Debug.Log("Banner Ad Küçültüldü.");
    }
    
    void OnBannerAdLeftApplication(LevelPlayAdInfo adInfo)
    {
        Debug.Log("Banner Ad - Uygulama Terk Edildi.");
    }
    
    void OnBannerAdExpanded(LevelPlayAdInfo adInfo)
    {
        Debug.Log("Banner Ad Genişletildi.");
    }
    
    void OnDestroy()
    {
        LevelPlay.OnInitSuccess -= OnLevelPlayInitSuccess;
        LevelPlay.OnInitFailed -= OnLevelPlayInitFailed;

        if (_interstitialAd != null)
        {
            _interstitialAd.Dispose();
        }
        
        if (_rewardedAd != null)
        {
            _rewardedAd.Dispose();
        }
        
        if (_bannerAd != null)
        {
            _bannerAd.DestroyAd();
        }
    }
}