using UnityEngine;
using UnityEditor;
using System.IO;

public class EmojiNadirlikAyarlayici
{
    [MenuItem("Tools/Emoji Nadirlik Ayarla")]
    public static void NadirliklariAyarla()
    {
        int siradan = 0, ender = 0, destansi = 0, efsanevi = 0;
        
        // Tüm EmojiData dosyalarını bul
        string[] guids = AssetDatabase.FindAssets("t:EmojiData", new[] { "Assets" });
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            EmojiData emoji = AssetDatabase.LoadAssetAtPath<EmojiData>(path);
            
            if (emoji == null) continue;
            
            // Klasör adından nadirlik belirle
            EmojiNadirlik yeniNadirlik = EmojiNadirlik.Siradan; // Varsayılan
            
            if (path.Contains("1_siradan"))
            {
                yeniNadirlik = EmojiNadirlik.Siradan;
                siradan++;
            }
            else if (path.Contains("2_ender"))
            {
                yeniNadirlik = EmojiNadirlik.Ender;
                ender++;
            }
            else if (path.Contains("3_destansi"))
            {
                yeniNadirlik = EmojiNadirlik.Destansi;
                destansi++;
            }
            else if (path.Contains("4_efsanevi"))
            {
                yeniNadirlik = EmojiNadirlik.Efsanevi;
                efsanevi++;
            }
            
            // Nadirlik atama (SerializedObject kullanarak)
            SerializedObject so = new SerializedObject(emoji);
            SerializedProperty prop = so.FindProperty("nadirlik");
            
            if (prop != null)
            {
                prop.enumValueIndex = (int)yeniNadirlik;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(emoji);
            }
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"<color=green>[Nadirlik Ayarlandı]</color>\n" +
                  $"Sıradan: {siradan}\n" +
                  $"Ender: {ender}\n" +
                  $"Destansı: {destansi}\n" +
                  $"Efsanevi: {efsanevi}\n" +
                  $"Toplam: {siradan + ender + destansi + efsanevi}");
    }
}
