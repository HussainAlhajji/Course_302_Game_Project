using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    [Header("Points")]
    public int points = 0;
    public int pointsForUltimate = 100;

    [Header("UI")]
    public TextMeshProUGUI pointsText;
    public TextMeshProUGUI ultimateReadyText;
    public TextMeshProUGUI pointsGainedText; // New text for showing points gained

    void Start()
    {
        UpdatePointsDisplay();
        CheckUltimateStatus();

        // Hide points gained text initially
        if (pointsGainedText != null)
        {
            pointsGainedText.gameObject.SetActive(false);
        }
    }

    public void AddPoints(int amount)
    {
        if (points >= pointsForUltimate)
            return;

        points += amount;

        if (points > pointsForUltimate)
            points = pointsForUltimate;

        UpdatePointsDisplay();
        CheckUltimateStatus();

        // Show points gained in the HUD
        ShowPointsGained(amount);
    }

    public void ResetPoints()
    {
        points = 0;
        UpdatePointsDisplay();

        // Hide the "Press Q" text
        if (ultimateReadyText != null)
        {
            ultimateReadyText.gameObject.SetActive(false);
        }
    }

    void UpdatePointsDisplay()
    {
        // Update the points text to show "current points / points required for ultimate"
        if (pointsText != null)
        {
            pointsText.text = $"{points} / {pointsForUltimate}";
        }
    }

    void CheckUltimateStatus()
    {
        // Show or hide the "Press Q" text based on whether the ultimate is ready
        if (ultimateReadyText != null)
        {
            ultimateReadyText.gameObject.SetActive(points >= pointsForUltimate);
        }
    }

    public bool CanUseUltimate()
    {
        return points >= pointsForUltimate;
    }

    private void ShowPointsGained(int amount)
    {
        if (pointsGainedText != null)
        {
            pointsGainedText.text = $"+{amount} Points!";
            pointsGainedText.gameObject.SetActive(true);

            // Hide the text after 2 seconds
            StartCoroutine(HidePointsGainedText());
        }
    }

    private IEnumerator HidePointsGainedText()
    {
        yield return new WaitForSeconds(2f);
        if (pointsGainedText != null)
        {
            pointsGainedText.gameObject.SetActive(false);
        }
    }

    public void HideUltimateReadyText()
    {
        if (ultimateReadyText != null)
        {
            ultimateReadyText.gameObject.SetActive(false);
        }
    }
}