using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// SFX açma/kapama toggle butonu
/// </summary>
public class SFXToggle : MonoBehaviour
{
    [Tooltip("SFX Toggle butonu")]
    public Toggle sfxToggle;

    [Tooltip("SFX kapalıyken gösterilecek sprite (opsiyonel)")]
    public Sprite sfxOffSprite;

    [Tooltip("SFX açıkken gösterilecek sprite (opsiyonel)")]
    public Sprite sfxOnSprite;

    [Tooltip("Toggle'ın background image'ı (sprite değiştirmek için - opsiyonel)")]
    public Image toggleImage;

    void Start()
    {
        if (sfxToggle == null)
        {
            sfxToggle = GetComponent<Toggle>();
        }

        if (sfxToggle != null)
        {
            // Başlangıç durumunu ayarla
            sfxToggle.isOn = AudioManager.Instance.SFXEnabled;
            UpdateVisual(AudioManager.Instance.SFXEnabled);

            // Toggle değiştiğinde callback
            sfxToggle.onValueChanged.AddListener(OnToggleChanged);
        }
        else
        {
            Debug.LogError("[SFXToggle] Toggle component bulunamadı!");
        }
    }

    void OnToggleChanged(bool isOn)
    {
        AudioManager.Instance.SFXEnabled = isOn;
        UpdateVisual(isOn);
    }

    void UpdateVisual(bool isOn)
    {
        // Opsiyonel: Sprite değiştir
        if (toggleImage != null && sfxOnSprite != null && sfxOffSprite != null)
        {
            toggleImage.sprite = isOn ? sfxOnSprite : sfxOffSprite;
        }
    }

    void OnDestroy()
    {
        if (sfxToggle != null)
        {
            sfxToggle.onValueChanged.RemoveListener(OnToggleChanged);
        }
    }
}
