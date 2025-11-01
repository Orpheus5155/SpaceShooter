using UnityEngine;
using UnityEngine.UI;

public class StarCounter : MonoBehaviour
{
    [Header("Stars UI")]
    [SerializeField] private Text starsText;
    
    private int starsCollected = 0;
    
    private static StarCounter instance;
    
    public static StarCounter Instance
    {
        get { return instance; }
    }
    
    private void Awake()
    {
        // Singleton pattern to ensure only one StarCounter exists
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
    }
    
    private void Start()
    {
        UpdateStarsDisplay();
    }
    
    public void AddStars(int count)
    {
        starsCollected += count;
        Debug.Log($"Stars collected: +{count}. Total stars: {starsCollected}");
        UpdateStarsDisplay();
    }
    
    public void ResetStars()
    {
        starsCollected = 0;
        UpdateStarsDisplay();
    }
    
    public int GetStars()
    {
        return starsCollected;
    }
    
    private void UpdateStarsDisplay()
    {
        if (starsText != null)
        {
            starsText.text = starsCollected.ToString("D3");
        }
        else
        {
            Debug.LogWarning("Stars Text is null! Please assign the Stars Text UI element in the Inspector.");
        }
    }
}
