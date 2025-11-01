using UnityEngine;

public class Collectible : MonoBehaviour
{
    [SerializeField] private int scoreValue = 75;
    [SerializeField] private int starCount = 1;
    
    private void OnTriggerEnter2D(Collider2D collision) 
    {
        // Check if collided with player
        Ship ship = collision.GetComponent<Ship>();
        
        if (ship != null)
        {
            Debug.Log($"Collectible {gameObject.name} collected! Awarding {scoreValue} points and {starCount} stars");
            
            // Award score for collecting item
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddScore(scoreValue, transform.position);
                Debug.Log($"Score added successfully. Current score: {ScoreManager.Instance.GetScore()}");
            }
            else
            {
                Debug.LogWarning("ScoreManager.Instance is null!");
            }
            
            // Add to star count
            if (StarCounter.Instance != null)
            {
                StarCounter.Instance.AddStars(starCount);
            }
            else
            {
                Debug.LogWarning("StarCounter.Instance is null!");
            }
            
            // Destroy only the collectible (this object)
            Destroy(gameObject);
        }
    }
}
