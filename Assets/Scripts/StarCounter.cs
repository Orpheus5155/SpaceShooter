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
        starsText.text = starsCollected.ToString("D3");
    }
}
