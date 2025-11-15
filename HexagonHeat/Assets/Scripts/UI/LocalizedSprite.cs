using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class LocalizedSprite : MonoBehaviour
{
    [Header("Sprites por idioma")]
    public Sprite spanishSprite;
    public Sprite englishSprite;

    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    private void Start()
    {
        UpdateSprite();
    }

    public void UpdateSprite()
    {
        if (LanguageManager.Instance == null) return;

        if (LanguageManager.Instance.currentLanguage == LanguageManager.Language.Espanol)
            image.sprite = spanishSprite;
        else
            image.sprite = englishSprite;
    }
}