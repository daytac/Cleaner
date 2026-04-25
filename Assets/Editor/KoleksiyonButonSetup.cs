using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

/// <summary>
/// Editor tool to setup the Collection button to open the Collection panel
/// </summary>
public class KoleksiyonButonSetup : EditorWindow
{
    [MenuItem("Tools/Koleksiyon Buton Setup")]
    public static void SetupKoleksiyonButon()
    {
        // Canvas altındaki Koleksiyon butonu ve EmojiKoleksiyonPaneli'ni bul
        GameObject canvasObj = GameObject.Find("Canvas");
        
        if (canvasObj == null)
        {
            Debug.LogError("Canvas bulunamadı!");
            return;
        }

        // Koleksiyon butonunu bul
        Transform koleksiyonTransform = canvasObj.transform.Find("Koleksiyon");
        if (koleksiyonTransform == null)
        {
            Debug.LogError("Koleksiyon butonu bulunamadı!");
            return;
        }
        
        Button koleksiyonButon = koleksiyonTransform.GetComponent<Button>();
        if (koleksiyonButon == null)
        {
            Debug.LogError("Koleksiyon GameObject'inde Button component'i yok!");
            return;
        }

        // EmojiKoleksiyonPaneli'ni bul
        Transform panelTransform = canvasObj.transform.Find("EmojiKoleksiyonPaneli");
        if (panelTransform == null)
        {
            Debug.LogError("EmojiKoleksiyonPaneli bulunamadı!");
            return;
        }
        
        EmojiKoleksiyonUI koleksiyonUI = panelTransform.GetComponent<EmojiKoleksiyonUI>();
        if (koleksiyonUI == null)
        {
            Debug.LogError("EmojiKoleksiyonPaneli'nde EmojiKoleksiyonUI component'i yok!");
            return;
        }

        // Butonu OnClick event'i ayarla
        koleksiyonButon.onClick.RemoveAllListeners();
        
        // Persistent listener ekle (Inspector'da görünür olacak)
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            koleksiyonButon.onClick,
            koleksiyonUI.KoleksiyonuAc
        );

        // GameObject ve sahneyi dirty işaretle
        EditorUtility.SetDirty(koleksiyonButon);
        EditorSceneManager.MarkSceneDirty(koleksiyonButon.gameObject.scene);

        Debug.Log("✅ Koleksiyon butonu başarıyla ayarlandı! Butona tıklandığında koleksiyon paneli açılacak.");
    }
}
