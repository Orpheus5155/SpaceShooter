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
        Bullet bullet = collision.GetComponent<Bullet>();
        if (bullet != null)
        {
            if (bullet.isEnemy)
            {
                Destroy(gameObject);
                Destroy(bullet.gameObject);
            }
        }

        // Check for collectible items (stars)
        Collectible collectible = collision.GetComponent<Collectible>();
        if (collectible != null)
        {
            // Collectible will handle score and destroy itself
            return;
        }

        Destructible destructible = collision.GetComponent<Destructible>();
        if (destructible != null)
        {
            Destroy(gameObject);
            Destroy(destructible.gameObject);
        }
    }
}
