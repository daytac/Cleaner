using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Runtime script to connect the collection button to the collection panel
/// This provides an alternative to Editor-time setup and ensures the button works at runtime
/// </summary>
public class KoleksiyonButonBagla : MonoBehaviour
{
    [Header("Referanslar")]
    [Tooltip("Koleksiyon paneli - Canvas/EmojiKoleksiyonPaneli")]
    public EmojiKoleksiyonUI koleksiyonPanel;
    
    [Tooltip("Koleksiyon butonu - Canvas/Koleksiyon")]
    public Button koleksiyonButon;
    
    void Start()
    {
        // Canvas GameObject'ini bul (her zaman aktif olmalı)
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj == null)
        {
            Debug.LogError("[KoleksiyonButonBagla] Canvas bulunamadı!");
            return;
        }

        // Eğer manuel olarak atanmamışsa, otomatik bul
        if (koleksiyonPanel == null)
        {
            // Transform.Find kullan - inactive GameObjects'i de bulur
            Transform panelTransform = canvasObj.transform.Find("EmojiKoleksiyonPaneli");
            
            if (panelTransform != null)
            {
                koleksiyonPanel = panelTransform.GetComponent<EmojiKoleksiyonUI>();
            }
            
            if (koleksiyonPanel == null)
            {
                Debug.LogError("[KoleksiyonButonBagla] EmojiKoleksiyonPaneli bulunamadı veya EmojiKoleksiyonUI component'i yok!");
                return;
            }
        }
        
        if (koleksiyonButon == null)
        {
            // Transform.Find kullan
            Transform butonTransform = canvasObj.transform.Find("Koleksiyon");
            
            if (butonTransform != null)
            {
                koleksiyonButon = butonTransform.GetComponent<Button>();
            }
            
            if (koleksiyonButon == null)
            {
                Debug.LogError("[KoleksiyonButonBagla] Koleksiyon butonu bulunamadı veya Button component'i yok!");
                return;
            }
        }
        
        // Butonu bağla
        koleksiyonButon.onClick.RemoveAllListeners(); // Önce mevcut listener'ları temizle
        koleksiyonButon.onClick.AddListener(() => 
        {
            Debug.Log("[KoleksiyonButonBagla] Koleksiyon butonu tıklandı, panel açılıyor...");
            koleksiyonPanel.KoleksiyonuAc();
        });
        
        Debug.Log("✅ [KoleksiyonButonBagla] Koleksiyon butonu başarıyla bağlandı!");
    }
}
