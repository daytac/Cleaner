using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class KoleksiyonSetupTools
{
    [MenuItem("Tools/Koleksiyon UI Kurulumu")]
    public static void KurulumYap()
    {
        // 1. Klasör Kontrolü
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/UI"))
            AssetDatabase.CreateFolder("Assets/Prefabs", "UI");

        // 2. Canvas Bul veya Oluştur
        Canvas mainCanvas = GameObject.FindFirstObjectByType<Canvas>();
        if (mainCanvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            mainCanvas = canvasObj.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0f; // Genişliğe göre ölçekle (Portrait mod için ideal)
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // 3. Emoji Item Prefab Oluşturma (Sahneye geçici olarak)
        GameObject itemRoot = new GameObject("EmojiItem_Template");
        itemRoot.transform.SetParent(mainCanvas.transform, false);
        
        // Arkaplan / Buton
        Image bgImage = itemRoot.AddComponent<Image>();
        Button mainBtn = itemRoot.AddComponent<Button>();
        
        // Emoji Görseli
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(itemRoot.transform, false);
        RectTransform iconRT = iconObj.AddComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0f, 0.15f); // Tam genişlik, yazı için alt boşluk
        iconRT.anchorMax = new Vector2(1f, 1f);
        iconRT.offsetMin = Vector2.zero;
        iconRT.offsetMax = Vector2.zero;
        Image iconImg = iconObj.AddComponent<Image>();
        
        // İsim Text - İPTAL EDİLDİ (Kullanıcı isteğiyle kaldırıldı)
        /*
        GameObject textObj = new GameObject("NameText");
        textObj.transform.SetParent(itemRoot.transform, false);
        // ... (Kod kaldırıldı)
        */

        // Kilit Overlay
        GameObject lockObj = new GameObject("LockOverlay");
        lockObj.transform.SetParent(itemRoot.transform, false);
        RectTransform lockRT = lockObj.AddComponent<RectTransform>();
        lockRT.anchorMin = Vector2.zero;
        lockRT.anchorMax = Vector2.one; 
        Image lockImg = lockObj.AddComponent<Image>();
        lockImg.color = new Color(0, 0, 0, 0.7f);
        lockObj.SetActive(false); // Varsayılan kapalı

        // Reklam Butonu
        GameObject adBtnObj = new GameObject("AdButton");
        adBtnObj.transform.SetParent(itemRoot.transform, false);
        RectTransform adRT = adBtnObj.AddComponent<RectTransform>();
        adRT.sizeDelta = new Vector2(100, 40);
        Image adImg = adBtnObj.AddComponent<Image>();
        adImg.color = Color.green;
        Button adBtn = adBtnObj.AddComponent<Button>();
        
        GameObject adTextObj = new GameObject("Text");
        adTextObj.transform.SetParent(adBtnObj.transform, false);
        TextMeshProUGUI adText = adTextObj.AddComponent<TextMeshProUGUI>();
        adText.text = "Reklam İzle";
        adText.alignment = TextAlignmentOptions.Center;
        adText.enableAutoSizing = true;
        adText.fontSizeMin = 8;
        adText.fontSizeMax = 18;
        adBtnObj.SetActive(false);

        // Component Ekleme ve Ayarlama
        EmojiKoleksiyonOgesi itemScript = itemRoot.AddComponent<EmojiKoleksiyonOgesi>();
        itemScript.emojiGorseli = iconImg;
        // itemScript.emojiAdiText = nameText; // Text iptal edildi
        itemScript.kilitIconu = lockObj;
        itemScript.buOgeButonu = mainBtn;
        itemScript.reklamButonuObjesi = adBtnObj;
        itemScript.reklamButonu = adBtn;

        // Prefab Olarak Kaydet
        string prefabPath = "Assets/Prefabs/UI/EmojiOgesi.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(itemRoot, prefabPath);
        GameObject.DestroyImmediate(itemRoot); // Sahneden temizle

        Debug.Log("Prefab oluşturuldu: " + prefabPath);

        // 3.5 Temizlik (Eski panel ve görselleri sil)
        Transform existingPanel = mainCanvas.transform.Find("EmojiKoleksiyonPaneli");
        while (existingPanel != null)
        {
            GameObject.DestroyImmediate(existingPanel.gameObject);
            existingPanel = mainCanvas.transform.Find("EmojiKoleksiyonPaneli");
        }
        
        Transform existingAnimImg = mainCanvas.transform.Find("EmojiAnimasyonImage");
        if (existingAnimImg != null) GameObject.DestroyImmediate(existingAnimImg.gameObject);

        // 4. Ana UI Oluşturma
        GameObject panelObj = new GameObject("EmojiKoleksiyonPaneli");
        panelObj.transform.SetParent(mainCanvas.transform, false);
        RectTransform panelRT = panelObj.AddComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.1f, 0.1f);
        panelRT.anchorMax = new Vector2(0.9f, 0.9f);
        Image panelImg = panelObj.AddComponent<Image>();
        panelImg.color = new Color(0.9f, 0.9f, 0.9f);

        // Close Button (X)
        GameObject closeBtn = new GameObject("CloseButton");
        closeBtn.transform.SetParent(panelObj.transform, false);
        RectTransform closeBtnRT = closeBtn.AddComponent<RectTransform>();
        closeBtnRT.anchorMin = new Vector2(1, 1);
        closeBtnRT.anchorMax = new Vector2(1, 1);
        closeBtnRT.pivot = new Vector2(1, 1);
        closeBtnRT.anchoredPosition = new Vector2(-10, -10);
        closeBtnRT.sizeDelta = new Vector2(40, 40);
        Image closeBtnImg = closeBtn.AddComponent<Image>();
        closeBtnImg.color = Color.red;
        Button closeBtnButton = closeBtn.AddComponent<Button>();
        
        GameObject closeBtnText = new GameObject("Text");
        closeBtnText.transform.SetParent(closeBtn.transform, false);
        TextMeshProUGUI closeBtnTxt = closeBtnText.AddComponent<TextMeshProUGUI>();
        closeBtnTxt.text = "X";
        closeBtnTxt.fontSize = 24;
        closeBtnTxt.alignment = TextAlignmentOptions.Center;
        closeBtnTxt.color = Color.white;

        // Tabs Container
        GameObject tabsContainer = new GameObject("Tabs");
        tabsContainer.transform.SetParent(panelObj.transform, false);
        RectTransform tabsRT = tabsContainer.AddComponent<RectTransform>();
        tabsRT.anchorMin = new Vector2(0, 1);
        tabsRT.anchorMax = new Vector2(1, 1);
        tabsRT.pivot = new Vector2(0.5f, 1);
        tabsRT.sizeDelta = new Vector2(0, 50);
        tabsRT.anchoredPosition = new Vector2(0, -10);
        HorizontalLayoutGroup tabsHLG = tabsContainer.AddComponent<HorizontalLayoutGroup>();
        tabsHLG.childControlWidth = true;
        tabsHLG.childForceExpandWidth = true;
        tabsHLG.spacing = 10;

        // Tab Butonları Oluşturma
        Button b1 = CreateTabButton("Siradan", Color.cyan, tabsContainer.transform);
        Button b2 = CreateTabButton("Ender", Color.magenta, tabsContainer.transform);
        Button b3 = CreateTabButton("Destansi", Color.yellow, tabsContainer.transform);
        Button b4 = CreateTabButton("Efsanevi", Color.red, tabsContainer.transform);

        // Scroll View / Grid
        GameObject scrollObj = new GameObject("ScrollView");
        scrollObj.transform.SetParent(panelObj.transform, false);
        RectTransform scrollRT = scrollObj.AddComponent<RectTransform>();
        scrollRT.anchorMin = new Vector2(0.05f, 0.05f);
        scrollRT.anchorMax = new Vector2(0.95f, 0.85f);
        
        ScrollRect scrollRect = scrollObj.AddComponent<ScrollRect>();
        scrollRect.horizontal = false; // Sadece dikey kaydırma
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Elastic; // Elastik kaydırma
        scrollRect.decelerationRate = 0.135f; // Yumuşak duruş
        
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollObj.transform, false);
        RectTransform viewRT = viewport.AddComponent<RectTransform>();
        viewRT.anchorMin = Vector2.zero;
        viewRT.anchorMax = Vector2.one;
        viewRT.offsetMin = Vector2.zero; // Sol/Alt sıfırla
        viewRT.offsetMax = Vector2.zero; // Sağ/Üst sıfırla
        viewport.AddComponent<Mask>();
        Image viewImg = viewport.AddComponent<Image>();
        viewImg.color = new Color(1,1,1,0.1f);

        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRT = content.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        
        GridLayoutGroup grid = content.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(480, 600); // 1080px için olabilecek en büyük boyut
        grid.spacing = new Vector2(80, 80); // Geniş aralık
        grid.padding = new RectOffset(20, 20, 40, 40); // Kenar boşlukları azaltıldı
        grid.childAlignment = TextAnchor.UpperCenter; 
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 2; // 2 Sütun
        
        ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.content = contentRT;
        scrollRect.viewport = viewRT;

        // Ana Scripti Ekleme
        EmojiKoleksiyonUI mainScript = panelObj.AddComponent<EmojiKoleksiyonUI>();
        mainScript.contentGrid = content.transform;
        mainScript.emojiOgesiPrefab = prefab;
        mainScript.tabSiradan = b1;
        mainScript.tabEnder = b2;
        mainScript.tabDestansi = b3;
        mainScript.tabEfsanevi = b4;
        mainScript.seciliTabRengi = Color.white;
        mainScript.normalTabRengi = Color.gray;
        mainScript.kapatButonu = closeBtnButton;

        // 5. Animasyon Image Oluşturma
        GameObject animObj = new GameObject("EmojiAnimasyonImage");
        animObj.transform.SetParent(mainCanvas.transform, false);
        RectTransform animRT = animObj.AddComponent<RectTransform>();
        animRT.sizeDelta = new Vector2(200, 200);
        animRT.anchoredPosition = Vector2.zero;
        RawImage animImg = animObj.AddComponent<RawImage>();
        animImg.color = Color.white;
        animObj.SetActive(false); // Varsayılan kapalı
        
        // Animasyon Scripti (Varsa al, yoksa ekle)
        EmojiKoleksiyonAnimasyon animScript = mainCanvas.GetComponent<EmojiKoleksiyonAnimasyon>();
        if (animScript == null) 
            animScript = mainCanvas.gameObject.AddComponent<EmojiKoleksiyonAnimasyon>();
            
        animScript.animasyonImage = animImg;
        animScript.koleksiyonPanelObj = panelObj;

        // 6. Yöneticide Otomatik Bağlama
        EmojiKurtarmaYoneticisi yonetici = Object.FindFirstObjectByType<EmojiKurtarmaYoneticisi>();
        if (yonetici != null)
        {
            yonetici.koleksiyonPanelObj = panelObj;
            yonetici.koleksiyonAnimasyon = animScript;
            EditorUtility.SetDirty(yonetici);
            Debug.Log("EmojiKurtarmaYoneticisi referansları otomatik bağlandı!");
        }

        Debug.Log("Koleksiyon UI kurulumu tamamlandı!");
    }

    private static Button CreateTabButton(string name, Color color, Transform parent)
    {
        GameObject btnObj = new GameObject("Tab_" + name);
        btnObj.transform.SetParent(parent, false);
        Image img = btnObj.AddComponent<Image>();
        img.color = color;
        Button btn = btnObj.AddComponent<Button>();
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        RectTransform rt = textObj.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        TextMeshProUGUI txt = textObj.AddComponent<TextMeshProUGUI>();
        txt.text = name;
        txt.alignment = TextAlignmentOptions.Center;
        txt.fontSize = 18;
        txt.enableAutoSizing = true; // Tab yazıları için de auto size
        txt.fontSizeMin = 10;
        txt.fontSizeMax = 24;
        txt.color = Color.black;
        
        return btn;
    }
}

public class ResetRectTransform : MonoBehaviour
{
    public void Reset()
    {
        RectTransform rt = GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        DestroyImmediate(this);
    }
}