using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Koleksiyon ekranındaki tek bir emoji kutucuğunu yönetir.
/// </summary>
public class EmojiKoleksiyonOgesi : MonoBehaviour
{
    [Header("UI Elemanları")]
    public Image emojiGorseli;
    public TextMeshProUGUI emojiAdiText;
    public GameObject kilitIconu;
    public Button buOgeButonu;
    
    [Header("Efsanevi Özel")]
    public GameObject reklamButonuObjesi; // Sadece efsanevi ve kilitli ise görünür
    public Button reklamButonu;
    public TextMeshProUGUI reklamIlerlemesiText; // Örn: "3/5 Reklam"
    
    private EmojiData mevcutEmoji;
    private bool kilitliMi;
    private bool efsaneviKilitliMi; // Sadece Efsanevi tipler için
    
    public void Ayarla(EmojiData veri, bool emojiKurtarildi)
    {
        mevcutEmoji = veri;
        
        // Temel Bilgiler
        if (emojiAdiText != null) 
            emojiAdiText.text = veri.emojiAdi;
            
        emojiGorseli.sprite = veri.emojiSprite;
        
        // Efsanevi Kontrolü
        bool isEfsanevi = veri.nadirlik == EmojiNadirlik.Efsanevi;
        
        if (emojiKurtarildi)
        {
            // Emoji zaten kurtarıldı, kilidi aç
            KilidiAc(false);
            reklamButonuObjesi.SetActive(false);
        }
        else if (isEfsanevi)
        {
            // Efsanevi ve kurtarılmamış - reklam sistemi aktif
            int watchedAds = EmojiReklamYoneticisi.Instance.GetWatchedAdCount(veri.emojiID);
            int requiredAds = veri.reklamGereksinimi;
            
            if (watchedAds >= requiredAds)
            {
                // Gerekli reklam sayısına ulaşıldı - otomatik aç
                KilidiAc(false);
                reklamButonuObjesi.SetActive(false);
            }
            else
            {
                // Hala reklam izlenmesi gerekiyor
                Kilitle(true);
                reklamButonuObjesi.SetActive(true);
                UpdateAdProgressText(watchedAds, requiredAds);
            }
        }
        else
        {
            // Efsanevi değil, normal kilitli
            Kilitle(false);
            reklamButonuObjesi.SetActive(false);
        }

        // Buton dinleyicileri
        buOgeButonu.onClick.RemoveAllListeners();
        buOgeButonu.onClick.AddListener(OnClicked);
        
        // Reklam butonu dinleyicisi
        if (reklamButonu != null)
        {
            reklamButonu.onClick.RemoveAllListeners();
            reklamButonu.onClick.AddListener(OnReklamIzleClicked);
        }
    }
    
    void Kilitle(bool isEfsanevi)
    {
        kilitliMi = true;
        efsaneviKilitliMi = isEfsanevi;
        
        emojiGorseli.color = Color.black; // veya gri
        kilitIconu.SetActive(true);
        buOgeButonu.interactable = false; // Tıklanamaz
        
        // Reklam mantığı kaldırıldı
    }
    
    void KilidiAc(bool isEfsanevi)
    {
        kilitliMi = false;
        efsaneviKilitliMi = false;
        
        emojiGorseli.color = Color.white;
        kilitIconu.SetActive(false);
        buOgeButonu.interactable = true;
        
        // Buton yazısını orijinal haline salla (opsiyonel, zaten buton gizleniyor ama temiz olsun)
        if (reklamButonu != null)
        {
            var btnText = reklamButonu.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null) btnText.text = "Reklam İzle";
        }
    }
    
    void OnClicked()
    {
        if (kilitliMi) return;
        
        Debug.Log($"Emoji seçildi: {mevcutEmoji.emojiAdi}");
        // Burada oyuna başlama veya detay gösterme eklenebilir
    }
    
    void OnReklamIzleClicked()
    {
        if (mevcutEmoji == null) return;
        
        Debug.Log($"Reklam izlenecek: {mevcutEmoji.emojiAdi}");
        EmojiReklamYoneticisi.Instance.ShowRewardedAdForEmoji(mevcutEmoji.emojiID);
        
        // Reklam tamamlandıktan sonra UI'yı güncellemek için invoke kullan
        Invoke(nameof(CheckAndUpdateAdProgress), 1f);
    }
    
    void CheckAndUpdateAdProgress()
    {
        if (mevcutEmoji == null) return;
        
        int watchedAds = EmojiReklamYoneticisi.Instance.GetWatchedAdCount(mevcutEmoji.emojiID);
        int requiredAds = mevcutEmoji.reklamGereksinimi;
        
        if (watchedAds >= requiredAds)
        {
            // Gerekli reklam sayısına ulaşıldı - Efsanevi kilidi aç ve temizleme oyununu başlat
            if (EmojiAlbumu.Instance != null)
            {
                Debug.Log($"[EmojiKoleksiyonOgesi] Gerekli reklam sayısına ulaşıldı, temizleme oyunu başlatılıyor: {mevcutEmoji.emojiAdi}");
                EmojiAlbumu.Instance.EfsaneviKilitAcVeOyunBaslat(mevcutEmoji.emojiID);
            }
        }
        else
        {
            // İlerlemeyi güncelle
            UpdateAdProgressText(watchedAds, requiredAds);
        }
    }
    
    void UpdateAdProgressText(int watched, int required)
    {
        if (reklamIlerlemesiText != null)
        {
            reklamIlerlemesiText.text = $"{watched}/{required} Reklam";
        }
    }
    
}
