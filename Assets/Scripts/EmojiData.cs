using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Emoji Kurtarma Operasyonu - Her emoji için veri yapısı
/// Unity Editor'da Assets > Create > Emoji Kurtarma > Emoji Data ile oluşturulur
/// </summary>
[CreateAssetMenu(fileName = "YeniEmoji", menuName = "Emoji Kurtarma/Emoji Data")]
public class EmojiData : ScriptableObject
{
    [Header("Emoji Bilgileri")]
    [Tooltip("Emojinin adı (örn: Gülen Yüz)")]
    public string emojiAdi = "Yeni Emoji";
    
    [Tooltip("Temiz emoji görseli (kurtarıldıktan sonra gösterilecek)")]
    public Sprite emojiSprite;

    [Header("Nadirlik Seviyesi")]
    [Tooltip("Emojinin nadirlik seviyesi")]
    public EmojiNadirlik nadirlik = EmojiNadirlik.Siradan;
    
    [Header("Efsanevi Özel")]
    [Tooltip("Efsanevi emojiler için gerekli reklam sayısı (Sadece Efsanevi için kullanılır)")]
    public int reklamGereksinimi = 5;
    
    [Header("Temizlik Aşamaları")]
    [Tooltip("Temizlik aşamaları - sırayla uygulanır")]
    public List<EmojiTemizlikAsamasi> asamalar = new List<EmojiTemizlikAsamasi>();
    
    [Header("Son Görüntü")]
    [Tooltip("Temizlik tamamlandığında gösterilecek final texture (kutlamada da kullanılır)")]
    public Texture2D temizTexture;
    
    [Header("Kutlama")]
    [Tooltip("Kurtarma kutlama yazısı")]
    public string kurtarmaYazisi = "Tebrikler! {0}'ı kurtardın!";
    
    [Header("Albüm")]
    [Tooltip("Bu emojinin benzersiz ID'si (kaydetme için)")]
    public string emojiID;
    
    /// <summary>
    /// Kurtarma yazısını emoji adıyla formatla
    /// </summary>
    public string FormatlıKurtarmaYazisi()
    {
        return string.Format(kurtarmaYazisi, emojiAdi);
    }
}

/// <summary>
/// Tek bir temizlik aşaması verisi
/// </summary>
[System.Serializable]
public class EmojiTemizlikAsamasi
{
    [Tooltip("Aşama adı (örn: Çamuru Kazı, Bezle Sil)")]
    public string asamaAdi = "Temizle";
    
    [Tooltip("Bu aşamadaki kirli görüntü")]
    public Texture2D kirliTexture;
    
    [Tooltip("Bu aşamada temizlendikten sonra görünecek")]
    public Texture2D temizTexture;
    
    [Tooltip("Bu aşamada kullanılacak alet modeli")]
    public GameObject aletModeli;
    
    [Tooltip("Fırça/alet şekli")]
    public Texture2D fircaSekli;
    
    [Tooltip("Fırça boyutu")]
    public float fircaBoyutu = 300f;
    
    [Tooltip("Tamamlanma oranı (0.5 - 1.0)")]
    [Range(0.5f, 1f)]
    public float tamamlanmaOrani = 0.99f;
}

/// <summary>
/// Emojilerin nadirlik seviyeleri
/// </summary>
public enum EmojiNadirlik
{
    Siradan,    // Mavi/Yeşil çerçeve
    Ender,      // Mor çerçeve
    Destansi,   // Turuncu/Altın çerçeve
    Efsanevi    // Kırmızı/Özel efektli çerçeve (Reklamla açılır)
}
