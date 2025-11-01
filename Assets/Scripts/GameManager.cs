using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject smallEnemy;
    [SerializeField] private GameObject bigEnemy;
    [SerializeField] private GameObject enemySpawner;

    [Header("Wave Settings")]
    [Range(0f, 1f)] public float difficulty = 0.2f;
    [Tooltip("Time in seconds between enemy spawns")]
    public float spawnRate = 2f;
    public int wave = 0;
    public int enemiesAmount = 10;
    
    [Header("Enemy Speed Scaling")]
    public float baseEnemySpeed = 5f;
    public float speedIncreasePer5Wave = 0.5f;
    
    private int enemiesLeft;
    private bool waveComplete;
    private float currentEnemySpeed;

    private void Start()
    {
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
    }

    private void Deinitialize()
    {
        CancelInvoke(nameof(SpawnEnemy));
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

    private GameObject PickEnemy()
    {
        return Random.value > difficulty ? smallEnemy : bigEnemy;
    }

    private void CompleteWave()
    {
        Deinitialize();
        wave++;
        enemiesAmount += 3;
        
        // Increase difficulty each wave (more big enemies)
        if (difficulty < 1f)
        {
            difficulty += 0.02f;
            difficulty = Mathf.Min(difficulty, 1f); // Cap at 1.0
            Debug.Log($"Difficulty increased to: {difficulty:F2}");
        }
        
        // Increase speed and spawn rate every 5 waves
        if (wave % 5 == 0)
        {
            if (currentEnemySpeed < 15f)
            {
                currentEnemySpeed += speedIncreasePer5Wave;
                Debug.Log($"Enemy speed increased to: {currentEnemySpeed}");
            }
            
            if (spawnRate > 0.5f)
            {
                spawnRate -= 0.5f;
                Debug.Log($"Spawn rate decreased to: {spawnRate}s");
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
        
        Debug.Log($"Starting Wave {wave} - Enemies: {enemiesLeft} - Speed: {currentEnemySpeed}");
        Initialize();
    }
}
