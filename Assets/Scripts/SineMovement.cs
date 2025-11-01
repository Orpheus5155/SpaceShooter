using UnityEngine;

public class SineMovement : MonoBehaviour
{

    float sinCenterY;
    public float amplitude = 2;
    public float frequency = 2;
    public bool inverted = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sinCenterY = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 pos = transform.position;

        float sin = Mathf.Sin(pos.x * frequency) * amplitude;

        if (inverted)
        {
            sin *= -1;
        }

        pos.y = sinCenterY + sin;

        transform.position = pos;
    }
}
