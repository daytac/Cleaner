using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Jel çizgilerini yöneten merkezi manager sınıfı
/// LineRenderer pooling ile kalıcı jel çizgilerini oluşturur ve yönetir
/// </summary>
public class JelCizgiYoneticisi : MonoBehaviour
{
    [Header("Jel Ayarları")]
    [Tooltip("Jel çizgiler için kullanılacak material (JelMaterial)")]
    public Material jelMateryal;
    
    [Tooltip("Her LineRenderer'da maksimum nokta sayısı")]
    public int maksimumNoktaSayisi = 100;
    
    [Tooltip("Maksimum toplam jel çizgi sayısı (performans için)")]
    public int maksimumCizgiSayisi = 15;
    
    [Tooltip("İki nokta arasında minimum mesafe (performans için)")]
    public float minimumMesafe = 0.05f;
    
    [Tooltip("Jel çizginin genişliği")]
    public float jelGenislik = 0.15f;
    
    [Tooltip("Yüzeyden yukarı offset (yüzeye gömülmesin diye)")]
    public float yuzeyOffset = 0.01f;
    
    [Header("Debug")]
    public bool debugMesajlari = false;
    
    // İç değişkenler
    private List<LineRenderer> aktifCizgiler = new List<LineRenderer>();
    private LineRenderer aktifCizgi;
    private Vector3 sonNoktaPozisyonu;
    private int aktifCizgiNoktaSayisi = 0;
    
    /// <summary>
    /// Yeni bir jel çizgisi başlatır
    /// </summary>
    public void BaslangicNoktasiEkle(Vector3 pozisyon, Vector3 normal)
    {
        // Yeni LineRenderer oluştur
        YeniCizgiOlustur();
        
        // İlk noktayı ekle
        Vector3 offsetPozisyon = pozisyon + (normal * yuzeyOffset);
        aktifCizgi.positionCount = 1;
        aktifCizgi.SetPosition(0, offsetPozisyon);
        
        sonNoktaPozisyonu = offsetPozisyon;
        aktifCizgiNoktaSayisi = 1;
        
        if (debugMesajlari)
            Debug.Log($"Yeni jel çizgisi başlatıldı. Pozisyon: {offsetPozisyon}");
    }
    
    /// <summary>
    /// Aktif çizgiye devam noktası ekler
    /// </summary>
    public void DevamNoktasiEkle(Vector3 pozisyon, Vector3 normal)
    {
        if (aktifCizgi == null)
        {
            BaslangicNoktasiEkle(pozisyon, normal);
            return;
        }
        
        Vector3 offsetPozisyon = pozisyon + (normal * yuzeyOffset);
        
        // Minimum mesafe kontrolü
        if (Vector3.Distance(sonNoktaPozisyonu, offsetPozisyon) < minimumMesafe)
            return; // Çok yakın, nokta ekleme
        
        // Maksimum nokta kontrolü
        if (aktifCizgiNoktaSayisi >= maksimumNoktaSayisi)
        {
            // Maksimum çizgi sayısını da kontrol et
            if (aktifCizgiler.Count >= maksimumCizgiSayisi)
            {
                // Limit aşıldı - yeni çizgi başlatma
                if (debugMesajlari)
                    Debug.LogWarning($"Maksimum jel çizgi sayısına ulaşıldı: {maksimumCizgiSayisi}");
                return;
            }
            
            // Yeni çizgi başlat
            BaslangicNoktasiEkle(pozisyon, normal);
            return;
        }
        
        // Nokta ekle
        aktifCizgiNoktaSayisi++;
        aktifCizgi.positionCount = aktifCizgiNoktaSayisi;
        aktifCizgi.SetPosition(aktifCizgiNoktaSayisi - 1, offsetPozisyon);
        
        // ÖNEMLI: Collider'ı her nokta eklendiğinde güncelle
        // Böylece sünger jel çizimi sırasında da silebilir
        ColliderGuncelle(aktifCizgi);
        
        sonNoktaPozisyonu = offsetPozisyon;
    }
    
    /// <summary>
    /// Aktif çizimi bitirir
    /// </summary>
    public void CizimBitir()
    {
        // Collider'ı güncelle
        if (aktifCizgi != null)
        {
            ColliderGuncelle(aktifCizgi);
        }
        
        aktifCizgi = null;
        aktifCizgiNoktaSayisi = 0;
        
        if (debugMesajlari)
            Debug.Log($"Çizim bitirildi. Toplam aktif çizgi: {aktifCizgiler.Count}");
    }
    
    /// <summary>
    /// Tüm jel çizgilerini temizler
    /// </summary>
    public void TumCizgileriTemizle()
    {
        foreach (LineRenderer lr in aktifCizgiler)
        {
            if (lr != null)
                Destroy(lr.gameObject);
        }
        
        aktifCizgiler.Clear();
        aktifCizgi = null;
        aktifCizgiNoktaSayisi = 0;
        
        if (debugMesajlari)
            Debug.Log("Tüm jel çizgileri temizlendi.");
    }
    
    /// <summary>
    /// Belirli bir jel çizgisini siler (sünger etkileşimi için)
    /// </summary>
    public void CizgiSil(LineRenderer cizgi)
    {
        if (aktifCizgiler.Contains(cizgi))
        {
            aktifCizgiler.Remove(cizgi);
            Destroy(cizgi.gameObject);
            
            if (debugMesajlari)
                Debug.Log($"Jel çizgisi silindi. Kalan: {aktifCizgiler.Count}");
        }
    }
    
    /// <summary>
    /// Aktif jel çizgilerinin listesini döndürür
    /// </summary>
    public List<LineRenderer> AktifCizgileriAl()
    {
        // Null çizgileri temizle
        aktifCizgiler.RemoveAll(item => item == null);
        return aktifCizgiler;
    }
    
    /// <summary>
    /// Yeni bir LineRenderer GameObject oluşturur ve yapılandırır
    /// </summary>
    private void YeniCizgiOlustur()
    {
        GameObject yeniCizgiObj = new GameObject($"JelCizgi_{aktifCizgiler.Count}");
        yeniCizgiObj.transform.SetParent(transform);
        yeniCizgiObj.tag = "Jel"; // Tag ekle (sünger etkileşimi için)
        
        // ÇÖZÜM: Raycast'i engellemesi için "Ignore Raycast" layer'ına koy
        // Bu sayede jel çizgileri fareyle yeni çizgi çizerken engel olmaz
        yeniCizgiObj.layer = LayerMask.NameToLayer("Ignore Raycast");
        
        LineRenderer lr = yeniCizgiObj.AddComponent<LineRenderer>();
        
        // LineRenderer ayarları
        lr.material = jelMateryal;
        lr.startWidth = jelGenislik;
        lr.endWidth = jelGenislik * 0.8f; // Uçta biraz incelt
        lr.positionCount = 0;
        lr.useWorldSpace = true;
        lr.numCapVertices = 5;
        lr.numCornerVertices = 5;
        
        // Render ayarları
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        
        // BoxCollider ekle (trigger olarak)
        BoxCollider collider = yeniCizgiObj.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = new Vector3(jelGenislik, jelGenislik, 0.1f); // Başlangıç boyutu
        
        // Listeye ekle
        aktifCizgiler.Add(lr);
        aktifCizgi = lr;
        
        if (debugMesajlari)
            Debug.Log($"Yeni LineRenderer oluşturuldu: {yeniCizgiObj.name} (Tag: Jel)");
    }
    
    /// <summary>
    /// Çizgi tamamlandığında Collider'ı günceller
    /// Küçük segmentler için geniş Collider
    /// </summary>
    private void ColliderGuncelle(LineRenderer lr)
    {
        if (lr == null || lr.positionCount < 2) return;
        
        BoxCollider collider = lr.GetComponent<BoxCollider>();
        if (collider == null) return;
        
        // İlk ve son nokta
        Vector3 basla = lr.GetPosition(0);
        Vector3 son = lr.GetPosition(lr.positionCount - 1);
        
        // Merkez nokta
        Vector3 center = (basla + son) / 2f;
        
        // DAHA BÜYÜK COLLIDER - Küçük segmentler için
        float uzunluk = Vector3.Distance(basla, son);
        Vector3 size = new Vector3(
            jelGenislik * 8f,  // X genişlik (2x daha geniş)
            jelGenislik * 8f,  // Y genişlik (2x daha geniş)
            Mathf.Max(uzunluk + jelGenislik * 6f, jelGenislik * 8f) // Z uzunluk (daha uzun)
        );
        
        collider.center = center;
        collider.size = size;
        
        // Debug log kapatıldı - her nokta eklendiğinde çağrılıyor, çok spam olur
        // if (debugMesajlari)
        //     Debug.Log($"Collider güncellendi - Segment: {lr.name}, Size: {size}, Uzunluk: {uzunluk:F2}");
    }
}

