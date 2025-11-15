using UnityEngine;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance;

    public enum Language { Espanol, Ingles }
    public Language currentLanguage = Language.Espanol;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetLanguage(Language nuevoIdioma)
    {
        currentLanguage = nuevoIdioma;
        UpdateAllLocalizedSprites();
    }

    public void UpdateAllLocalizedSprites()
    {
     
        LocalizedSprite[] elementos = FindObjectsOfType<LocalizedSprite>();
        foreach (var e in elementos)
        {
            e.UpdateSprite();
        }
    }

    public void SetLanguageSpanish() => SetLanguage(Language.Espanol);
    public void SetLanguageEnglish() => SetLanguage(Language.Ingles);
}