using UnityEngine;
using UnityEditor;

public class ParticleSystemFixer : EditorWindow
{
    [MenuItem("Tools/Fix Particle System Ayarları")]
    static void FixParticleSystem()
    {
        // Particle System'i bul - inactive olsa da bulur
        ParticleSystem ps = null;
        foreach (var obj in Resources.FindObjectsOfTypeAll<ParticleSystem>())
        {
            if (obj.name == "Particle System" && obj.transform.parent != null && obj.transform.parent.name == "Cube")
            {
                ps = obj;
                break;
            }
        }

        if (ps == null)
        {
            Debug.LogError("Particle System bulunamadı!");
            return;
        }

        // Main Module
        var main = ps.main;
        main.duration = 5f;
        main.loop = true;
        main.playOnAwake = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.3f); // Daha geniş boyut aralığı
        
        // Rastgele kahverengi tonları - açık kahveden koyu kahveye
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.65f, 0.50f, 0.35f, 0.95f),  // Açık kahverengi
            new Color(0.30f, 0.20f, 0.10f, 0.65f)   // Koyu kahverengi
        );
        
        main.gravityModifier = 0.8f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 200;

        // Emission Module
        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 60f;

        // Shape Module
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.08f;

        // Velocity Over Lifetime Module
        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.World;
        vel.x = new ParticleSystem.MinMaxCurve(-0.8f, 0.8f);
        vel.y = new ParticleSystem.MinMaxCurve(-1.5f, 0.5f);
        vel.z = new ParticleSystem.MinMaxCurve(-0.8f, 0.8f);

        // Color Over Lifetime Module - Devre dışı (sabit renk)
        var col = ps.colorOverLifetime;
        col.enabled = false;

        // Size Over Lifetime Module - Devre dışı (sabit boyut)
        var size = ps.sizeOverLifetime;
        size.enabled = false;

        // Particle System Renderer - Render order ayarı
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            renderer.sortingOrder = 100; // Yüksek değer = en önde render edilir
            renderer.sortMode = ParticleSystemSortMode.Distance; // Kameraya uzaklığa göre sırala
        }

        Debug.Log("✓ Particle System ayarları düzeltildi!");
        
        // Sahneyi kirli olarak işaretle
        EditorUtility.SetDirty(ps.gameObject);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(ps.gameObject.scene);
    }
}
