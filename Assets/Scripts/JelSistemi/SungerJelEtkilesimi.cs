using UnityEngine;

/// <summary>
/// Sünger ile jel çizgilerinin etkileşimini yönetir
/// OnTriggerEnter ile "Jel" tag'li objeleri siler
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class SungerJelEtkilesimi : MonoBehaviour
{
    [Header("Ayarlar")]
    [Tooltip("Köpük efekti (opsiyonel)")]
    public ParticleSystem kopukEfekti;
    
    [Header("Debug")]
    public bool debugMesajlari = false;
    
    private Rigidbody rb;
    private Collider col;
    
    void Start()
    {
        // Collider'ı trigger olarak ayarla
        col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        else
        {
            Debug.LogError("SungerJelEtkilesimi: Collider bulunamadı!");
        }
        
        // Rigidbody'yi kinematic yap (fizik simülasyonu kullanmıyoruz)
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;
        rb.useGravity = false;
        
        if (debugMesajlari)
            Debug.Log("SungerJelEtkilesimi hazır. Trigger ve Rigidbody aktif.");
    }
    
    /// <summary>
    /// Sünger jel çizgilerine değdiği sürece tetiklenir (her frame)
    /// </summary>
    void OnTriggerStay(Collider other)
    {
        // Debug logları KAPATILDI - her frame basıyor, performans düşürür!
        // if (debugMesajlari)
        //     Debug.Log($"[SÜNGER] OnTriggerStay çağrıldı! Obje: {other.gameObject.name}, Tag: {other.tag}");
        
        // Sadece "Jel" tag'li objeleri sil
        if (other.CompareTag("Jel"))
        {
            // Köpük efekti çal
            if (kopukEfekti != null && !kopukEfekti.isPlaying)
            {
                kopukEfekti.transform.position = other.transform.position;
                kopukEfekti.Play();
            }
            
            // Sadece ilk silme anında log (performans için)
            if (debugMesajlari)
                Debug.Log($"✅ Sünger jel çizgisini sildi: {other.gameObject.name}");
            
            // Jel çizgisini yok et
            Destroy(other.gameObject);
        }
    }
    
    /// <summary>
    /// Bu script'i aktif/deaktif et (sadece sünger aşamasında aktif olmalı)
    /// </summary>
    public void AktifEt(bool aktif)
    {
        enabled = aktif;
        
        if (col != null)
            col.enabled = aktif;
        
        if (debugMesajlari)
            Debug.Log($"SungerJelEtkilesimi {(aktif ? "aktif" : "deaktif")} edildi.");
    }
}
