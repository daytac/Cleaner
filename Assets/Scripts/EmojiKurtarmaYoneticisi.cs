using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Emoji Kurtarma Operasyonu - Ana Oyun Yöneticisi
/// Emoji döngüsünü yönetir: Temizlik Aşamaları → Kutlama → Sonraki Emoji
/// </summary>
public class EmojiKurtarmaYoneticisi : MonoBehaviour
{
    [Header("Emoji Havuzu")]
    [Tooltip("Kurtarılabilecek tüm emojiler")]
    public List<EmojiData> emojiHavuzu = new List<EmojiData>();

    [Header("Çıkma Olasılıkları %")]
    [Range(0, 100)] public float sansSiradan = 60f;
    [Range(0, 100)] public float sansEnder = 30f;
    [Range(0, 100)] public float sansDestansi = 10f;
    
    [Header("Referanslar")]
    [Tooltip("Temizlik yapılan objenin LekeTemizleyici scripti")]
    public LekeTemizleyici temizleyici;
    
    [Tooltip("Emoji albümü (opsiyonel - yoksa otomatik bulunur)")]
    public EmojiAlbumu album;
    
    [Header("Kutlama UI")]
    [Tooltip("Kurtarma kutlama paneli")]
    public GameObject kutlamaPanel;
    
    [Tooltip("Kutlama yazısı (örn: 'Tebrikler! Gülen Yüz'ü kurtardın!')")]
    public TMP_Text kutlamaYazisi;
    
    [Tooltip("Nadirlik seviyesi yazısı (emoji görselinin üstünde)")]
    public TMP_Text nadirlikYazisi;
    
    [Tooltip("Kurtarılan emoji görseli (RawImage - Texture2D için)")]
    public RawImage kurtarilanEmojiGorsel;
    
    [Tooltip("Sonraki emojiye geç butonu")]
    public Button sonrakiEmojiButonu;
    
    [Tooltip("Koleksiyonu gör butonu (kutlama ekranında)")]
    public Button koleksiyonuGorButonu;
    
    [Header("Başlangıç Hikayesi")]
    [Tooltip("Hikaye paneli (Eyvah! Emojiler bataklığa düştü!)")]
    public GameObject hikayePanel;
    
    [Tooltip("Oyunu başlat butonu")]
    public Button baslatButonu;
    
    [Header("Bitiş Ekranı")]
    [Tooltip("Tüm emojiler kurtarıldığında gösterilecek panel")]
    public GameObject bitisPanel;
    
    [Tooltip("Bitiş yazısı")]
    public TMP_Text bitisYazisi;
    
    [Header("Koleksiyon")]
    [Tooltip("Koleksiyon paneli GameObject'i")]
    public GameObject koleksiyonPanelObj;
    
    [Tooltip("Koleksiyon animasyonu")]
    public EmojiKoleksiyonAnimasyon koleksiyonAnimasyon;
    
    private EmojiKoleksiyonUI koleksiyonPanel;
    
    [Header("Debug")]
    public bool debugModu = true;
    
    // Aktif emoji takibi
    private EmojiData aktifEmoji;
    private int emojiIndex = 0;
    private int asamaIndex = 0;
    private bool oyunAktif = false;
    
    
    void OnEnable()
    {
        // Efsanevi açıldığında otomatik yükleme için event listener ekle
        EmojiAlbumu.OnEfsaneviAcildi += OtomatikEfsaneviYukle;
    }
    
    void OnDisable()
    {
        // Event listener'ı kaldır
        EmojiAlbumu.OnEfsaneviAcildi -= OtomatikEfsaneviYukle;
    }
    
    void Start()
    {
        // Albümü bul
        if (album == null)
            album = Object.FindFirstObjectByType<EmojiAlbumu>();
        
        // Koleksiyon paneli component'ini al
        if (koleksiyonPanelObj != null)
            koleksiyonPanel = koleksiyonPanelObj.GetComponent<EmojiKoleksiyonUI>();
        
        // UI başlangıç durumu
        if (kutlamaPanel != null) kutlamaPanel.SetActive(false);
        if (bitisPanel != null) bitisPanel.SetActive(false);
        
        // Buton dinleyicileri
        if (sonrakiEmojiButonu != null)
            sonrakiEmojiButonu.onClick.AddListener(SonrakiEmojiyeGec);
        
        if (baslatButonu != null)
            baslatButonu.onClick.AddListener(OyunuBaslat);
        
        if (koleksiyonuGorButonu != null)
            koleksiyonuGorButonu.onClick.AddListener(KoleksiyonuAc);
        
        // Animasyon bitince kutlama panelini göster
        if (koleksiyonAnimasyon != null)
            koleksiyonAnimasyon.OnAnimasyonBitti += KutlamaGoster;
        
        // LekeTemizleyici event'ine abone ol
        if (temizleyici != null)
            temizleyici.OnTemizlikTamamlandi += AsamaTamamlandi;
        
        // Hikaye panelini göster (varsa)
        if (hikayePanel != null)
        {
            hikayePanel.SetActive(true);
        }
        else
        {
            // Hikaye paneli yoksa direkt başla
            OyunuBaslat();
        }
        
        if (debugModu)
            Debug.Log($"[EmojiKurtarma] Başlatıldı. {emojiHavuzu.Count} emoji mevcut.");
    }
    
    void OnDestroy()
    {
        // Event aboneliğini kaldır
        if (temizleyici != null)
            temizleyici.OnTemizlikTamamlandi -= AsamaTamamlandi;
    }
    
    /// <summary>
    /// Oyunu başlat - ilk emojiyi yükle
    /// </summary>
    public void OyunuBaslat()
    {
        if (hikayePanel != null)
            hikayePanel.SetActive(false);
        
        emojiIndex = 0;
        oyunAktif = true;
        
        YeniEmojiYukle();
        
        if (debugModu)
            Debug.Log("[EmojiKurtarma] Oyun başladı!");
    }
    
    /// <summary>
    /// Yeni emoji yükle ve temizliğe hazırla
    /// </summary>
    public void YeniEmojiYukle(EmojiData ozelEmoji = null)
    {
        oyunAktif = true; // Oyunun aktif olduğunu doğrula (aşama geçişleri için kritik)
        
        if (ozelEmoji != null)
        {
            aktifEmoji = ozelEmoji;
        }
        else
        {
            // Rastgele bir emoji seç (Efsanevi hariç)
            aktifEmoji = RastgeleEmojiGetir();
        }

        asamaIndex = 0;
        
        if (aktifEmoji == null)
        {
            Debug.LogError($"[EmojiKurtarma] Uygun emoji bulunamadı! Havuzu kontrol edin.");
            return;
        }
        
        if (debugModu)
            Debug.Log($"[EmojiKurtarma] Yeni emoji yüklendi: {aktifEmoji.emojiAdi} ({emojiIndex + 1}/{emojiHavuzu.Count}) - {aktifEmoji.asamalar.Count} aşama");
        
        // İlk aşamayı başlat
        AsamayiBaslat();
    }
    
    /// <summary>
    /// Mevcut aşamayı başlat
    /// </summary>
    private void AsamayiBaslat()
    {
        if (aktifEmoji == null || aktifEmoji.asamalar.Count == 0)
        {
            Debug.LogError("[EmojiKurtarma] Emoji aşaması bulunamadı!");
            return;
        }
        
        // Eğer tüm aşamalar tamamlandıysa kutlama göster
        if (asamaIndex >= aktifEmoji.asamalar.Count)
        {
            EmojiKurtarildi();
            return;
        }
        
        EmojiTemizlikAsamasi asama = aktifEmoji.asamalar[asamaIndex];
        
        // Son aşamaysa temizTexture olarak emoji'nin final texture'ını kullan
        Texture2D temizTex = asama.temizTexture;
        if (asamaIndex == aktifEmoji.asamalar.Count - 1 && aktifEmoji.temizTexture != null)
        {
            temizTex = aktifEmoji.temizTexture;
        }
        
        // Temizleyiciyi aşama için ayarla
        if (temizleyici != null)
        {
            temizleyici.YeniAsamaBaslat(
                asama.kirliTexture, 
                temizTex,
                asama.aletModeli,
                asama.fircaSekli,
                asama.fircaBoyutu,
                asama.tamamlanmaOrani
            );
        }
        
        if (debugModu)
            Debug.Log($"[EmojiKurtarma] Aşama başlatıldı: {asama.asamaAdi} ({asamaIndex + 1}/{aktifEmoji.asamalar.Count})");
    }
    
    /// <summary>
    /// Bir aşama tamamlandığında çağrılır (LekeTemizleyici event'i)
    /// </summary>
    public void AsamaTamamlandi()
    {
        if (aktifEmoji == null) return;
        
        if (!oyunAktif)
        {
            Debug.LogWarning("[EmojiKurtarma] Aşama tamamlandı ama oyun aktif değil! 'oyunAktif' true yapılıyor.");
            oyunAktif = true;
        }
        
        if (debugModu)
            Debug.Log($"[EmojiKurtarma] ✅ Aşama tamamlandı! Mevcut: {asamaIndex + 1}/{aktifEmoji.asamalar.Count}");
        
        // Sonraki aşamaya geç
        asamaIndex++;
        
        // Kısa bir gecikme ile sonraki aşamayı başlat
        Invoke(nameof(AsamayiBaslat), 0.3f);
    }
    
    /// <summary>
    /// Emoji tamamen kurtarıldı
    /// </summary>
    private void EmojiKurtarildi()
    {
        if (debugModu)
            Debug.Log($"[EmojiKurtarma] Emoji kurtarıldı: {aktifEmoji.emojiAdi}");
        
        // Albüme kaydet
        if (album != null)
            album.EmojiKurtar(aktifEmoji);
        
        // Animasyon başlat (eğer varsa)
        if (koleksiyonAnimasyon != null && kurtarilanEmojiGorsel != null)
        {
            RectTransform baslangicRect = kurtarilanEmojiGorsel.GetComponent<RectTransform>();
            koleksiyonAnimasyon.AnimasyonBaslat(aktifEmoji.temizTexture, baslangicRect);
        }
        
        // Animasyon bitince kutlama panelinin açılmasını Action event ile sağlıyoruz (Start metodunda abone olundu)
        // Eğer animasyon yoksa direkt aç
        if (koleksiyonAnimasyon == null)
        {
             KutlamaGoster();
        }
    }
    
    /// <summary>
    /// Kurtarma kutlaması göster
    /// </summary>
    private void KutlamaGoster()
    {
        if (kutlamaPanel == null) return;
        
        kutlamaPanel.SetActive(true);
        
        // Yazı güncelle
        if (kutlamaYazisi != null)
            kutlamaYazisi.text = aktifEmoji.FormatlıKurtarmaYazisi();
        
        // Nadirlik seviyesini göster
        if (nadirlikYazisi != null)
            nadirlikYazisi.text = NadirlikAdiGetir(aktifEmoji.nadirlik);
        
        // Emoji görseli güncelle (RawImage ile Texture2D kullanıyoruz)
        if (kurtarilanEmojiGorsel != null)
        {
            // Final temiz texture'ı kullan
            if (aktifEmoji.temizTexture != null)
            {
                kurtarilanEmojiGorsel.texture = aktifEmoji.temizTexture;
            }
            else if (aktifEmoji.emojiSprite != null)
            {
                kurtarilanEmojiGorsel.texture = aktifEmoji.emojiSprite.texture;
            }
        }
        
        if (debugModu)
            Debug.Log($"[EmojiKurtarma] Kutlama gösteriliyor: {aktifEmoji.emojiAdi}");
    }
    
    /// <summary>
    /// Sonraki emojiye geç (buton click)
    /// </summary>
    public void SonrakiEmojiyeGec()
    {
        if (kutlamaPanel != null)
            kutlamaPanel.SetActive(false);
        
        // Ads removed by user request
        // if (UnityAdsManager.Instance != null)
        // {
        //     UnityAdsManager.Instance.EmojiCompleted();
        // }
        
        emojiIndex++;
        YeniEmojiYukle();
    }
    
    /// <summary>
    /// Tüm emojiler kurtarıldığında
    /// </summary>
    private void TumEmojilerKurtarildi()
    {
        oyunAktif = false;
        
        if (bitisPanel != null)
        {
            bitisPanel.SetActive(true);
            
            if (bitisYazisi != null)
                bitisYazisi.text = $"Tebrikler!\nTüm {emojiHavuzu.Count} emojiyi kurtardın!";
        }
        
        if (debugModu)
            Debug.Log("[EmojiKurtarma] TÜM EMOJİLER KURTARILDI! 🎉");
    }
    
    /// <summary>
    /// Oyunu sıfırla ve baştan başla
    /// </summary>
    [ContextMenu("Oyunu Sıfırla")]
    public void OyunuSifirla()
    {
        if (kutlamaPanel != null) kutlamaPanel.SetActive(false);
        if (bitisPanel != null) bitisPanel.SetActive(false);
        
        OyunuBaslat();
    }
    
    /// <summary>
    /// Koleksiyonu aç
    /// </summary>
    public void KoleksiyonuAc()
    {
        Debug.Log("[EmojiKurtarma] KoleksiyonuAc butonu tıklandı!");
        
        if (koleksiyonPanel == null && koleksiyonPanelObj != null)
        {
             koleksiyonPanel = koleksiyonPanelObj.GetComponent<EmojiKoleksiyonUI>();
        }

        if (koleksiyonPanel != null)
        {
            Debug.Log("[EmojiKurtarma] Panel açılıyor...");
            koleksiyonPanel.KoleksiyonuAc();
        }
        else
        {
            Debug.LogError($"[EmojiKurtarma] Koleksiyon Panel referansı EKSİK! Obj: {koleksiyonPanelObj}");
        }
    }

    /// <summary>
    /// Nadirlik ağırlıklarına göre rastgele emoji seçer (Efsanevi HARİÇ)
    /// </summary>
    private EmojiData RastgeleEmojiGetir()
    {
        // 1. Havuzu nadirliklere göre ayır
        List<EmojiData> siradanlar = new List<EmojiData>();
        List<EmojiData> enderler = new List<EmojiData>();
        List<EmojiData> destansilar = new List<EmojiData>();

        foreach (var emoji in emojiHavuzu)
        {
            if (emoji == null) continue;

            switch (emoji.nadirlik)
            {
                case EmojiNadirlik.Siradan: siradanlar.Add(emoji); break;
                case EmojiNadirlik.Ender: enderler.Add(emoji); break;
                case EmojiNadirlik.Destansi: destansilar.Add(emoji); break;
                // Efsanevi buraya eklenmez, oyun içinde çıkmayacak
            }
        }

        // 2. Hangi kategoriden seçim yapacağız?
        float toplamSans = sansSiradan + sansEnder + sansDestansi;
        float rastgeleDeger = Random.Range(0, toplamSans);
        
        List<EmojiData> secilenHavuz = null;

        if (rastgeleDeger < sansSiradan)
        {
            secilenHavuz = siradanlar;
        }
        else if (rastgeleDeger < sansSiradan + sansEnder)
        {
            secilenHavuz = enderler;
        }
        else
        {
            secilenHavuz = destansilar;
        }

        // 3. Seçilen havuz boşsa dolu olan başka bir havuza fallback yap (Yedek plan)
        if (secilenHavuz == null || secilenHavuz.Count == 0)
        {
            if (siradanlar.Count > 0) secilenHavuz = siradanlar;
            else if (enderler.Count > 0) secilenHavuz = enderler;
            else if (destansilar.Count > 0) secilenHavuz = destansilar;
            
            // Hiçbiri yoksa null dön
            if (secilenHavuz == null || secilenHavuz.Count == 0) return null;
        }

        // 4. Havuzdan rastgele bir tane seç
        int r = Random.Range(0, secilenHavuz.Count);
        return secilenHavuz[r];
    }

#if UNITY_EDITOR
    /// <summary>
    /// Projedeki tüm EmojiData dosyalarını bulur ve listeye ekler
    /// </summary>
    [ContextMenu("Tüm Emojileri Otomatik Bul")]
    public void TumEmojileriBul()
    {
        emojiHavuzu.Clear();
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:EmojiData");
        
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            EmojiData emoji = UnityEditor.AssetDatabase.LoadAssetAtPath<EmojiData>(path);
            if (emoji != null)
            {
                emojiHavuzu.Add(emoji);
            }
        }
        
        Debug.Log($"[EmojiKurtarma] {emojiHavuzu.Count} emoji bulundu ve listeye eklendi!");
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
    
    /// <summary>
    /// Efsanevi emoji kilidi açıldığında otomatik olarak temizleme ekranına yükler
    /// </summary>
    void OtomatikEfsaneviYukle(EmojiData emoji)
    {
        if (emoji == null) return;
        
        Debug.Log($"[EmojiKurtarma] 🎉 {emoji.emojiAdi} kilidi açıldı! Otomatik olarak temizleme ekranına yükleniyor...");
        
        // Kutlama panelini kapat (eğer açıksa)
        if (kutlamaPanel != null)
            kutlamaPanel.SetActive(false);
        
        // Yeni emojiyi yükle
        emojiIndex = emojiHavuzu.IndexOf(emoji); // Index'i güncelle (eğer havuzda varsa)
        
        // Koleksiyon panelini kapat
        if (koleksiyonPanel != null)
        {
            Debug.Log("[EmojiKurtarma] Koleksiyon paneli kapatılıyor...");
            koleksiyonPanel.KoleksiyonuKapat();
        }
        
        // Temizleme ekranını başlat ve emojiyi yükle
        YeniEmojiYukle(emoji);
    }
    
    /// <summary>
    /// Nadirlik enum değerini Türkçe metne çevirir
    /// </summary>
    private string NadirlikAdiGetir(EmojiNadirlik nadirlik)
    {
        switch (nadirlik)
        {
            case EmojiNadirlik.Siradan:
                return "Sıradan";
            case EmojiNadirlik.Ender:
                return "Ender";
            case EmojiNadirlik.Destansi:
                return "Destansı";
            case EmojiNadirlik.Efsanevi:
                return "Efsanevi";
            default:
                return "Bilinmeyen";
        }
    }
}
