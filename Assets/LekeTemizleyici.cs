using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class LekeTemizleyici : MonoBehaviour
{
    [Header("Ayarlar")]
    public RenderTexture lekeMaskesi;
    public Material objeMateryali;
    
    [Header("UI ve Efektler")]
    public ParticleSystem temizlemeEfekti;
    public Slider ilerlemeCubugu;
    
    [Tooltip("Temizleme sırasında çalacak ses (scrubbing sound)")]
    public AudioSource temizlemeSesi;
    
    [Header("Alet Konumlandırma")]
    [Tooltip("Aletin yüzeye göre offset değeri. X=yatay, Y=yukarı/aşağı, Z=ileri/geri")]
    public Vector3 aletKonumOffset = new Vector3(0, 0.1f, 0);

    [Tooltip("Aletin ekstra rotasyonu (Euler angles). İnce ayar için.")]
    public Vector3 aletRotasyonOffset = Vector3.zero;

    [Tooltip("Aletin boyut çarpanı.")]
    public float aletBoyutCarpani = 1f;

    [Header("Debug")]
    public bool debugModu = true;
    
    // Mevcut aşama bilgileri (EmojiData'dan gelir)
    private EmojiTemizlikAsamasi aktifAsama;
    private GameObject aktifAlet;
    
    // Fırça çizimi için materyal (otomatik oluşturulur)
    private Material fircaMateryali;

    private bool oyunBittiMi = false;
    private float zamanlayici = 0;
    
    // EVENT: Temizlik tamamlandığında dışarıya bildirim
    public System.Action OnTemizlikTamamlandi;
    private bool mouseBasiliMi = false;
    
    // Debug için son temas noktası
    private Vector3 sonHitNoktasi;

    void Start()
    {
        // Fırça materyali oluştur - birikimli çizim için
        fircaMateryali = new Material(Shader.Find("Unlit/Transparent"));
        
        // Particle effect'i başlangıçta durdur
        if (temizlemeEfekti != null)
        {
            temizlemeEfekti.Stop();
            temizlemeEfekti.gameObject.SetActive(false);
        }
        
        if (debugModu)
            Debug.Log("[LekeTemizleyici] Başlatıldı. RenderTexture: " + (lekeMaskesi != null ? lekeMaskesi.name : "NULL"));
    }

    void Update()
    {
        if (oyunBittiMi) return;

        // --- UI ÜZERİNDE Mİ KONTROLÜ ---
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            if (mouseBasiliMi)
            {
                AletiGizle();
                mouseBasiliMi = false;
            }
            return;
        }

        // --- GİRİŞ KONTROLÜ ---
        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit) && hit.transform == transform)
            {
                if (!mouseBasiliMi)
                {
                    if (aktifAlet != null)
                        aktifAlet.SetActive(true);
                    
                    // Particle system'i aktif et ve başlat
                    if (temizlemeEfekti != null)
                    {
                        temizlemeEfekti.gameObject.SetActive(true);
                        temizlemeEfekti.Clear(); // Eski partikülleri temizle
                        temizlemeEfekti.Play();
                    }
                    
                    mouseBasiliMi = true;
                }
                
                Boya(hit.textureCoord);
                AletiHareketEttir(hit.point, hit.normal);
                sonHitNoktasi = hit.point;
                
                // Her frame particle efektinin çalışır durumda olduğundan emin ol
                if (temizlemeEfekti != null)
                {
                    if (!temizlemeEfekti.isPlaying)
                    {
                        temizlemeEfekti.Play();
                    }
                    // Emission'ın aktif olduğundan emin ol
                    var emission = temizlemeEfekti.emission;
                    if (!emission.enabled)
                    {
                        emission.enabled = true;
                    }
                }
                    
                if (temizlemeSesi != null && !temizlemeSesi.isPlaying)
                {
                    // Random pitch ekle (0.9 - 1.1 arası)
                    temizlemeSesi.pitch = Random.Range(0.9f, 1.1f);
                    AudioManager.Instance.PlaySFX(temizlemeSesi);
                }
            }
        }
        else
        {
            if (mouseBasiliMi)
            {
                AletiGizle();
                mouseBasiliMi = false;
            }
        }

        // --- YÜZDE HESAPLAMA ---
        if (Input.GetMouseButton(0))
        {
            zamanlayici += Time.deltaTime;
            if (zamanlayici > 0.2f)
            {
                YuzdeyiHesapla();
                zamanlayici = 0;
            }
        }
    }

    /// <summary>
    /// EmojiKurtarmaYoneticisi tarafından çağrılan overload - Parametreleri paketler
    /// </summary>
    public void YeniAsamaBaslat(Texture2D kirliTex, Texture2D temizTex, GameObject alet, Texture2D firca, float boyut, float oran)
    {
        EmojiTemizlikAsamasi geciciAsama = new EmojiTemizlikAsamasi();
        geciciAsama.asamaAdi = "Otomatik Aşama";
        geciciAsama.kirliTexture = kirliTex;
        geciciAsama.temizTexture = temizTex;
        geciciAsama.aletModeli = alet;
        geciciAsama.fircaSekli = firca;
        geciciAsama.fircaBoyutu = boyut;
        geciciAsama.tamamlanmaOrani = oran;

        AsamayiBaslat(geciciAsama);
    }

    /// <summary>
    /// Yeni aşama başlat - EmojiKurtarmaYoneticisi'nden çağrılır
    /// </summary>
    public void AsamayiBaslat(EmojiTemizlikAsamasi asama)
    {
        aktifAsama = asama;
        oyunBittiMi = false;
        zamanlayici = 0;

        // Önceki aleti yok et (artık prefabdan instantiate ediyoruz)
        if (aktifAlet != null) 
        {
            Destroy(aktifAlet);
            aktifAlet = null;
        }

        // Maskeyi temizle (siyah = kirli)
        RenderTexture.active = lekeMaskesi;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = null;

        // Dokuları ayarla
        objeMateryali.SetTexture("_KirliTexture", asama.kirliTexture);
        objeMateryali.SetTexture("_TemizTexture", asama.temizTexture);

        // Aleti ayarla
        if (asama.aletModeli != null)
        {
            // Prefabı sahneye oluştur
            aktifAlet = Instantiate(asama.aletModeli, transform) as GameObject;
            aktifAlet.transform.localScale = Vector3.one * aletBoyutCarpani;
            aktifAlet.SetActive(false); // Başlangıçta gizli
        }

        // İlerleme çubuğunu sıfırla
        if (ilerlemeCubugu != null)
            ilerlemeCubugu.value = 0;

        if (debugModu)
        {
            string aletAdi = (asama.aletModeli != null) ? asama.aletModeli.name : "YOK";
            Debug.Log($"[LekeTemizleyici] Aşama başlatıldı: {asama.asamaAdi}, Alet: {aletAdi} (Instantiated)");
        }
    }

    void YuzdeyiHesapla()
    {
        if (aktifAsama == null) return;

        int kucukBoyut = 64;
        RenderTexture kucukRT = RenderTexture.GetTemporary(kucukBoyut, kucukBoyut);
        Graphics.Blit(lekeMaskesi, kucukRT);
        
        RenderTexture.active = kucukRT;
        Texture2D tex = new Texture2D(kucukBoyut, kucukBoyut);
        tex.ReadPixels(new Rect(0,0,kucukBoyut, kucukBoyut), 0, 0);
        tex.Apply();
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(kucukRT);

        Color[] pikseller = tex.GetPixels();
        int beyaz = 0;
        foreach(Color c in pikseller) 
            if(c.r > 0.5f) beyaz++;
        
        float oran = (float)beyaz / pikseller.Length;
        if(ilerlemeCubugu != null) 
            ilerlemeCubugu.value = oran;

        Destroy(tex);

        // Aşamaya özel tamamlanma oranı
        if(oran >= aktifAsama.tamamlanmaOrani) 
            AsamaTamamlandi();
    }

    void AsamaTamamlandi()
    {
        // Maskeyi beyaza dön (tamamen temiz)
        RenderTexture.active = lekeMaskesi;
        GL.Clear(true, true, Color.white);
        RenderTexture.active = null;

        oyunBittiMi = true;
        AletiGizle();
        
        // EVENT: EmojiKurtarmaYoneticisi'ne bildir
        OnTemizlikTamamlandi?.Invoke();
        
        if (debugModu)
            Debug.Log("[LekeTemizleyici] Aşama tamamlandı! Event tetiklendi.");
    }

    void Boya(Vector2 uv)
    {
        if (lekeMaskesi == null || aktifAsama == null) return;
        
        Texture2D firca = aktifAsama.fircaSekli;
        if (firca == null) return;
        
        float size = aktifAsama.fircaBoyutu;
        float x = uv.x * lekeMaskesi.width;
        float y = (1 - uv.y) * lekeMaskesi.height;
        
        // RenderTexture'a çiz
        RenderTexture oncekiRT = RenderTexture.active;
        RenderTexture.active = lekeMaskesi;
        
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, lekeMaskesi.width, lekeMaskesi.height, 0);
        
        Rect rect = new Rect(x - size/2, y - size/2, size, size);
        Graphics.DrawTexture(rect, firca, fircaMateryali);
        
        GL.PopMatrix();
        RenderTexture.active = oncekiRT;
    }

    void AletiHareketEttir(Vector3 pos, Vector3 normal)
    {
        if (aktifAlet == null || aktifAsama == null) return;
        
        // Rotasyonu ayarla
        Quaternion yuzeyRotasyonu = Quaternion.FromToRotation(Vector3.up, normal);
        aktifAlet.transform.rotation = yuzeyRotasyonu * Quaternion.Euler(aletRotasyonOffset);
        
        // ÇÖZÜM: Aletin küpün arkasında kalmaması için yüzey normaline göre offset ekle
        Vector3 yuzeyOffset = normal * 0.15f; // Yüzeyin 0.15 birim önünde
        
        // Offset'i rotasyona göre uygula (local space)
        Vector3 offsetDunya = aktifAlet.transform.TransformDirection(aletKonumOffset);
        aktifAlet.transform.position = pos + offsetDunya + yuzeyOffset;
        
        // Particle System pozisyonunu da güncelle
        if (temizlemeEfekti != null)
        {
            // Partikülleri yüzeyin önünde, temas noktasında göster
            Vector3 particleOffset = normal * 0.1f; // Yüzeyin biraz önünde
            temizlemeEfekti.transform.position = pos + particleOffset;
            
            // Partiküllerin yönünü yüzey normaline göre ayarla (yüzeyden dışarı)
            temizlemeEfekti.transform.rotation = Quaternion.LookRotation(normal);
        }
    }

    void AletiGizle()
    {
        if (aktifAlet != null) 
            aktifAlet.SetActive(false);
            
        if (temizlemeEfekti != null)
        {
            temizlemeEfekti.Stop();
            temizlemeEfekti.gameObject.SetActive(false);
        }
            
        if (temizlemeSesi != null)
            AudioManager.Instance.StopSFX(temizlemeSesi);
    }

    /// <summary>
    /// Oyunu sıfırla - EmojiKurtarmaYoneticisi'nden çağrılabilir
    /// </summary>
    public void Sifirla()
    {
        oyunBittiMi = false;
        zamanlayici = 0;
        mouseBasiliMi = false;
        
        if (aktifAlet != null)
            aktifAlet.SetActive(false);
            
        if (ilerlemeCubugu != null)
            ilerlemeCubugu.value = 0;

        if (debugModu)
            Debug.Log("[LekeTemizleyici] Sıfırlandı.");
    }
    
    void OnDrawGizmos()
    {
        if (!debugModu) return;
        
        // Raycast'in vurduğu nokta (Hedef)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(sonHitNoktasi, 0.05f);
        
        // Aletin konumu ile hedef arasındaki çizgi
        if (aktifAlet != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(sonHitNoktasi, aktifAlet.transform.position);
        }
    }
}