using UnityEngine;

public class Bullet : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Vector2 direction = Vector2.right;
    public float speed = 2;
    public Vector2 velocity;

    public bool isEnemy = false;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {   
        velocity = direction * speed;
    }
    
    private void FixedUpdate() 
    {
        Vector2 pos = transform.position;

        pos += velocity * Time.fixedDeltaTime;

        transform.position = pos;
        
        // Destroy object nếu ra khỏi player view
        if (pos.x < -1 || pos.x > 18 || pos.y < -1 || pos.y > 11)
        {
            Destroy(gameObject);
        }
    }
}
