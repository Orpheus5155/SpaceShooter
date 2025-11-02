using UnityEngine;
using UnityEngine.InputSystem;

public class Ship : MonoBehaviour
{

    Gun[] guns;

    // thiết lập thông số
    [SerializeField] float moveSpeed = 15;
    int shootCooldownFrames = 0;
    bool moveUp;
    bool moveDown;
    bool moveLeft;
    bool moveRight;
    bool speedUp;
    bool shoot;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        guns = transform.GetComponentsInChildren<Gun>();
    }

    // Update is called once per frame   
    void Update()
    {
        moveUp = Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed;
        moveDown = Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed;
        moveLeft = Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed;
        moveRight = Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed;
        speedUp = Keyboard.current.shiftKey.isPressed;
        shoot = Keyboard.current.spaceKey.isPressed;
        
        // check đạn bắn mỗi 0.2s
        if (shootCooldownFrames > 0)
        {
            shootCooldownFrames--;
        }
        
        if (shoot && shootCooldownFrames == 0)
        {
            shootCooldownFrames = 18; // 12@60 frame = 0.2s
            foreach(Gun gun in guns)
            {
                gun.Shoot();
            }
        }
    }

    private void FixedUpdate()
    {
        Vector2 pos = transform.position;

        float moveAmount = moveSpeed * Time.fixedDeltaTime;
        if (speedUp)
        {
            moveAmount *= 1.5f;
        }
        Vector2 move = Vector2.zero;

        if (moveUp)
        {
            move.y += moveAmount;
        }
        if (moveDown)
        {
            move.y -= moveAmount;
        }
        if (moveLeft)
        {
            move.x -= moveAmount;
        }
        if (moveRight)
        {
            move.x += moveAmount;
        }

        // sửa tốc độ di chuyển chéo
        float moveMagnitude = Mathf.Sqrt(move.x * move.x + move.y * move.y);
        if (moveMagnitude > moveAmount)
        {
            float ratio = moveAmount / moveMagnitude;
            move *= ratio;
        }

        Debug.Log(move);
        Debug.Log(moveAmount);

        // boundary play field
        pos += move;
        if (pos.x <= 1.5f)
        {
            pos.x = 1.5f;
        }
        if (pos.x >= 16f)
        {
            pos.x = 16f;
        }
        if (pos.y <= 1)
        {
            pos.y = 1;
        }
        if (pos.y >= 9)
        {
            pos.y = 9;
        }

        transform.position = pos;
    }

    private void OnTriggerEnter2D(Collider2D collision) 
    {
        Debug.Log($"Ship collision detected with: {collision.gameObject.name}");
        
        // Check for collectible items (stars) - no damage
        Collectible collectible = collision.GetComponent<Collectible>();
        if (collectible != null)
        {
            Debug.Log("Collision is a Collectible - no damage");
            // Collectible will handle score and destroy itself
            return;
        }
        
        // Check for enemy bullets
        Bullet bullet = collision.GetComponent<Bullet>();
        if (bullet != null)
        {
            Debug.Log($"Collision is a Bullet - isEnemy: {bullet.isEnemy}");
            if (bullet.isEnemy)
            {
                // Enemy bullet deals 2 damage
                if (HealthManager.Instance != null)
                {
                    HealthManager.Instance.TakeDamage(2);
                }
                else
                {
                    Debug.LogError("HealthManager.Instance is NULL!");
                }
                Destroy(bullet.gameObject);
                return;
            }
        }

        // Check for destructible enemies/asteroids
        Destructible destructible = collision.GetComponent<Destructible>();
        if (destructible != null)
        {
            Debug.Log($"Collision is a Destructible: {destructible.gameObject.name}");
            
            // Check if this is an asteroid by name
            string objName = destructible.gameObject.name.ToLower();
            bool isAsteroid = objName.Contains("asteroid");
            Debug.Log($"Is asteroid: {isAsteroid}");
            
            int damage = GetCollisionDamage(destructible.gameObject);
            
            Debug.Log($"Calculated damage: {damage}");
            
            if (HealthManager.Instance != null)
            {
                HealthManager.Instance.TakeDamage(damage);
            }
            else
            {
                Debug.LogError("HealthManager.Instance is NULL!");
            }
            
            Destroy(destructible.gameObject);
        }
        else
        {
            Debug.LogWarning($"Collision object '{collision.gameObject.name}' has no Destructible component!");
        }
    }
    
    private int GetCollisionDamage(GameObject obj)
    {
        // Determine damage based on object name
        string objName = obj.name.ToLower();
        
        Debug.Log($"GetCollisionDamage called for: '{obj.name}' (lowercase: '{objName}')");
        
        // Check AsteroidSmall BEFORE generic "asteroid" to avoid false matches
        if (objName.Contains("asteroidsmall"))
        {
            Debug.Log("Identified as AsteroidSmall - 3 damage");
            return 3; // AsteroidSmall deals 3 damage
        }
        else if (objName.Contains("asteroidnormal"))
        {
            Debug.Log("Identified as AsteroidNormal - 5 damage");
            return 5; // AsteroidNormal deals 5 damage
        }
        else if (objName.Contains("asteroid"))
        {
            Debug.Log("Identified as Asteroid (large) - 7 damage");
            return 7; // Asteroid (large) deals 7 damage
        }
        else if (objName.Contains("enemytype1") || objName.Contains("smallenemy"))
        {
            Debug.Log("Identified as EnemyType1 - 4 damage");
            return 4; // EnemyType1 deals 4 damage
        }
        else if (objName.Contains("enemytype2") || objName.Contains("bigenemy"))
        {
            Debug.Log("Identified as EnemyType2 - 6 damage");
            return 6; // EnemyType2 deals 6 damage
        }
        
        // Default damage if type not recognized
        Debug.LogWarning($"Enemy/Asteroid type not recognized: '{objName}' - using default 3 damage");
        return 3;
    }
}
