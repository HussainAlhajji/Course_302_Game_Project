using UnityEngine;
using TMPro;

public class UltimateReadyTextAnimator : MonoBehaviour
{
    public TextMeshProUGUI ultimateText;
    public Color flashColor = Color.yellow;
    public float pulseSpeed = 2f;
    public float colorFlashSpeed = 5f;
    private Color originalColor;
    private Vector3 originalScale;

    void Start()
    {
        if (ultimateText == null)
            ultimateText = GetComponent<TextMeshProUGUI>();

        originalColor = ultimateText.color;
        originalScale = ultimateText.rectTransform.localScale;
    }

    void Update()
    {
        // Pulse size
        float scale = 1f + Mathf.Sin(Time.time * pulseSpeed) * 0.1f;
        ultimateText.rectTransform.localScale = originalScale * scale;

        // Flash between original and flashColor
        ultimateText.color = Color.Lerp(originalColor, flashColor, Mathf.PingPong(Time.time * colorFlashSpeed, 1));
    }
}
