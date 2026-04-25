using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Emoji Koleksiyon Ekranı Yöneticisi
/// </summary>
public class EmojiKoleksiyonUI : MonoBehaviour
{
    [Header("Referanslar")]
    public Transform contentGrid; // Emojilerin listeleneceği grid
    public GameObject emojiOgesiPrefab; // EmojiKoleksiyonOgesi prefab'ı
    
    [Header("Tab Butonları")]
    public Button tabSiradan;
    public Button tabEnder;
    public Button tabDestansi;
    public Button tabEfsanevi;
    
    [Header("Tab Görselleri (Seçili/Seçisiz durumu için)")]
    public Color seciliTabRengi = Color.white;
    public Color normalTabRengi = Color.gray;
    
    [Header("Kontroller")]
    public Button kapatButonu;
    
    private EmojiNadirlik suankiKategori = EmojiNadirlik.Siradan;
    
    void Start()
    {
        // Butonları ayarla
        tabSiradan.onClick.AddListener(() => KategoriDegistir(EmojiNadirlik.Siradan));
        tabEnder.onClick.AddListener(() => KategoriDegistir(EmojiNadirlik.Ender));
        tabDestansi.onClick.AddListener(() => KategoriDegistir(EmojiNadirlik.Destansi));
        tabEfsanevi.onClick.AddListener(() => KategoriDegistir(EmojiNadirlik.Efsanevi));
        
        if (kapatButonu != null)
            kapatButonu.onClick.AddListener(KoleksiyonuKapat);
        
        // İlk açılış - Kapalı başlat
        gameObject.SetActive(false);
    }
    
    void KategoriDegistir(EmojiNadirlik yeniKategori)
    {
        suankiKategori = yeniKategori;
        TabGorselleriniGuncelle();
        ListeyiYenile();
    }
    
    void TabGorselleriniGuncelle()
    {
        // Basit renk değişimi
        tabSiradan.image.color = suankiKategori == EmojiNadirlik.Siradan ? seciliTabRengi : normalTabRengi;
        tabEnder.image.color = suankiKategori == EmojiNadirlik.Ender ? seciliTabRengi : normalTabRengi;
        tabDestansi.image.color = suankiKategori == EmojiNadirlik.Destansi ? seciliTabRengi : normalTabRengi;
        tabEfsanevi.image.color = suankiKategori == EmojiNadirlik.Efsanevi ? seciliTabRengi : normalTabRengi;
    }
    
    public void ListeyiYenile()
    {
        // Önce temizle
        foreach (Transform child in contentGrid)
        {
            Destroy(child.gameObject);
        }
        
        // Albümden verileri al
        if (EmojiAlbumu.Instance == null)
        {
            Debug.LogError("EmojiAlbumu Instance bulunamadı!");
            return;
        }
        
        List<EmojiData> filtrelenmisListe = EmojiAlbumu.Instance.GetEmojisByRarity(suankiKategori);
        
        if (filtrelenmisListe.Count == 0)
        {
            // Boş durum mesajı eklenebilir
            Debug.Log($"Bu kategoride emoji yok: {suankiKategori}");
        }
        
        foreach (EmojiData data in filtrelenmisListe)
        {
            GameObject ogeObj = Instantiate(emojiOgesiPrefab, contentGrid);
            EmojiKoleksiyonOgesi ogeScript = ogeObj.GetComponent<EmojiKoleksiyonOgesi>();
            
            bool kurtarildi = EmojiAlbumu.Instance.EmojiKurtarildiMi(data.emojiID);
            ogeScript.Ayarla(data, kurtarildi);
        }

        // Layout'u güncellemeye zorla (UI bozukluklarını önler)
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentGrid.GetComponent<RectTransform>());
    }
    
    /// <summary>
    /// Koleksiyon panelini aç
    /// </summary>
    public void KoleksiyonuAc()
    {
        gameObject.SetActive(true);
        ListeyiYenile();
    }
    
    /// <summary>
    /// Koleksiyon panelini kapat
    /// </summary>
    public void KoleksiyonuKapat()
    {
        gameObject.SetActive(false);
    }
}
