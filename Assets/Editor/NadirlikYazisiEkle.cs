using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Kutlama paneline nadirlik yazısı ekler
/// </summary>
public class NadirlikYazisiEkle
{
    [MenuItem("Tools/Kutlama Paneline Nadirlik Yazısı Ekle")]
    public static void NadirlikYazisiOlustur()
    {
        // 1. EmojiKurtarmaYoneticisi'ni bul
        EmojiKurtarmaYoneticisi yonetici = Object.FindFirstObjectByType<EmojiKurtarmaYoneticisi>();
        
        if (yonetici == null)
        {
            Debug.LogError("❌ Sahnede EmojiKurtarmaYoneticisi bulunamadı!");
            return;
        }

        // 2. Kutlama Panelini al
        GameObject panel = yonetici.kutlamaPanel;
        
        if (panel == null)
        {
            Debug.LogError("❌ EmojiKurtarmaYoneticisi'nde 'Kutlama Panel' referansı boş!");
            return;
        }

        // 3. Emoji görselini bul
        RawImage emojiGorsel = yonetici.kurtarilanEmojiGorsel;
        
        if (emojiGorsel == null)
        {
            Debug.LogError("❌ Emoji görseli bulunamadı!");
            return;
        }

        // 4. Nadirlik yazısı zaten var mı kontrol et
        if (yonetici.nadirlikYazisi != null)
        {
            Debug.LogWarning("⚠️ Nadirlik yazısı zaten mevcut! Yeniden oluşturulmayacak.");
            return;
        }

        // 5. Yeni TextMeshPro objesi oluştur
        GameObject nadirlikObj = new GameObject("NadirlikYazisi");
        nadirlikObj.transform.SetParent(panel.transform, false);
        
        // 6. RectTransform ayarları - Emoji görselinin üstünde konumlandır
        RectTransform nadirlikRect = nadirlikObj.AddComponent<RectTransform>();
        RectTransform emojiRect = emojiGorsel.GetComponent<RectTransform>();
        
        // Emoji görselinin pozisyonunu al ve üstüne yerleştir
        nadirlikRect.anchorMin = new Vector2(0.5f, 0.5f);
        nadirlikRect.anchorMax = new Vector2(0.5f, 0.5f);
        nadirlikRect.pivot = new Vector2(0.5f, 0.5f);
        
        // Emoji görselinin üstünde konumlandır (Y pozisyonunu ayarla)
        Vector2 emojiPos = emojiRect.anchoredPosition;
        float emojiHeight = emojiRect.sizeDelta.y;
        nadirlikRect.anchoredPosition = new Vector2(emojiPos.x, emojiPos.y + (emojiHeight / 2) + 40); // 40 piksel yukarıda
        
        nadirlikRect.sizeDelta = new Vector2(400, 60);
        
        // 7. TextMeshProUGUI component ekle
        TMP_Text nadirlikText = nadirlikObj.AddComponent<TextMeshProUGUI>();
        nadirlikText.text = "Sıradan";
        nadirlikText.fontSize = 36;
        nadirlikText.fontStyle = FontStyles.Bold;
        nadirlikText.alignment = TextAlignmentOptions.Center;
        nadirlikText.color = Color.white;
        
        // Outline ekle (daha belirgin olması için)
        nadirlikText.outlineWidth = 0.2f;
        nadirlikText.outlineColor = new Color(0, 0, 0, 0.5f);
        
        // 8. Yöneticiye referansı ata
        yonetici.nadirlikYazisi = nadirlikText;
        
        // 9. Sahneyi kaydet
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );
        
        Debug.Log("✅ Nadirlik yazısı başarıyla eklendi ve emoji görselinin üstüne yerleştirildi!");
        
        // Selection'ı yeni oluşturulan objeye ayarla
        Selection.activeGameObject = nadirlikObj;
    }
}
