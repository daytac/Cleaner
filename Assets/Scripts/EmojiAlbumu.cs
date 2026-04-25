using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Emoji Albümü - Kurtarılan emojileri takip eder ve kaydeder
/// PlayerPrefs kullanarak kalıcı kayıt sağlar
/// </summary>
public class EmojiAlbumu : MonoBehaviour
{
    [Header("Tüm Emojiler")]
    [Tooltip("Oyundaki tüm emoji verileri")]
    public List<EmojiData> tumEmojiler = new List<EmojiData>();
    
    [Header("Debug")]
    public bool debugModu = false;
    
    // Kurtarılan emoji ID'leri
    private HashSet<string> kurtarilanEmojiIDleri = new HashSet<string>();
    
    // PlayerPrefs anahtarı
    private const string KAYIT_ANAHTARI = "KurtarilanEmojiler";
    
    public static EmojiAlbumu Instance { get; private set; }
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        AlbumuYukle();
    }
    
    /// <summary>
    /// Emojiyi kurtarılmış olarak işaretle
    /// </summary>
    public void EmojiKurtar(string emojiID)
    {
        if (string.IsNullOrEmpty(emojiID)) return;
        
        if (!kurtarilanEmojiIDleri.Contains(emojiID))
        {
            kurtarilanEmojiIDleri.Add(emojiID);
            AlbumuKaydet();
            
            if (debugModu)
                Debug.Log($"[EmojiAlbumu] Yeni emoji kurtarıldı: {emojiID}");
        }
    }
    
    /// <summary>
    /// Emojiyi kurtarılmış olarak işaretle (EmojiData ile)
    /// </summary>
    public void EmojiKurtar(EmojiData emoji)
    {
        if (emoji != null)
            EmojiKurtar(emoji.emojiID);
    }
    
    /// <summary>
    /// Emoji daha önce kurtarıldı mı?
    /// </summary>
    public bool EmojiKurtarildiMi(string emojiID)
    {
        return kurtarilanEmojiIDleri.Contains(emojiID);
    }
    
    /// <summary>
    /// Emoji daha önce kurtarıldı mı? (EmojiData ile)
    /// </summary>
    public bool EmojiKurtarildiMi(EmojiData emoji)
    {
        return emoji != null && EmojiKurtarildiMi(emoji.emojiID);
    }
    
    /// <summary>
    /// Toplam kurtarılan emoji sayısı
    /// </summary>
    public int KurtarilanSayisi()
    {
        return kurtarilanEmojiIDleri.Count;
    }
    
    /// <summary>
    /// Toplam emoji sayısı
    /// </summary>
    public int ToplamEmojiSayisi()
    {
        return tumEmojiler.Count;
    }
    
    /// <summary>
    /// İlerleme yüzdesi (0-1 arası)
    /// </summary>
    public float IlerlemeYuzdesi()
    {
        if (tumEmojiler.Count == 0) return 0f;
        return (float)kurtarilanEmojiIDleri.Count / tumEmojiler.Count;
    }
    
    /// <summary>
    /// Albümü PlayerPrefs'e kaydet
    /// </summary>
    public void AlbumuKaydet()
    {
        string kayitString = string.Join(",", kurtarilanEmojiIDleri);
        PlayerPrefs.SetString(KAYIT_ANAHTARI, kayitString);
        PlayerPrefs.Save();
        
        if (debugModu)
            Debug.Log($"[EmojiAlbumu] Kaydedildi: {kayitString}");
    }
    
    /// <summary>
    /// Albümü PlayerPrefs'ten yükle
    /// </summary>
    public void AlbumuYukle()
    {
        kurtarilanEmojiIDleri.Clear();
        
        string kayitString = PlayerPrefs.GetString(KAYIT_ANAHTARI, "");
        
        if (!string.IsNullOrEmpty(kayitString))
        {
            string[] idler = kayitString.Split(',');
            foreach (string id in idler)
            {
                if (!string.IsNullOrEmpty(id))
                    kurtarilanEmojiIDleri.Add(id);
            }
        }
        
        if (debugModu)
            Debug.Log($"[EmojiAlbumu] Yüklendi: {kurtarilanEmojiIDleri.Count} emoji");
    }
    
    /// <summary>
    /// Albümü sıfırla (debug için)
    /// </summary>
    
    // Efsanevi kilit ID'leri (Açılanlar)
    private HashSet<string> acilanEfsaneviIDleri = new HashSet<string>();
    private const string EFSANEVI_KAYIT_ANAHTARI = "AcilanEfsaneviler";
    
    // Efsanevi açıldığında tetiklenen event
    public static event System.Action<EmojiData> OnEfsaneviAcildi;

    public void EfsaneviKilitAc(string emojiID)
    {
        if (string.IsNullOrEmpty(emojiID)) return;
        
        if (!acilanEfsaneviIDleri.Contains(emojiID))
        {
            acilanEfsaneviIDleri.Add(emojiID);
            EfsaneviKaydet();
            
            if (debugModu)
                Debug.Log($"[EmojiAlbumu] Efsanevi kilit açıldı: {emojiID}");
        }
    }
    
    /// <summary>
    /// Efsanevi kilidi aç ve temizleme oyununu başlatmak için event tetikle
    /// </summary>
    public void EfsaneviKilitAcVeOyunBaslat(string emojiID)
    {
        // Önce kilidi aç
        EfsaneviKilitAc(emojiID);
        
        // Emoji verisini bul
        EmojiData emoji = GetEmojiByID(emojiID);
        if (emoji != null)
        {
            // Event'i tetikle - EmojiKurtarmaYoneticisi bunu dinliyor
            OnEfsaneviAcildi?.Invoke(emoji);
            
            if (debugModu)
                Debug.Log($"[EmojiAlbumu] OnEfsaneviAcildi eventi tetiklendi: {emoji.emojiAdi}");
        }
    }

    public bool EfsaneviKilitAcikMi(string emojiID)
    {
        return acilanEfsaneviIDleri.Contains(emojiID);
    }
    
    // Reklam metodları kaldırıldı
    
    /// <summary>
    /// Emoji ID'sine göre EmojiData bul
    /// </summary>
    public EmojiData GetEmojiByID(string emojiID)
    {
        foreach (var emoji in tumEmojiler)
        {
            if (emoji.emojiID == emojiID)
                return emoji;
        }
        return null;
    }
    
    public List<EmojiData> GetEmojisByRarity(EmojiNadirlik rarity)
    {
        List<EmojiData> filteredList = new List<EmojiData>();
        foreach (var emoji in tumEmojiler)
        {
            if (emoji.nadirlik == rarity)
            {
                filteredList.Add(emoji);
            }
        }
        return filteredList;
    }

    private void EfsaneviKaydet()
    {
        string kayitString = string.Join(",", acilanEfsaneviIDleri);
        PlayerPrefs.SetString(EFSANEVI_KAYIT_ANAHTARI, kayitString);
        PlayerPrefs.Save();
    }
    

    private void EfsaneviYukle()
    {
        acilanEfsaneviIDleri.Clear();
        string kayitString = PlayerPrefs.GetString(EFSANEVI_KAYIT_ANAHTARI, "");
        if (!string.IsNullOrEmpty(kayitString))
        {
            string[] idler = kayitString.Split(',');
            foreach (string id in idler)
            {
                if (!string.IsNullOrEmpty(id))
                    acilanEfsaneviIDleri.Add(id);
            }
        }
    }
    

    private void Start()
    {
        EfsaneviYukle();
        // IlerlemeYukle(); // Kaldırıldı
    }
    
    [ContextMenu("Albümü Sıfırla")]
    public void AlbumuSifirla()
    {
        kurtarilanEmojiIDleri.Clear();
        acilanEfsaneviIDleri.Clear();
        // efsaneviReklamIlerlemeleri.Clear(); // Dictionary removed
        PlayerPrefs.DeleteKey(KAYIT_ANAHTARI);
        PlayerPrefs.DeleteKey(EFSANEVI_KAYIT_ANAHTARI);
        // PlayerPrefs.DeleteKey("EfsaneviIlerlemeler"); // Key const removed
        PlayerPrefs.Save();
        
        Debug.Log("[EmojiAlbumu] Albüm sıfırlandı!");
    }

#if UNITY_EDITOR
    /// <summary>
    /// Projedeki tüm EmojiData dosyalarını bulur ve listeye ekler
    /// </summary>
    [ContextMenu("Tüm Emojileri Otomatik Bul")]
    public void TumEmojileriBul()
    {
        tumEmojiler.Clear();
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:EmojiData");
        
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            EmojiData emoji = UnityEditor.AssetDatabase.LoadAssetAtPath<EmojiData>(path);
            if (emoji != null)
            {
                tumEmojiler.Add(emoji);
            }
        }
        
        Debug.Log($"[EmojiAlbumu] {tumEmojiler.Count} emoji bulundu ve albüme eklendi!");
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}
