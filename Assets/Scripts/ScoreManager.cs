using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    [Header("Score UI")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text multiplierText;
    
    [Header("Score Popup")]
    [SerializeField] private GameObject scorePopupPrefab;
    
    [Header("Status Text")]
    [SerializeField] private GameObject statusTextPrefab;
    
    private int score = 0;
    private float multiplier = 1.0f;
    
    private static ScoreManager instance;
    
    public static ScoreManager Instance
    {
        get { return instance; }
    }
    
    private void Awake()
    {
        // Singleton pattern to ensure only one ScoreManager exists
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
    }
    
    private void Start()
    {
        UpdateScoreDisplay();
        UpdateMultiplierDisplay();
    }
    
    public void AddScore(int points, Vector3 worldPosition)
    {
        int multipliedPoints = Mathf.RoundToInt(points * multiplier);
        score += multipliedPoints;
        UpdateScoreDisplay();
        
        // Spawn score popup
        ShowScorePopup(multipliedPoints, worldPosition);
    }
    
    private void ShowScorePopup(int score, Vector3 worldPosition)
    {   
        GameObject popup = Instantiate(scorePopupPrefab, transform);
        popup.name = "ScorePopup_" + score;
        
        ScorePopup popupScript = popup.GetComponent<ScorePopup>();
        popupScript.Initialize(score, worldPosition);
    }
    
    public void SetMultiplier(float newMultiplier)
    {
        multiplier = newMultiplier;
        UpdateMultiplierDisplay();
        
        // Hiển thị status tăng multiplier
        ShowStatusText($"Multiplier increased to {multiplier:F1}X!");
    }
    
    public float GetMultiplier()
    {
        return multiplier;
    }
    
    public void ResetScore()
    {
        score = 0;
        UpdateScoreDisplay();
    }
    
    public int GetScore()
    {
        return score;
    }
    
    private void UpdateScoreDisplay()
    {
        scoreText.text = score.ToString("D8");
    }
    
    private void UpdateMultiplierDisplay()
    {
         string displayText = multiplier.ToString("F1") + "X";
        multiplierText.text = displayText;
    }
    
    private void ShowStatusText(string message, float duration = 2f)
    {
        GameObject statusTextObj = Instantiate(statusTextPrefab, transform);
        statusTextObj.name = "StatusText_" + message.Substring(0, Mathf.Min(10, message.Length));
        
        StatusText statusTextScript = statusTextObj.GetComponent<StatusText>();
        statusTextScript.Initialize(message, duration);
    }
}
