using UnityEngine;

public class ExplosionAnimation : MonoBehaviour
{
    [SerializeField] private float destroyAfter = 0.5f; // Adjust based on animation length

    void ExplosionDone()
    {
        // Destroy the root parent GameObject (the prefab)
        Destroy(transform.root.gameObject);
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Destroy the root parent after animation completes
        Destroy(transform.root.gameObject, destroyAfter);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
