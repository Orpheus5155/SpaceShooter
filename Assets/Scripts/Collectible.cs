using UnityEngine;

public class Collectible : MonoBehaviour
{
    [SerializeField] private int scoreValue = 75;
    [SerializeField] private int starCount = 1;
    public GameObject starCollect;
    
    private void OnTriggerEnter2D(Collider2D collision) 
    {
        // Check if collided with player
        Ship ship = collision.GetComponent<Ship>();
        
        if (ship != null)
        {
            ScoreManager.Instance.AddScore(scoreValue, transform.position);
            StarCounter.Instance.AddStars(starCount);
            Instantiate(starCollect, transform.position, Quaternion.identity);
            // Destroy only the collectible (this object)
            Destroy(gameObject);
        }
    }
}
