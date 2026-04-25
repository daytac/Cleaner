using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// SFX açma/kapama butonu (Toggle yerine Button)
/// </summary>
public class SFXButton : MonoBehaviour
{
    [Tooltip("SFX Button")]
    public Button sfxButton;

    [Tooltip("SFX kapalıyken gösterilecek sprite")]
    public Sprite sfxOffSprite;

    [Tooltip("SFX açıkken gösterilecek sprite")]
    public Sprite sfxOnSprite;

    [Tooltip("Butonun image component'i")]
    public Image buttonImage;

    void Start()
    {
        if (sfxButton == null)
        {
            sfxButton = GetComponent<Button>();
        }

        if (sfxButton != null)
        {
            // Başlangıç durumunu ayarla
            UpdateVisual(AudioManager.Instance.SFXEnabled);

            // Butona tıklandığında callback
            sfxButton.onClick.AddListener(OnButtonClick);
        }
        else
        {
            Debug.LogError("[SFXButton] Button component bulunamadı!");
        }
    }

    void OnButtonClick()
    {
        // Durumu ters çevir
        bool newState = !AudioManager.Instance.SFXEnabled;
        AudioManager.Instance.SFXEnabled = newState;
        UpdateVisual(newState);
    }

    void UpdateVisual(bool isOn)
    {
        // Sprite değiştir
        if (buttonImage != null && sfxOnSprite != null && sfxOffSprite != null)
        {
            buttonImage.sprite = isOn ? sfxOnSprite : sfxOffSprite;
        }
    }

    void OnDestroy()
    {
        if (sfxButton != null)
        {
            sfxButton.onClick.RemoveListener(OnButtonClick);
        }
    }
}
