using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Emoji verileri oluşturma yardımcı scripti
/// Menu: Assets > Emoji Kurtarma > Örnek Emojileri Oluştur
/// </summary>
public class EmojiOlusturucu : Editor
{
    [MenuItem("Assets/Emoji Kurtarma/Örnek Emojileri Oluştur (Çok Aşamalı)")]
    public static void OrnekEmojileriOlustur()
    {
        // Klasörü kontrol et
        if (!AssetDatabase.IsValidFolder("Assets/EmojiVerileri"))
        {
            AssetDatabase.CreateFolder("Assets", "EmojiVerileri");
        }
        
        // Emoji 1: Gülen Yüz - 2 aşamalı
        CreateMultiStageEmoji(
            "GulenYuz",
            "Gülen Yüz",
            "emoji_grinning",
            new string[] {
                "Assets/Game Buffs/Free Realistic Textures/Textures/Cracked_Soil_16/Cracked_Soil_16_Albedo.png",  // Aşama 1 kirli: Çamur
                "Assets/Game Buffs/Free Realistic Textures/Textures/Concrete_3/Concrete_3_Albedo.png"  // Aşama 1 temiz: Tozlu
            },
            new string[] {
                "Assets/Game Buffs/Free Realistic Textures/Textures/Concrete_3/Concrete_3_Albedo.png",  // Aşama 2 kirli: Tozlu
                "Assets/TwemojiGrinningFace.png"  // Aşama 2 temiz: Temiz emoji
            },
            "Assets/TwemojiGrinningFace.png",  // Final temiz görüntü
            new string[] { "Çamuru Kazı", "Bezle Sil" },
            "Tebrikler! Gülen Yüz'ü kurtardın! 😊"
        );
        
        // Emoji 2: Eriyen Yüz - 2 aşamalı
        CreateMultiStageEmoji(
            "EriyenYuz",
            "Eriyen Yüz",
            "emoji_melting",
            new string[] {
                "Assets/Game Buffs/Free Realistic Textures/Textures/Dirty_Brick_Wall_6/Dirty_Brick_Wall_6_Albedo.png",
                "Assets/Game Buffs/Free Realistic Textures/Textures/Beach_Sand_1/Beach_Sand_1_Albedo.png"
            },
            new string[] {
                "Assets/Game Buffs/Free Realistic Textures/Textures/Beach_Sand_1/Beach_Sand_1_Albedo.png",
                "Assets/TwemojiMeltingFace.png"
            },
            "Assets/TwemojiMeltingFace.png",
            new string[] { "Çamuru Kazı", "Bezle Sil" },
            "Tebrikler! Eriyen Yüz'ü kurtardın! 🫠"
        );
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("✅ Çok aşamalı örnek emoji verileri oluşturuldu!");
    }
    
    [MenuItem("Assets/Emoji Kurtarma/Emoji Havuzunu Doldur")]
    public static void EmojiHavuzunuDoldur()
    {
        // Sahnedeki EmojiKurtarmaYoneticisi'ni bul
        EmojiKurtarmaYoneticisi yonetici = Object.FindFirstObjectByType<EmojiKurtarmaYoneticisi>();
        
        if (yonetici == null)
        {
            Debug.LogError("❌ Sahnede EmojiKurtarmaYoneticisi bulunamadı!");
            return;
        }
        
        // Emoji havuzunu temizle
        yonetici.emojiHavuzu.Clear();
        
        // Tüm emoji verilerini yükle
        string[] guids = AssetDatabase.FindAssets("t:EmojiData", new[] { "Assets/EmojiVerileri" });
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            EmojiData emoji = AssetDatabase.LoadAssetAtPath<EmojiData>(path);
            
            if (emoji != null)
            {
                yonetici.emojiHavuzu.Add(emoji);
                Debug.Log($"✅ Emoji eklendi: {emoji.emojiAdi} ({emoji.asamalar.Count} aşama)");
            }
        }
        
        // Sahneyi dirty işaretle (kaydedilmesi gerekiyor)
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );
        
        Debug.Log($"✅ Emoji havuzu dolduruldu! Toplam: {yonetici.emojiHavuzu.Count} emoji");
    }
    
    [MenuItem("Assets/Emoji Kurtarma/Toplu Emoji Oluştur (Yeni Klasör)")]
    public static void TopluEmojiOlustur()
    {
        string kaynakKlasor = "Assets/emojiler/ayri";
        string hedefKlasor = "Assets/EmojiVerileri/Otomatik";
        
        string kirliTexturePath = "Assets/emojiler/texture/kirli/kirli.png";
        string azKirliTexturePath = "Assets/emojiler/texture/az kirli/az kirli.png";

        // Hedef klasör yoksa oluştur
        if (!AssetDatabase.IsValidFolder("Assets/EmojiVerileri"))
        {
            AssetDatabase.CreateFolder("Assets", "EmojiVerileri");
        }
        if (!AssetDatabase.IsValidFolder(hedefKlasor))
        {
            AssetDatabase.CreateFolder("Assets/EmojiVerileri", "Otomatik");
        }

        // Kaynak klasördeki tüm dosyaları bul
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { kaynakKlasor });
        
        Debug.Log($"📂 {kaynakKlasor} içinde {guids.Length} adet görsel bulundu.");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Texture2D emojiTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            
            if (emojiTexture == null) continue;

            string dosyaAdi = emojiTexture.name;
            // Dosya adını temizle (varsa uzantıları veya garip karakterleri)
            string emojiAdi = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dosyaAdi.Replace("_", " ").Replace("-", " "));
            string assetPath = $"{hedefKlasor}/Emoji_{dosyaAdi}.asset";

            // EmojiData oluştur
            EmojiData emoji = ScriptableObject.CreateInstance<EmojiData>();
            emoji.emojiAdi = emojiAdi;
            emoji.emojiID = "emoji_" + dosyaAdi.ToLower();
            emoji.kurtarmaYazisi = "Tebrikler! {0}'ı kurtardın! 🎉";
            emoji.temizTexture = emojiTexture;
            emoji.emojiSprite = Sprite.Create(emojiTexture, new Rect(0, 0, emojiTexture.width, emojiTexture.height), new Vector2(0.5f, 0.5f));

            emoji.asamalar = new List<EmojiTemizlikAsamasi>();

            // Aşama 1: Kirli -> Az Kirli
            var asama1 = new EmojiTemizlikAsamasi();
            asama1.asamaAdi = "Kabayı Temizle";
            asama1.kirliTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(kirliTexturePath);
            asama1.temizTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(azKirliTexturePath);
            asama1.fircaBoyutu = 100f;
            asama1.tamamlanmaOrani = 0.85f;
            
            // Spatula prefabını yükle ve ata
            GameObject spatulaPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/spatulaPrefab.prefab");
            asama1.aletModeli = spatulaPrefab;
            
            emoji.asamalar.Add(asama1);

            // Aşama 2: Az Kirli -> Temiz
            var asama2 = new EmojiTemizlikAsamasi();
            asama2.asamaAdi = "İnce Temizlik";
            asama2.kirliTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(azKirliTexturePath);
            asama2.temizTexture = emojiTexture;
            asama2.fircaBoyutu = 120f;
            asama2.tamamlanmaOrani = 0.90f;
            emoji.asamalar.Add(asama2);

            // Varsa eskisini sil
            EmojiData eskiAsset = AssetDatabase.LoadAssetAtPath<EmojiData>(assetPath);
            if (eskiAsset != null)
            {
                AssetDatabase.DeleteAsset(assetPath);
            }

            AssetDatabase.CreateAsset(emoji, assetPath);
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"✅ Toplu emoji oluşturma tamamlandı! {guids.Length} adet emoji {hedefKlasor} klasörüne kaydedildi.");
    }

    private static void CreateMultiStageEmoji(
        string dosyaAdi, 
        string emojiAdi, 
        string emojiID, 
        string[] asama1Textures,  // [kirli, temiz]
        string[] asama2Textures,  // [kirli, temiz]
        string finalTexturePath,
        string[] asamaAdlari,
        string kurtarmaYazisi)
    {
        string assetPath = $"Assets/EmojiVerileri/{dosyaAdi}.asset";
        
        // Mevcut emojiyi sil ve yeniden oluştur
        EmojiData mevcutEmoji = AssetDatabase.LoadAssetAtPath<EmojiData>(assetPath);
        if (mevcutEmoji != null)
        {
            AssetDatabase.DeleteAsset(assetPath);
        }
        
        EmojiData emoji = ScriptableObject.CreateInstance<EmojiData>();
        emoji.emojiAdi = emojiAdi;
        emoji.emojiID = emojiID;
        emoji.kurtarmaYazisi = kurtarmaYazisi;
        
        // Final texture
        emoji.temizTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(finalTexturePath);
        
        // Aşamaları oluştur
        emoji.asamalar = new List<EmojiTemizlikAsamasi>();
        
        // Aşama 1
        var asama1 = new EmojiTemizlikAsamasi();
        asama1.asamaAdi = asamaAdlari.Length > 0 ? asamaAdlari[0] : "Aşama 1";
        asama1.kirliTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(asama1Textures[0]);
        asama1.temizTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(asama1Textures[1]);
        asama1.fircaBoyutu = 100f;
        asama1.tamamlanmaOrani = 0.85f;
        
        // Spatula prefabını yükle ve ata (Aşama 1 için varsayılan)
        GameObject spatulaPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/spatulaPrefab.prefab");
        asama1.aletModeli = spatulaPrefab;
        
        emoji.asamalar.Add(asama1);
        
        // Aşama 2
        var asama2 = new EmojiTemizlikAsamasi();
        asama2.asamaAdi = asamaAdlari.Length > 1 ? asamaAdlari[1] : "Aşama 2";
        asama2.kirliTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(asama2Textures[0]);
        asama2.temizTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(asama2Textures[1]);
        asama2.fircaBoyutu = 120f;
        asama2.tamamlanmaOrani = 0.90f;
        emoji.asamalar.Add(asama2);
        
        AssetDatabase.CreateAsset(emoji, assetPath);
        Debug.Log($"✅ Çok aşamalı emoji oluşturuldu: {emojiAdi} ({emoji.asamalar.Count} aşama)");
    }
    
    [MenuItem("Assets/Emoji Kurtarma/Mevcut Aletleri Güncelle (Spatula)")]
    public static void GuncelleTumAletler()
    {
        GameObject spatulaPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/spatulaPrefab.prefab");
        
        if (spatulaPrefab == null)
        {
            Debug.LogError("❌ Spatula prefabı bulunamadı! 'Assets/spatulaPrefab.prefab' yolunu kontrol edin.");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:EmojiData");
        int count = 0;
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            EmojiData emoji = AssetDatabase.LoadAssetAtPath<EmojiData>(path);
            
            if (emoji != null && emoji.asamalar.Count > 0)
            {
                // Sadece ilk aşamaya spatula ata
                emoji.asamalar[0].aletModeli = spatulaPrefab;
                EditorUtility.SetDirty(emoji);
                count++;
            }
        }
        
        AssetDatabase.SaveAssets();
        Debug.Log($"✅ {count} adet emojinin ilk aşamasına Spatula atandı!");
    }
}
