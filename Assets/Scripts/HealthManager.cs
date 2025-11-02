using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    [SerializeField] private int maxHealth = 15;
    private int currentHealth;
    
    [SerializeField] private Transform healthBarContainer;
    
    private Transform[] healthPoints;
    private bool lowHealthWarningShown = false;
    
    private static HealthManager instance;
    
    public static HealthManager Instance
    {
        get { return instance; }
    }
    
    private void Awake()
    {
        // Singleton pattern
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
    }
    
    private void Start()
    {
        currentHealth = maxHealth;
        GatherHealthPoints();
    }
    
    private void GatherHealthPoints()
    {
        int childCount = healthBarContainer.childCount;
        
        if (childCount == 0)
        {
            return;
        }
        
        // Store references to all child health points
        healthPoints = new Transform[childCount];
        
        for (int i = 0; i < childCount; i++)
        {
            healthPoints[i] = healthBarContainer.GetChild(i);
        }
        
        // Update maxHealth to match actual number of health points
        maxHealth = childCount;
        currentHealth = maxHealth;
    }
    
    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0)
            return;
        
        // Destroy health points from right to left (last to first)
        for (int i = 0; i < damage; i++)
        {
            if (currentHealth > 0)
            {
                int indexToDestroy = currentHealth - 1; // Get the rightmost active health point
                
                if (indexToDestroy >= 0 && indexToDestroy < healthPoints.Length && healthPoints[indexToDestroy] != null)
                {
                    Destroy(healthPoints[indexToDestroy].gameObject);
                    healthPoints[indexToDestroy] = null;
                }
                
                currentHealth--;
            }
        }
        
        currentHealth = Mathf.Max(currentHealth, 0); // Clamp to 0
        
        // Show LOW HEALTH warning when health reaches 5 or below (only once, permanent and blinking)
        if (currentHealth <= 5 && currentHealth > 0 && !lowHealthWarningShown)
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.ShowLowHealthWarning();
                lowHealthWarningShown = true;
                Debug.Log("LOW HEALTH warning displayed (permanent, blinking)");
            }
        }
        
        // Check for death
        if (currentHealth <= 0)
        {
            OnPlayerDeath();
        }
    }
    
    private void OnPlayerDeath()
    {
        Debug.Log("Player has died!");
        
        // Show status text
        if (ScoreManager.Instance != null)
        {
            // You can add game over logic here
        }
        
        // Disable player controls or trigger game over
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Optionally destroy player or disable controls
            // Destroy(player);
            player.SetActive(false);
        }
    }
    
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
    
    public int GetMaxHealth()
    {
        return maxHealth;
    }
}
