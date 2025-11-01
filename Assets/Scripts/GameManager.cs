using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject smallEnemy;
    [SerializeField] private GameObject bigEnemy;
    [SerializeField] private GameObject asteroid;
    [SerializeField] private GameObject asteroidNormal;
    [SerializeField] private GameObject asteroidSmall;
    [SerializeField] private GameObject star;
    [SerializeField] private GameObject starBig;
    [SerializeField] private GameObject starBigger;
    [SerializeField] private GameObject enemySpawner;

    [Header("Wave Settings")]
    [Range(0f, 1f)] public float difficulty = 0.2f;
    [Tooltip("Time in seconds between enemy spawns")]
    public float spawnRate = 2f;
    public int wave = 0;
    public int enemiesAmount = 10;
    
    [Header("Asteroid Settings")]
    [Tooltip("Time in seconds between asteroid spawns")]
    public float asteroidSpawnRate = 1f;
    
    [Header("Star Settings")]
    [Tooltip("Time in seconds between star spawn attempts")]
    public float starSpawnRate = 0.8f;
    [Range(0f, 1f)]
    [Tooltip("Probability of spawning a star (0-1)")]
    public float starSpawnProbability = 0.6f;
    
    [Header("Enemy Speed Scaling")]
    public float baseEnemySpeed = 5f;
    public float speedIncreasePer2Wave = 0.5f;
    
    private int enemiesLeft;
    private bool waveComplete;
    private float currentEnemySpeed;

    private void Start()
    {
        // Start star spawning immediately (runs continuously)
        InvokeRepeating(nameof(SpawnStar), 0.5f, starSpawnRate);
        
        // Update wave counter at start
        UpdateWaveCounter();
        
        StartNextWave();
    }

    private void FixedUpdate()
    {
        if (enemiesLeft <= 0 && !waveComplete)
        {
            waveComplete = true;
            CompleteWave();
        }
    }

    private void Initialize()
    {
        InvokeRepeating(nameof(SpawnEnemy), 1f, spawnRate);
        InvokeRepeating(nameof(SpawnAsteroid), 0.5f, asteroidSpawnRate);
    }

    private void Deinitialize()
    {
        CancelInvoke(nameof(SpawnEnemy));
        CancelInvoke(nameof(SpawnAsteroid));
    }
    
    private void SpawnEnemy()
    {
        Vector3 spawnPos = GetRandomSpawnPosition();
        GameObject enemy = PickEnemy();
        
        GameObject spawnedEnemy = Instantiate(enemy, spawnPos, Quaternion.identity);
        
        // Apply speed boost based on current wave
        MoveRightLeft movement = spawnedEnemy.GetComponent<MoveRightLeft>();
        if (movement != null)
        {
            movement.moveSpeed = currentEnemySpeed;
        }
        
        // Randomly enable/disable SineMovement (50% chance)
        SineMovement sineMovement = spawnedEnemy.GetComponent<SineMovement>();
        if (sineMovement != null)
        {
            sineMovement.enabled = Random.value > 0.7f;
        }
        
        enemiesLeft--;
    }
    
    private void SpawnAsteroid()
    {
        GameObject asteroidPrefab = PickAsteroid();
        
        // Skip spawning if null is returned
        if (asteroidPrefab == null)
            return;
        
        Vector3 spawnPos = GetRandomSpawnPosition();
        GameObject spawnedAsteroid = Instantiate(asteroidPrefab, spawnPos, Quaternion.identity);
        
        // Apply speed boost based on current wave
        MoveRightLeft movement = spawnedAsteroid.GetComponent<MoveRightLeft>();
        if (movement != null)
        {
            movement.moveSpeed = currentEnemySpeed;
        }
    }
    
    private void SpawnStar()
    {
        // Check spawn probability
        if (Random.value > starSpawnProbability)
            return;
        
        // Choose between Star, StarBig, and StarBigger
        GameObject starPrefab = PickStar();
        
        Vector3 spawnPos = GetRandomSpawnPosition();
        GameObject spawnedStar = Instantiate(starPrefab, spawnPos, Quaternion.identity);
        
        // Apply speed boost based on current wave
        MoveRightLeft movement = spawnedStar.GetComponent<MoveRightLeft>();
        if (movement != null)
        {
            movement.moveSpeed = currentEnemySpeed;
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 basePos = enemySpawner.transform.position;
        float rand = Random.value;

        if (rand < 0.33f)
            return basePos;
        else if (rand < 0.66f)
            return basePos + new Vector3(0, 2.5f, 0);
        else
            return basePos + new Vector3(0, -2.5f, 0);
    }
    
    private GameObject PickStar()
    {
        float rand = Random.value;
        
        // 5% chance for StarBigger, 15% for StarBig, 80% for Star
        if (rand < 0.05f)
            return starBigger;
        else if (rand < 0.2f)
            return starBig;
        else
            return star;
    }    

    private GameObject PickEnemy()
    {
        return Random.value > difficulty ? smallEnemy : bigEnemy;
    }
    
    private GameObject PickAsteroid()
    {
        float rand = Random.value;
        
        // 30% chance for AsteroidSmall, 20% for AsteroidNormal, 10% for Asteroid, 40% skip spawn
        if (rand < 0.3f)
            return asteroidSmall;
        else if (rand < 0.5f)
            return asteroidNormal;
        else if (rand < 0.6f)
            return asteroid;
        else
            return null; // 40% chance to skip spawning
    }

    private void CompleteWave()
    {
        Deinitialize();
        wave++;
        enemiesAmount += 3;
        
        // Update wave counter
        UpdateWaveCounter();
        
        // Increase score multiplier by 0.10 each wave
        if (ScoreManager.Instance != null)
        {
            float newMultiplier = 1.0f + (wave * 0.1f);
            ScoreManager.Instance.SetMultiplier(newMultiplier);
        }
        
        // Increase difficulty each wave (more big enemies)
        if (difficulty < 1f)
        {
            difficulty += 0.02f;
            difficulty = Mathf.Min(difficulty, 1f); // Cap at 1.0
        }
        
        // Increase speed and spawn rate every 2 waves
        if (wave % 2 == 0)
        {
            if (currentEnemySpeed < 15f)
            {
                currentEnemySpeed += speedIncreasePer2Wave;
            }
            
            if (spawnRate > 0.5f)
            {
                spawnRate -= 0.1f;
            }
        }
        
        Invoke(nameof(StartNextWave), 5f);
    }

    private void StartNextWave()
    {
        enemiesLeft = enemiesAmount;
        waveComplete = false;
        
        // Set initial speed on first wave
        if (wave == 0)
        {
            currentEnemySpeed = baseEnemySpeed;
        }

        Initialize();
    }
    
    private void UpdateWaveCounter()
    {
        if (WaveCounter.Instance != null)
        {
            WaveCounter.Instance.UpdateWaveDisplay(wave);
        }
    }
}
