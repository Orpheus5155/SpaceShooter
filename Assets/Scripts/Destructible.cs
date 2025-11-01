using UnityEngine;

public class Destructible : MonoBehaviour
{
    [SerializeField] private int scoreValue = 100;
    [SerializeField] private int maxHealth = 1;
    
    private int currentHealth;
    
    private void Start()
    {
        currentHealth = maxHealth;
        Debug.Log($"{gameObject.name} initialized with {currentHealth} HP");
    }
    
    private void OnTriggerEnter2D(Collider2D collision) 
    {
        // Handle bullet collision
        Bullet bullet = collision.GetComponent<Bullet>();
        if (bullet != null)
        {
            if (!bullet.isEnemy) {
                // Destroy the bullet
                Destroy(bullet.gameObject);
                
                // Take damage
                TakeDamage(1);
            }
        }
    }
    
    private void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}/{maxHealth}");
        
        // Destroy if health reaches zero
        if (currentHealth <= 0)
        {
            AwardScore();
            Destroy(gameObject);
        }
    }
    
    private void AwardScore()
    {
        if (ScoreManager.Instance != null && scoreValue > 0)
        {
            ScoreManager.Instance.AddScore(scoreValue, transform.position);
        }
    }
}
