using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// Emoji kurtarma animasyonu - Emoji görseli koleksiyon paneline uçar
/// </summary>
public class EmojiKoleksiyonAnimasyon : MonoBehaviour
{
    [Header("Referanslar")]
    [Tooltip("Animasyon için kullanılacak geçici RawImage")]
    public RawImage animasyonImage;
    
    [Tooltip("Koleksiyon paneli GameObject'i")]
    public GameObject koleksiyonPanelObj;
    
    [Tooltip("Animasyon başladığında çalacak ses (correct sound)")]
    public AudioSource tamamlanmaSesi;
    
    [Header("Animasyon Ayarları")]
    [Tooltip("Animasyon süresi (saniye)")]
    public float animasyonSuresi = 1.0f;
    
    [Tooltip("Başlangıç ölçeği")]
    public float baslangicOlcegi = 1.5f;
    
    [Tooltip("Bitiş ölçeği")]
    public float bitisOlcegi = 0.3f;
    
    private bool animasyonOynuyor = false;
    private float animasyonZamani = 0f;
    private Vector3 baslangicPozisyon;
    private Vector3 hedefPozisyon;
    private Texture2D aktifTexture;
    private EmojiKoleksiyonUI koleksiyonPanel;
    
    // Animasyon bitince çağrılacak callback
    public event Action OnAnimasyonBitti;
    
    /// <summary>
    /// Emoji kurtarma animasyonunu başlat
    /// </summary>
    public void AnimasyonBaslat(Texture2D emojiTexture, RectTransform baslangicRect)
    {
        // Koleksiyon paneli component'ini al (ilk çağrıda)
        if (koleksiyonPanel == null && koleksiyonPanelObj != null)
            koleksiyonPanel = koleksiyonPanelObj.GetComponent<EmojiKoleksiyonUI>();
        
        if (animasyonImage == null)
        {
            Debug.LogError("[EmojiKoleksiyonAnimasyon] AnimasyonImage referansı eksik!");
            return;
        }
        
        // Texture'ı ayarla
        aktifTexture = emojiTexture;
        animasyonImage.texture = emojiTexture;
        animasyonImage.gameObject.SetActive(true);
        
        // Pozisyonları ayarla
        RectTransform animRect = animasyonImage.GetComponent<RectTransform>();
        
        if (baslangicRect != null)
        {
            // Başlangıç pozisyonunu kutlama panelindeki emoji pozisyonundan al
            baslangicPozisyon = baslangicRect.position;
        }
        else
        {
            // Varsayılan: Ekranın ortası
            baslangicPozisyon = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        }
        
        // Hedef: Koleksiyon panelinin sağ üst köşesi (isteğe bağlı olarak değiştirilebilir)
        hedefPozisyon = new Vector3(Screen.width * 0.85f, Screen.height * 0.85f, 0);
        
        // Başlangıç durumunu ayarla
        animRect.position = baslangicPozisyon;
        animRect.localScale = Vector3.one * baslangicOlcegi;
        
        // Animasyonu başlat
        animasyonOynuyor = true;
        animasyonZamani = 0f;
        
        // Tamamlanma sesi çal
        if (tamamlanmaSesi != null)
        {
            Debug.Log("[EmojiKoleksiyonAnimasyon] Tamamlanma sesi çalıyor! Volume: " + tamamlanmaSesi.volume);
            AudioManager.Instance.PlaySFX(tamamlanmaSesi);
        }
        else
        {
            Debug.LogWarning("[EmojiKoleksiyonAnimasyon] Tamamlanma sesi NULL! Inspector'dan AudioSource ata.");
        }
    }
    
    void Update()
    {
        if (!animasyonOynuyor) return;
        
        animasyonZamani += Time.deltaTime;
        float t = animasyonZamani / animasyonSuresi;
        
        if (t >= 1f)
        {
            // Animasyon tamamlandı
            AnimasyonTamamlandi();
            return;
        }
        
        // Ease-out cubic eğrisi
        float easeT = 1f - Mathf.Pow(1f - t, 3f);
        
        // Pozisyonu güncelle
        RectTransform animRect = animasyonImage.GetComponent<RectTransform>();
        animRect.position = Vector3.Lerp(baslangicPozisyon, hedefPozisyon, easeT);
        
        // Ölçeği güncelle
        float currentScale = Mathf.Lerp(baslangicOlcegi, bitisOlcegi, easeT);
        animRect.localScale = Vector3.one * currentScale;
        
        // Opsiyonel: Hafif rotasyon ekle
        animRect.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(0, 360, easeT));
    }
    
    void AnimasyonTamamlandi()
    {
        animasyonOynuyor = false;
        
        // Geçici image'ı gizle
        if (animasyonImage != null)
            animasyonImage.gameObject.SetActive(false);
        
        // Koleksiyon panelini aç (Artık açmıyoruz, kutlama paneli görünecek)
        /*if (koleksiyonPanel != null)
        {
            koleksiyonPanel.KoleksiyonuAc();
        }*/
        
        // Callback'i çağır (kutlama paneli için)
        OnAnimasyonBitti?.Invoke();
        
        Debug.Log("[EmojiKoleksiyonAnimasyon] Animasyon tamamlandı!");
    }
}
