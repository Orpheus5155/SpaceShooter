# SpaceShooter Codebase – Line‑by‑Line Walkthrough and How Pieces Fit Together

This document explains the logic in each script, line by line, and how the systems connect. Files are covered in dependency-friendly order, then tied together with a short architecture overview.

---

## AudioManager.cs

```csharp
using UnityEngine;                           // Use Unity engine API

public class AudioManager : MonoBehaviour     // Component to manage game audio
{
    [SerializeField] AudioSource musicSource; // AudioSource for background music
    [SerializeField] AudioSource sfxSource;   // AudioSource for one-shot sound effects
    [SerializeField] AudioSource sfxLoop;     // AudioSource for looping SFX (alarms, etc.)

    public AudioClip background;              // Music clip reference (assign in Inspector)
    public AudioClip gunshot;                 // Gunshot SFX clip
    public AudioClip enemyDestroy;            // Enemy destroyed SFX clip
    public AudioClip lowHealthWarning;        // Low-health alarm SFX clip

    void Start()                              // Called on first frame when enabled
    {
        musicSource.clip = background;        // Select the background track
        musicSource.Play();                   // Start playing the background music
        sfxLoop.loop = true;                  // Ensure the loop SFX AudioSource repeats
    }

    public void PlaySFX(AudioClip clip)       // Play a one-shot sound
    {
        sfxSource.PlayOneShot(clip);          // Non-blocking, mixes with other sounds
    }

    public void PlaySFXLoop(AudioClip clip)   // Start/replace a looping SFX
    {
        sfxLoop.clip = clip;                  // Assign clip to loop source
        sfxLoop.Play();                       // Start loop; keeps playing until stopped
    }

    void Update() { }                         // Unused per-frame hook (kept for future use)
}
```

---

## ExplosionAnimation.cs

```csharp
using UnityEngine;                                   // Unity API

public class ExplosionAnimation : MonoBehaviour       // Handles explosion cleanup
{
    [SerializeField] private float destroyAfter = 0.5f; // Fallback lifetime seconds

    void ExplosionDone()                              // Called by Animation Event at end
    {
        Destroy(transform.root.gameObject);           // Destroy the whole prefab (root)
    }
    
    void Start()                                      // On first frame
    {
        Destroy(transform.root.gameObject, destroyAfter); // Backup timed destroy
    }

    void Update() { }                                 // Unused
}
```

Why both? The animation event is the primary trigger; the timed destroy is a safety net in case the event isn’t configured.

---

## GameManager.cs

```csharp
using UnityEngine;                                  // Unity API

public class GameManager : MonoBehaviour            // Central game flow controller
{
    // Prefab references (assign in Inspector)
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject smallEnemy; // Small enemy prefab
    [SerializeField] private GameObject bigEnemy;   // Big enemy prefab
    [SerializeField] private GameObject asteroid;   // Large asteroid prefab
    [SerializeField] private GameObject asteroidNormal; // Medium asteroid prefab
    [SerializeField] private GameObject asteroidSmall;  // Small asteroid prefab
    [SerializeField] private GameObject star;       // Small star prefab
    [SerializeField] private GameObject starBig;    // Medium star prefab
    [SerializeField] private GameObject starBigger; // Large star prefab
    [SerializeField] private GameObject enemySpawner; // Spawn position marker

    // Wave and spawn pacing
    [Header("Wave Settings")]
    [Range(0f, 1f)] public float difficulty = 0.2f; // Big-enemy probability
    [Tooltip("Time in seconds between enemy spawns")]
    public float spawnRate = 2f;                    // Enemy spawn cadence
    public int wave = 0;                            // Current wave index (0-based)
    public int enemiesAmount = 10;                  // Enemies per wave
    
    [Header("Asteroid Settings")]
    [Tooltip("Time in seconds between asteroid spawns")]
    public float asteroidSpawnRate = 1f;            // Asteroid spawn cadence
    
    [Header("Star Settings")]
    [Tooltip("Time in seconds between star spawn attempts")]
    public float starSpawnRate = 0.8f;              // Star spawn check cadence
    [Range(0f, 1f)]
    [Tooltip("Probability of spawning a star (0-1)")]
    public float starSpawnProbability = 0.6f;       // 60% chance to spawn when checked
    
    // Movement scaling that respects each prefab’s base speed
    [Header("Enemy Speed Scaling")]
    [Tooltip("Speed multiplier that increases each wave")]
    public float speedMultiplierPer2Wave = 0.1f;    // +0.1x every 2 waves
    
    [Header("Multiplier Scaling")]
    [Tooltip("Multiplier increase per wave (e.g., 0.1 = +0.1X per wave)")]
    public float multiplierIncreasePerWave = 0.1f;  // Score multiplier growth
    
    private int enemiesLeft;                        // Countdown of spawns remaining
    private bool waveComplete;                      // Flag: wave finished
    private float currentSpeedMultiplier = 1.0f;    // Cumulative speed factor

    private void Start()                            // Startup hook
    {
        InvokeRepeating(nameof(SpawnStar), 0.5f, starSpawnRate); // Start star spawns
        UpdateWaveCounter();                         // Initialize UI
        StartNextWave();                             // Begin first wave
    }

    private void FixedUpdate()                       // Physics step
    {
        if (enemiesLeft <= 0 && !waveComplete)      // When all enemies for wave spawned
        {
            waveComplete = true;                     // Prevent double-complete
            CompleteWave();                          // Progress and scale
        }
    }

    private void Initialize()                        // Start wave spawners
    {
        InvokeRepeating(nameof(SpawnEnemy), 1f, spawnRate);       // Enemies
        InvokeRepeating(nameof(SpawnAsteroid), 0.5f, asteroidSpawnRate); // Asteroids
    }

    private void Deinitialize()                      // Stop spawners
    {
        CancelInvoke(nameof(SpawnEnemy));            // Stop enemies
        CancelInvoke(nameof(SpawnAsteroid));         // Stop asteroids
    }
    
    private void SpawnEnemy()                        // Spawn one enemy
    {
        Vector3 spawnPos = GetRandomSpawnPosition(); // Pick lane
        GameObject enemy = PickEnemy();              // Choose type by difficulty
        GameObject spawnedEnemy = Instantiate(enemy, spawnPos, Quaternion.identity);
        
        MoveRightLeft movement = spawnedEnemy.GetComponent<MoveRightLeft>();
        if (movement != null)
        {
            movement.moveSpeed *= currentSpeedMultiplier; // Multiply, don’t override
        }
        
        SineMovement sineMovement = spawnedEnemy.GetComponent<SineMovement>();
        if (sineMovement != null)
        {
            sineMovement.enabled = Random.value > 0.7f;   // ~30% get sine movement
        }
        
        enemiesLeft--;                                    // Reduce remaining count
    }
    
    private void SpawnAsteroid()                   // Spawn one asteroid (maybe)
    {
        GameObject asteroidPrefab = PickAsteroid();
        if (asteroidPrefab == null) return;             // 40% skip chance
        
        Vector3 spawnPos = GetRandomSpawnPosition();
        GameObject spawnedAsteroid = Instantiate(asteroidPrefab, spawnPos, Quaternion.identity);
        
        MoveRightLeft movement = spawnedAsteroid.GetComponent<MoveRightLeft>();
        if (movement != null)
        {
            movement.moveSpeed *= currentSpeedMultiplier; // Apply multiplier
        }
    }
    
    private void SpawnStar()                       // Spawn collectible star (probabilistic)
    {
        if (Random.value > starSpawnProbability) return; // Fail the roll → skip
        
        GameObject starPrefab = PickStar();              // Choose star size by rarity
        Vector3 spawnPos = GetRandomSpawnPosition();
        GameObject spawnedStar = Instantiate(starPrefab, spawnPos, Quaternion.identity);
        
        MoveRightLeft movement = spawnedStar.GetComponent<MoveRightLeft>();
        if (movement != null)
        {
            movement.moveSpeed *= currentSpeedMultiplier; // Apply multiplier
        }
    }

    private Vector3 GetRandomSpawnPosition()       // Five lanes, even 20% each
    {
        Vector3 basePos = enemySpawner.transform.position; // Right-edge base point
        float rand = Random.value;                         // 0..1 roll

        if (rand < 0.2f) return basePos + new Vector3(0,  3.0f, 0); // Top
        else if (rand < 0.4f) return basePos + new Vector3(0,  1.5f, 0); // Top-mid
        else if (rand < 0.6f) return basePos;                                // Mid
        else if (rand < 0.8f) return basePos + new Vector3(0, -1.5f, 0); // Bot-mid
        else return basePos + new Vector3(0, -3.0f, 0);                   // Bottom
    }
    
    private GameObject PickStar()                  // Rarity table for stars
    {
        float rand = Random.value;                 // 0..1
        if (rand < 0.05f) return starBigger;       // 5%
        else if (rand < 0.2f) return starBig;      // next 15%
        else return star;                          // remaining 80%
    }    

    private GameObject PickEnemy()                 // Enemy choice by difficulty
    {
        return Random.value > difficulty ? smallEnemy : bigEnemy; // e.g., 0.2 → 80/20
    }
    
    private GameObject PickAsteroid()              // Asteroid weighted choice
    {
        float rand = Random.value;                 // 0..1
        if (rand < 0.3f) return asteroidSmall;     // 30%
        else if (rand < 0.5f) return asteroidNormal; // next 20%
        else if (rand < 0.6f) return asteroid;     // next 10%
        else return null;                          // remaining 40% → skip
    }

    private void CompleteWave()                    // Wrap up and scale
    {
        Deinitialize();                            // Stop spawns
        wave++;                                    // Next wave index
        enemiesAmount += 3;                        // Harder: more enemies
        
        UpdateWaveCounter();                       // Update UI
        
        if (ScoreManager.Instance != null)         // Scale score multiplier
        {
            float newMultiplier = 1.0f + (wave * multiplierIncreasePerWave);
            ScoreManager.Instance.SetMultiplier(newMultiplier);
        }
        
        if (difficulty < 1f)                       // Increase big-enemy odds
        {
            difficulty = Mathf.Min(difficulty + 0.02f, 1f);
        }
        
        if (wave % 2 == 0)                         // Every two waves
        {
            currentSpeedMultiplier += speedMultiplierPer2Wave; // Faster movement
            if (spawnRate > 0.5f) spawnRate -= 0.1f;  // Spawn a bit faster
        }
        
        Invoke(nameof(StartNextWave), 5f);         // Delay before next wave
    }

    private void StartNextWave()                   // Begin a wave
    {
        enemiesLeft = enemiesAmount;               // Reset remaining enemies
        waveComplete = false;                      // Clear completion flag
        
        if (wave == 0) currentSpeedMultiplier = 1.0f; // Initial baseline

        Initialize();                              // Start spawners
    }
    
    private void UpdateWaveCounter()               // Sync UI with wave
    {
        if (WaveCounter.Instance != null)
        {
            WaveCounter.Instance.UpdateWaveDisplay(wave); // UI shows wave+1
        }
    }
}
```

---

## MoveRightLeft.cs

```csharp
using UnityEngine;                      // Unity API

public class MoveRightLeft : MonoBehaviour
{
    public float moveSpeed = 5;         // Units/second to the left

    void Start() { }                    // Unused start hook

    void Update() { }                   // Unused update hook
    
    private void FixedUpdate()          // Physics tick for smooth movement
    {
        Vector2 pos = transform.position;                 // Read position
        pos.x -= moveSpeed * Time.fixedDeltaTime;          // Move left by speed
        transform.position = pos;                          // Apply new position

        if (pos.x < -1 || pos.y < -1 || pos.y > 11)       // Off-screen bounds
        {
            Destroy(gameObject);                           // Cleanup
        }
    }
}
```

---

## Collectible.cs

```csharp
using UnityEngine;                                // Unity API

public class Collectible : MonoBehaviour          // Star collectible behaviour
{
    [SerializeField] private int scoreValue = 75; // Points awarded when collected
    [SerializeField] private int starCount = 1;   // Stars added to counter
    public GameObject starCollect;                // VFX on collect
    
    private void OnTriggerEnter2D(Collider2D collision)  // Triggered on overlap
    {
        Ship ship = collision.GetComponent<Ship>();      // Did player touch it?
        
        if (ship != null)                                // Only if player
        {
            ScoreManager.Instance.AddScore(scoreValue, transform.position); // Add points
            StarCounter.Instance.AddStars(starCount);     // Add stars
            Instantiate(starCollect, transform.position, Quaternion.identity); // VFX
            Destroy(gameObject);                          // Remove collectible
        }
    }
}
```

---

## Destructible.cs

```csharp
using UnityEngine;                                        // Unity API

public class Destructible : MonoBehaviour                 // Enemies/Asteroids
{
    AudioManager audioManager;                            // Cached audio ref

    public GameObject bulletImpact;                       // Impact VFX prefab

    [SerializeField] private int scoreValue = 100;        // Points on death
    [SerializeField] private int maxHealth = 1;           // Starting HP
    
    private int currentHealth;                            // Runtime HP

    private void Start()                                  // Initialize health
    {
        currentHealth = maxHealth;
    }
    
    private void Awake()                                  // Early component setup
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }
    
    private void OnTriggerEnter2D(Collider2D collision)   // Bullet impact handler
    {
        Bullet bullet = collision.GetComponent<Bullet>(); // Did a bullet hit?
        if (bullet != null)
        {
            if (!bullet.isEnemy) {                        // Ignore enemy-friendly hits
                Destroy(bullet.gameObject);               // Remove bullet
                TakeDamage(1);                            // Apply damage
                Instantiate(bulletImpact, transform.position, Quaternion.identity); // VFX
            }
        }
    }
    
    private void TakeDamage(int damage)                   // Reduce health
    {
        currentHealth -= damage;                          // Subtract HP
        
        if (currentHealth <= 0)                           // Dead?
        {
            audioManager.PlaySFX(audioManager.enemyDestroy); // Explosion SFX
            AwardScore();                                  // Give points
            Destroy(gameObject);                           // Remove entity
        }
    }
    
    private void AwardScore()                             // Score popup + tally
    {
        if (ScoreManager.Instance != null && scoreValue > 0)
        {
            ScoreManager.Instance.AddScore(scoreValue, transform.position);
        }
    }
}
```

---

## HealthManager.cs

```csharp
using UnityEngine;                                         // Unity API
using UnityEngine.UI;                                      // UI API

public class HealthManager : MonoBehaviour                 // Player HP system
{
    [SerializeField] private int maxHealth = 15;           // UI-driven max HP
    private int currentHealth;                             // Runtime HP

    [SerializeField] private Transform healthBarContainer; // Parent of HP icons

    AudioManager audioManager;                             // For warnings

    private Transform[] healthPoints;                      // Icon references
    private bool lowHealthWarningShown = false;            // One-time flag
    
    private static HealthManager instance;                 // Singleton instance
    
    public static HealthManager Instance                   // Global accessor
    {
        get { return instance; }
    }

    private void Awake()                                   // Early init
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
        if (instance != null && instance != this)          // Enforce singleton
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }
    
    private void Start()                                   // Setup from UI
    {
        currentHealth = maxHealth;
        GatherHealthPoints();                               // Read icons from UI
    }
    
    private void GatherHealthPoints()                      // Cache child icons
    {
        int childCount = healthBarContainer.childCount;    // Count icons
        if (childCount == 0) { return; }                   // No icons -> skip
        
        healthPoints = new Transform[childCount];          // Allocate array
        for (int i = 0; i < childCount; i++)               // Cache each child
        {
            healthPoints[i] = healthBarContainer.GetChild(i);
        }
        maxHealth = childCount;                             // Sync max to icons
        currentHealth = maxHealth;                          // Full health
    }
    
    public void TakeDamage(int damage)                     // Damage API
    {
        if (currentHealth <= 0) return;                    // Already dead
        
        for (int i = 0; i < damage; i++)                   // Remove icons
        {
            if (currentHealth > 0)
            {
                int indexToDestroy = currentHealth - 1;    // Rightmost active
                if (indexToDestroy >= 0 && indexToDestroy < healthPoints.Length && healthPoints[indexToDestroy] != null)
                {
                    Destroy(healthPoints[indexToDestroy].gameObject); // Remove icon
                    healthPoints[indexToDestroy] = null;               // Clear slot
                }
                currentHealth--;                            // Decrement HP
            }
        }
        
        currentHealth = Mathf.Max(currentHealth, 0);        // Clamp to 0
        
        if (currentHealth <= 5 && currentHealth > 0 && !lowHealthWarningShown) // Threshold
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.ShowLowHealthWarning();           // UI warn
                lowHealthWarningShown = true;                           // Only once
                audioManager.PlaySFXLoop(audioManager.lowHealthWarning); // Loop alarm
            }
        }
        
        if (currentHealth <= 0) OnPlayerDeath();            // Trigger death flow
    }
    
    private void OnPlayerDeath()                           // Handle player death
    {
        if (ScoreManager.Instance != null) { /* placeholder for status text */ }
        GameObject player = GameObject.FindGameObjectWithTag("Player"); // Find ship
        if (player != null) { player.SetActive(false); }   // Disable player
    }
    
    public int GetCurrentHealth() { return currentHealth; } // Accessor
    public int GetMaxHealth() { return maxHealth; }         // Accessor
}
```

---

## ScoreManager.cs

```csharp
using UnityEngine;                                     // Unity API
using UnityEngine.UI;                                  // UI API

public class ScoreManager : MonoBehaviour              // Score + UI + messages
{
    [Header("Score UI")]
    [SerializeField] private Text scoreText;           // Score display Text
    [SerializeField] private Text multiplierText;      // Multiplier display Text
    
    [Header("Score Popup")]
    [SerializeField] private GameObject scorePopupPrefab; // Floating "+N" prefab
    
    [Header("Status Text")]
    [SerializeField] private GameObject statusTextPrefab; // Generic message prefab
    [SerializeField] private GameObject lowHealthPrefab;  // Low-health message prefab
    
    private int score = 0;                            // Running total
    private float multiplier = 1.0f;                  // Current multiplier
    
    private static ScoreManager instance;             // Singleton
    public static ScoreManager Instance { get { return instance; } }
    
    private void Awake()                              // Enforce one instance
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);                      // Remove duplicates
            return;
        }
        instance = this;
    }
    
    private void Start()                              // Init UI
    {
        UpdateScoreDisplay();                         // 00000000
        UpdateMultiplierDisplay();                    // 1.0X
    }
    
    public void AddScore(int points, Vector3 worldPosition) // Add with popup
    {
        int multipliedPoints = Mathf.RoundToInt(points * multiplier); // Apply mult
        score += multipliedPoints;                     // Add
        UpdateScoreDisplay();                          // Refresh UI
        ShowScorePopup(multipliedPoints, worldPosition); // Floating UI at world pos
    }
    
    private void ShowScorePopup(int score, Vector3 worldPosition) // Spawn popup
    {   
        GameObject popup = Instantiate(scorePopupPrefab, transform); // UI child
        popup.name = "ScorePopup_" + score;          // Helpful for debugging
        ScorePopup popupScript = popup.GetComponent<ScorePopup>();
        popupScript.Initialize(score, worldPosition);  // Configure
    }
    
    public void SetMultiplier(float newMultiplier)    // External setter (GameManager)
    {
        multiplier = newMultiplier;                   // Store
        UpdateMultiplierDisplay();                    // Refresh UI
        ShowStatusText($"Multiplier increased to {multiplier:F1}X!"); // Notify
    }
    
    public float GetMultiplier() { return multiplier; } // Accessor
    
    public void ResetScore() { score = 0; UpdateScoreDisplay(); } // Reset
    public int GetScore() { return score; }             // Accessor
    
    private void UpdateScoreDisplay()                  // Format 8 digits
    {
        scoreText.text = score.ToString("D8");        // e.g., 00001234
    }
    
    private void UpdateMultiplierDisplay()             // Format 1 decimal + X
    {
        string displayText = multiplier.ToString("F1") + "X"; // e.g., 1.5X
        multiplierText.text = displayText;
    }
    
    public void ShowStatusText(string message, float duration = 2f, bool permanent = false, bool blink = false)
    {
        GameObject statusTextObj = Instantiate(statusTextPrefab, transform); // UI child
        statusTextObj.name = "StatusText_" + message.Substring(0, Mathf.Min(10, message.Length));
        StatusText statusTextScript = statusTextObj.GetComponent<StatusText>();
        statusTextScript.Initialize(message, duration, permanent, blink); // Configure
    }
    
    public void ShowLowHealthWarning()                 // Specialized warning
    {
        GameObject lowHealthObj = Instantiate(lowHealthPrefab, transform);
        lowHealthObj.name = "LowHealth_Warning";      // Constant name for lookup
        StatusText statusTextScript = lowHealthObj.GetComponent<StatusText>();
        if (statusTextScript != null)
        {
            statusTextScript.Initialize("LOW HEALTH!", 0f, permanent: true, blink: true);
        }
    }
}
```

---

## Ship.cs

Key points only (file is long; comments align to code structure you have):

- using statements: bring in UnityEngine and the new Input System.
- class Ship : MonoBehaviour: player controller component.
- Gun[] guns; caches all Gun components in children.
- Serialized moveSpeed, various bools for input state, shoot cooldown counter.
- AudioManager audioManager; cached in Awake via tag "Audio".
- Start(): guns = GetComponentsInChildren<Gun>() to find gun mounts.
- Update():
  - Reads keyboard state (WASD/Arrows, Shift, Space) each frame.
  - Handles shooting cooldown (decrement each frame; when zero and Space pressed: reset to 18 frames and call Shoot() on every Gun).
- FixedUpdate():
  - Computes movement amount per physics tick; increases if Shift held.
  - Builds movement vector from input flags.
  - Normalizes diagonal movement to avoid faster diagonals.
  - Clamps final position to a defined playfield rectangle and applies transform.position.
- OnTriggerEnter2D():
  - If colliding with Collectible: return (collectible handles itself).
  - If colliding with Bullet and bullet.isEnemy: TakeDamage(2) and destroy bullet.
  - If colliding with Destructible (enemy/asteroid): compute damage via GetCollisionDamage(name), apply damage to HealthManager, destroy the destructible.
- GetCollisionDamage(GameObject obj):
  - Lowercases name and returns damage based on substrings (asteroid sizes, enemy types) with a default fallback.

This script ties input → movement → shooting → damage handling.

---

## Bullet.cs

```csharp
using UnityEngine;                              // Unity API

public class Bullet : MonoBehaviour             // Generic projectile
{
    public Vector2 direction = Vector2.right;   // Heading (unit vector)
    public float speed = 2;                     // Units/second
    public Vector2 velocity;                    // Computed per Update

    public bool isEnemy = false;                // True if fired by enemies

    void Start() { }                            // Unused

    void Update()                               // Per-frame logic
    {   
        velocity = direction * speed;           // Compute velocity
    }
    
    private void FixedUpdate()                  // Physics tick
    {
        Vector2 pos = transform.position;       // Read position
        pos += velocity * Time.fixedDeltaTime;  // Integrate motion
        transform.position = pos;               // Apply
        
        if (pos.x < -1 || pos.x > 18 || pos.y < -1 || pos.y > 11) // Off-screen?
        {
            Destroy(gameObject);                // Cleanup
        }
    }
}
```

---

## Gun.cs

```csharp
using UnityEngine;                                   // Unity API

public class Gun : MonoBehaviour                     // Shooter component
{
    public Bullet bullet;                            // Bullet prefab reference
    Vector2 direction;                               // Aim direction (local)
    public bool autoShoot = false;                   // Enemy guns auto-fire
    public float shootIntervalSeconds = 0.5f;        // Cadence of auto-fire
    public float shootDelaySeconds = 0.0f;           // Initial delay before first shot
    float shootTimer = 0f;                           // Timer since last shot
    float delayTimer = 0f;                           // Timer since enable

    void Update()                                     // Per-frame logic
    {
        direction = (transform.localRotation * Vector2.right).normalized; // Aim
        if (autoShoot)
        {
            if (delayTimer >= shootDelaySeconds)      // Initial delay finished?
            {
                if (shootTimer >= shootIntervalSeconds) // Time to shoot?
                {
                    Shoot();                           // Fire
                    shootTimer = 0;                    // Reset cadence timer
                }
                else
                {
                    shootTimer += Time.deltaTime;      // Accumulate time
                }
            }
            else
            {
                delayTimer += Time.deltaTime;          // Wait initial delay
            }
        }
    }
    
    public void Shoot()                                // Spawn and initialize bullet
    {
        GameObject go = Instantiate(bullet.gameObject, this.transform.position, Quaternion.identity);
        Bullet goBullet = go.GetComponent<Bullet>();
        goBullet.direction = direction;                // Set heading
    }
}
```

---

## Parallax.cs

```csharp
using UnityEngine;                                      // Unity API

public class Parallax : MonoBehaviour                   // Scrolling background
{
    Material mat;                                       // Material to scroll
    float distance;                                     // Accumulated offset

    [Range(0f, 1.0f)]
    public float baseSpeed = 0.2f;                      // Baseline scroll speed
    
    [Tooltip("How much the difficulty affects speed (0 = no effect, 1 = full effect)")]
    [Range(0f, 2f)]
    public float difficultyMultiplier = 0.5f;           // Difficulty influence
    
    private GameManager gameManager;                    // For difficulty value

    void Start()                                        // Initialize
    {
        mat = GetComponent<Renderer>().material;        // Get material instance
        gameManager = FindFirstObjectByType<GameManager>(); // Cache GameManager
        
    }

    void Update()                                       // Scroll each frame
    {
        float currentSpeed = baseSpeed;                 // Start with base
        
        if (gameManager != null)                        // If GM exists
        {
            float speedIncrease = gameManager.difficulty * difficultyMultiplier; // Scale
            currentSpeed = baseSpeed + speedIncrease;    // Apply influence
        }
        
        distance += Time.deltaTime * currentSpeed;      // Integrate offset
        mat.SetTextureOffset("_MainTex", Vector2.right * distance); // Scroll texture
    }
}
```

---

## ScorePopup.cs

```csharp
using UnityEngine;                                         // Unity API
using UnityEngine.UI;                                      // UI API

public class ScorePopup : MonoBehaviour                    // Floating score text
{
    [SerializeField] private Text scoreText;               // UI Text
    [SerializeField] private float moveSpeed = 50f;        // Upward speed (pixels/s)
    [SerializeField] private float lifetime = 1.5f;        // Time before auto-destroy
    
    private float timer = 0f;                              // Elapsed time
    private Color originalColor;                           // For fade effect
    private RectTransform rectTransform;                   // UI transform
    private Camera mainCamera;                             // World→screen conversion
    private Vector3 worldPosition;                         // Anchor world pos
    
    private void Awake()                                   // Cache components
    {
        rectTransform = GetComponent<RectTransform>();
        mainCamera = Camera.main;
    }
    
    public void Initialize(int score, Vector3 worldPosition) // External setup
    {
        this.worldPosition = worldPosition;                // Anchor location
        scoreText.text = "+" + score.ToString();          // Format text
        originalColor = scoreText.color;                    // Remember color
    }
    
    private void Update()                                   // Animate/fade
    {
        if (mainCamera != null && rectTransform != null)    // Ensure refs
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPosition); // Convert
            screenPos.y += moveSpeed * timer;               // Rise over time
            rectTransform.position = screenPos;             // Apply screen pos
        }
        
        timer += Time.deltaTime;                            // Tick
        if (scoreText != null && timer > lifetime * 0.3f)   // Fade in last 70%
        {
            float alpha = Mathf.Lerp(1f, 0f, (timer - lifetime * 0.3f) / (lifetime * 0.7f));
            scoreText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
        }
        
        if (timer >= lifetime) Destroy(gameObject);         // Cleanup
    }
}
```

---

## WaveCounter.cs

```csharp
using UnityEngine;                           // Unity API
using UnityEngine.UI;                        // UI API

public class WaveCounter : MonoBehaviour     // Wave UI display (singleton)
{
    [SerializeField] private Text waveText;  // Text reference
    
    private static WaveCounter instance;     // Singleton instance
    public static WaveCounter Instance { get { return instance; } } // Accessor
    
    private void Awake()                     // Singleton enforcement
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
    }
    
    private void Start()                     // Initial UI
    {
        UpdateWaveDisplay(0);                // Show WAVE - 1
    }
    
    public void UpdateWaveDisplay(int waveNumber) // External setter
    {
        if (waveText != null)
        {
            int displayWave = waveNumber + 1;      // Convert 0-based to 1-based
            waveText.text = $"WAVE - {displayWave}"; // Update UI text
        }
    }
}
```

---

## StarCounter.cs

```csharp
using UnityEngine;                            // Unity API
using UnityEngine.UI;                         // UI API

public class StarCounter : MonoBehaviour      // Star tally UI (singleton)
{
    [Header("Stars UI")]
    [SerializeField] private Text starsText;  // Text reference
    
    private int starsCollected = 0;           // Internal count
    
    private static StarCounter instance;      // Singleton instance
    public static StarCounter Instance { get { return instance; } } // Accessor
    
    private void Awake()                      // Singleton enforcement
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
    }
    
    private void Start()                      // Initial UI
    {
        UpdateStarsDisplay();                 // Show 000
    }
    
    public void AddStars(int count)           // Increment API
    {
        starsCollected += count;              // Add
        UpdateStarsDisplay();                 // Refresh UI
    }
    
    public void ResetStars()                  // Reset API
    {
        starsCollected = 0;                   // Zero out
        UpdateStarsDisplay();                 // Refresh UI
    }
    
    public int GetStars() { return starsCollected; } // Accessor
    
    private void UpdateStarsDisplay()         // Format 3 digits
    {
        starsText.text = starsCollected.ToString("D3"); // e.g., 007
    }
}
```

---

## SineMovement.cs

```csharp
using UnityEngine;                             // Unity API

public class SineMovement : MonoBehaviour      // Vertical sine wave motion
{
    float sinCenterY;                          // Baseline Y (start position)
    public float amplitude = 2;                // Wave height
    public float frequency = 2;                // Wave frequency
    public bool inverted = false;              // Flip vertically

    void Start()                               // Cache baseline
    {
        sinCenterY = transform.position.y;     // Store start Y
    }

    void Update()                              // Recompute each frame
    {
        Vector2 pos = transform.position;      // Current position
        float sin = Mathf.Sin(pos.x * frequency) * amplitude; // Compute offset
        if (inverted) { sin *= -1; }           // Flip if required
        pos.y = sinCenterY + sin;              // Apply vertical offset
        transform.position = pos;              // Commit position
    }
}
```

---

# How it all fits together

- GameManager orchestrates gameplay: spawns Enemies, Asteroids, and Stars in lanes, controls waves, increases difficulty, and scales speeds by multiplying each spawned object’s MoveRightLeft.moveSpeed.
- Ship reads input, moves within bounds, fires via its child Gun components, and handles collisions with Bullets (enemy), Destructibles, and Collectibles.
- Gun spawns Bullet prefabs and sets their direction based on local rotation; enemy guns can auto-fire with delays and intervals.
- Bullet advances each physics tick and self-destroys when off-screen.
- Destructible takes damage from player bullets, awards score through ScoreManager, plays audio via AudioManager, and spawns impact VFX; ScorePopup renders floating “+N”.
- HealthManager manages player HP using UI icons, triggers low-health warnings (UI blinking + looping alarm), and disables the player on death.
- WaveCounter and StarCounter keep the UI in sync with current wave and star totals.
- Parallax scrolls background textures, subtly scaling speed with difficulty from GameManager.
- ExplosionAnimation cleans up explosion prefabs either when the animation event fires or after a timed fallback.
- AudioManager centralizes all sound playback.

This design keeps each responsibility small and composable, with singletons for global UI state and manager access, and clean component boundaries for movement, shooting, UI, and effects.
