using UnityEngine;
using UnityEditor;

public class ParticleSystemBaglayici : EditorWindow
{
    [MenuItem("Tools/Particle System Bağla")]
    static void BaglaParticleSystem()
    {
        // Cube'u bul
        GameObject cube = GameObject.Find("Cube");
        if (cube == null)
        {
            Debug.LogError("Cube bulunamadı!");
            return;
        }

        // LekeTemizleyici component'ini al
        LekeTemizleyici temizleyici = cube.GetComponent<LekeTemizleyici>();
        if (temizleyici == null)
        {
            Debug.LogError("Cube üzerinde LekeTemizleyici component'i bulunamadı!");
            return;
        }

        // Particle System'i bul
        Transform particleTransform = cube.transform.Find("Particle System");
        if (particleTransform == null)
        {
            Debug.LogError("Particle System child bulunamadı!");
            return;
        }

        ParticleSystem particleSystem = particleTransform.GetComponent<ParticleSystem>();
        if (particleSystem == null)
        {
            Debug.LogError("ParticleSystem component bulunamadı!");
            return;
        }

        // Bağla
        SerializedObject serializedObject = new SerializedObject(temizleyici);
        SerializedProperty prop = serializedObject.FindProperty("temizlemeEfekti");
        prop.objectReferenceValue = particleSystem;
        serializedObject.ApplyModifiedProperties();

        Debug.Log("Particle System başarıyla bağlandı!");
        
        // Sahneyi kirli olarak işaretle
        EditorUtility.SetDirty(cube);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(cube.scene);
    }
}
