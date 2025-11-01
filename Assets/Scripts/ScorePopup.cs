using UnityEngine;
using UnityEngine.UI;

public class ScorePopup : MonoBehaviour
{
    [SerializeField] private Text scoreText;
    [SerializeField] private float moveSpeed = 50f;
    [SerializeField] private float lifetime = 1.5f;
    
    private float timer = 0f;
    private Color originalColor;
    private RectTransform rectTransform;
    private Camera mainCamera;
    private Vector3 worldPosition;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        mainCamera = Camera.main;
    }
    
    public void Initialize(int score, Vector3 worldPosition)
    {
        this.worldPosition = worldPosition;
        
        Debug.Log($"ScorePopup Initialize called: score={score}, worldPos={worldPosition}");
        Debug.Log($"ScoreText component: {(scoreText != null ? "FOUND" : "NULL")}");
        Debug.Log($"Main Camera: {(mainCamera != null ? "FOUND" : "NULL")}");
        Debug.Log($"RectTransform: {(rectTransform != null ? "FOUND" : "NULL")}");
        
        // Set the score text
        if (scoreText == null)
        {
            Debug.LogError("ScoreText is null in ScorePopup! Did you assign it in the prefab?");
            return;
        }
        
        scoreText.text = "+" + score.ToString();
        originalColor = scoreText.color;
        Debug.Log($"ScorePopup initialized successfully: text='{scoreText.text}', color={originalColor}");
    }
    
    private void Update()
    {
        // Convert world position to screen position
        if (mainCamera != null && rectTransform != null)
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPosition);
            
            // Add upward movement based on timer
            screenPos.y += moveSpeed * timer;
            
            rectTransform.position = screenPos;
        }
        
        // Fade out over time
        timer += Time.deltaTime;
        if (scoreText != null && timer > lifetime * 0.3f)
        {
            float alpha = Mathf.Lerp(1f, 0f, (timer - lifetime * 0.3f) / (lifetime * 0.7f));
            scoreText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
        }
        
        // Destroy after lifetime
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
