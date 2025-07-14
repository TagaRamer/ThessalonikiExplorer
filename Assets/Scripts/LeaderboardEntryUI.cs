// Component for individual leaderboard entry UI
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardEntryUI : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI dateText;
    public Image backgroundImage;

    [Header("Rank Colors")]
    public Color firstPlaceColor = Color.yellow;
    public Color secondPlaceColor = new Color(0.75f, 0.75f, 0.75f); // Silver
    public Color thirdPlaceColor = new Color(0.8f, 0.5f, 0.2f); // Bronze
    public Color defaultColor = Color.white;

    public void SetupEntry(int rank, string playerName, int score, string date)
    {
        if (rankText != null)
            rankText.text = $"#{rank}";

        if (nameText != null)
            nameText.text = playerName;

        if (scoreText != null)
            scoreText.text = score.ToString();

        if (dateText != null)
            dateText.text = date;

        // Set background color based on rank
        //if (backgroundImage != null)
        //{
        //    Color bgColor = defaultColor;
        //    switch (rank)
        //    {
        //        case 1:
        //            bgColor = firstPlaceColor;
        //            break;
        //        case 2:
        //            bgColor = secondPlaceColor;
        //            break;
        //        case 3:
        //            bgColor = thirdPlaceColor;
        //            break;
        //        default:
        //            bgColor = defaultColor;
        //            break;
        //    }

        //    bgColor.a = 0.3f; // Make it semi-transparent
        //    backgroundImage.color = bgColor;
        //}

        // Debug output to verify display
        Debug.Log($"Displaying entry: Rank {rank}, Name: {playerName}, Score: {score}, Date: {date}");
    }
}