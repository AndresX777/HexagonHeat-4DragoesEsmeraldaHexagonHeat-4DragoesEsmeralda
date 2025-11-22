using UnityEngine;
using TMPro;

public class RoundTextAnimator : MonoBehaviour
{
    private TextMeshProUGUI text;
    private Vector3 originalScale;

    [Header("Animation Settings")]
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.1f;
    public bool doPulse = true;

    [Header("Color Settings")]
    public Color normalColor = Color.yellow;
    public Color glowColor = Color.white;
    public float colorSpeed = 3f;

    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        originalScale = transform.localScale;
        text.color = normalColor;
    }

    void Update()
    {
        if (doPulse)
        {
            // Efecto de pulso (crece y decrece)
            float pulse = 1 + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            transform.localScale = originalScale * pulse;
        }

        // Efecto de brillo en el color
        float t = (Mathf.Sin(Time.time * colorSpeed) + 1f) / 2f;
        text.color = Color.Lerp(normalColor, glowColor, t * 0.3f);
    }

    // Llamar cuando cambie la ronda para efecto especial
    public void PlayNewRoundEffect()
    {
        StartCoroutine(ScalePopEffect());
    }

    private System.Collections.IEnumerator ScalePopEffect()
    {
        // Crece rápido
        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float scale = 1f + (0.5f * (1f - elapsed / duration));
            transform.localScale = originalScale * scale;
            yield return null;
        }

        transform.localScale = originalScale;
    }
}
