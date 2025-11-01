using UnityEngine;
using UnityEngine.UI;

public class WaveCounter : MonoBehaviour
{
    [SerializeField] private Text waveText;
    
    private static WaveCounter instance;
    
    public static WaveCounter Instance
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
        UpdateWaveDisplay(0);
    }
    
    public void UpdateWaveDisplay(int waveNumber)
    {
        if (waveText != null)
        {
            int displayWave = waveNumber + 1;
            waveText.text = $"WAVE - {displayWave}";
        }

    }
}
