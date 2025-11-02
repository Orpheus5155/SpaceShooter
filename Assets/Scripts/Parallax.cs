using UnityEngine;

public class Parallax : MonoBehaviour
{
    Material mat;
    float distance;

    [Range(0f, 1.0f)]
    public float baseSpeed = 0.2f;
    
    [Tooltip("How much the difficulty affects speed (0 = no effect, 1 = full effect)")]
    [Range(0f, 2f)]
    public float difficultyMultiplier = 0.5f;
    
    private GameManager gameManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mat = GetComponent<Renderer>().material;
        gameManager = FindFirstObjectByType<GameManager>();
        
    }

    // Update is called once per frame
    void Update()
    {
        float currentSpeed = baseSpeed;
        
        // Adjust speed based on difficulty if GameManager exists
        if (gameManager != null)
        {
            // Add difficulty-based speed increase (difficultyMultiplier controls how much difficulty affects speed)
            float speedIncrease = gameManager.difficulty * difficultyMultiplier;
            currentSpeed = baseSpeed + speedIncrease;
        }
        
        distance += Time.deltaTime * currentSpeed;
        mat.SetTextureOffset("_MainTex", Vector2.right * distance);
    }
}
