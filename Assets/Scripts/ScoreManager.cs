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
        Debug.Log($"AddScore called: {points} x {multiplier:F2} = {multipliedPoints} points. Total score: {score}");
        UpdateScoreDisplay();
        
        // Spawn score popup
        ShowScorePopup(multipliedPoints, worldPosition);
    }
    
    private void ShowScorePopup(int score, Vector3 worldPosition)
    {
        Debug.Log($"ShowScorePopup called with score: {score} at position: {worldPosition}");
        
        if (scorePopupPrefab == null)
        {
            Debug.LogError("ScorePopup prefab is not assigned in ScoreManager! Please assign it in the Inspector.");
            return;
        }
        
        Debug.Log($"Instantiating popup prefab as child of {gameObject.name}");
        GameObject popup = Instantiate(scorePopupPrefab, transform);
        popup.name = "ScorePopup_" + score;
        
        Debug.Log($"Popup created: {popup.name}, active: {popup.activeSelf}");
        
        ScorePopup popupScript = popup.GetComponent<ScorePopup>();
        if (popupScript != null)
        {
            Debug.Log("ScorePopup script found, calling Initialize");
            popupScript.Initialize(score, worldPosition);
        }
        else
        {
            Debug.LogError("ScorePopup script not found on prefab! Make sure the prefab has the ScorePopup script component.");
        }
    }
    
    public void SetMultiplier(float newMultiplier)
    {
        multiplier = newMultiplier;
        Debug.Log($"Multiplier set to: {multiplier:F2}X");
        UpdateMultiplierDisplay();
        
        // Show status text for multiplier increase
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
        if (scoreText != null)
        {
            scoreText.text = score.ToString("D8");
        }
        else
        {
            Debug.LogError("ScoreText is null! Please assign the Score Text UI element in the Inspector.");
        }
    }
    
    private void UpdateMultiplierDisplay()
    {
        if (multiplierText != null)
        {
            string displayText = multiplier.ToString("F1") + "X";
            multiplierText.text = displayText;
            Debug.Log($"Multiplier UI updated to: {displayText}");
        }
        else
        {
            Debug.LogWarning("MultiplierText is null! Please assign the Multiplier Text UI element in the Inspector.");
        }
    }
    
    private void ShowStatusText(string message, float duration = 2f)
    {
        Debug.Log($"ShowStatusText called with message: {message}");
        
        if (statusTextPrefab == null)
        {
            Debug.LogError("StatusText prefab is not assigned in ScoreManager! Please assign it in the Inspector.");
            return;
        }
        
        Debug.Log($"Instantiating StatusText prefab as child of {gameObject.name}");
        GameObject statusTextObj = Instantiate(statusTextPrefab, transform);
        statusTextObj.name = "StatusText_" + message.Substring(0, Mathf.Min(10, message.Length));
        
        Debug.Log($"StatusText created: {statusTextObj.name}, active: {statusTextObj.activeSelf}");
        
        StatusText statusTextScript = statusTextObj.GetComponent<StatusText>();
        if (statusTextScript != null)
        {
            Debug.Log("StatusText script found, calling Initialize");
            statusTextScript.Initialize(message, duration);
        }
        else
        {
            Debug.LogError("StatusText script not found on prefab! Make sure the prefab has the StatusText script component.");
        }
    }
}
