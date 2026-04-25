using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class KutlamaButonSetup
{
    [MenuItem("Tools/Kutlama Butonu Ekle")]
    public static void ButonEkle()
    {
        // 1. EmojiKurtarmaYoneticisi'ni bul
        EmojiKurtarmaYoneticisi yonetici = Object.FindFirstObjectByType<EmojiKurtarmaYoneticisi>();
        if (yonetici == null)
        {
            Debug.LogError("EmojiKurtarmaYoneticisi sahnede bulunamadı!");
            return;
        }

        // 2. Kutlama Panelini al
        GameObject panel = yonetici.kutlamaPanel;
        if (panel == null)
        {
            Debug.LogError("EmojiKurtarmaYoneticisi'nde 'Kutlama Panel' referansı boş!");
            return;
        }

        // 3. Butonu oluştur
        GameObject btnObj = new GameObject("KoleksiyonuGorButonu");
        btnObj.transform.SetParent(panel.transform, false);
        
        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(200, 60);
        rt.anchoredPosition = new Vector2(0, -100); // Ortada biraz aşağıda olsun (tahmini)

        Image img = btnObj.AddComponent<Image>();
        img.color = Color.yellow;

        Button btn = btnObj.AddComponent<Button>();

        // Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        
        RectTransform textRT = textObj.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        TextMeshProUGUI txt = textObj.AddComponent<TextMeshProUGUI>();
        txt.text = "Koleksiyonu Gör";
        txt.fontSize = 24;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = Color.black;

        // 4. Referansı atama
        yonetici.koleksiyonuGorButonu = btn;
        EditorUtility.SetDirty(yonetici);

        Debug.Log($"<color=green>Koleksiyonu Gör butonu başarıyla oluşturuldu ve atandı!</color> Panel: {panel.name}");
    }
}
