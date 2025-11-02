using UnityEngine;
using UnityEngine.UI;

public class StatusText : MonoBehaviour
{
    [SerializeField] private Text textComponent;
    private float displayDuration = 2f;
    private float timer = 0f;
    private Color originalColor;
    private bool isInitialized = false;
    private bool isPermanent = false;
    private bool shouldBlink = false;
    private float blinkTimer = 0f;
    private float blinkInterval = 0.5f;

    private void Awake()
    {
        // Try to get Text component if not assigned
        if (textComponent == null)
        {
            textComponent = GetComponent<Text>();
        }
        

        originalColor = textComponent.color;

    }

    public void Initialize(string message, float duration = 2f, bool permanent = false, bool blink = false)
    {
        if (textComponent == null)
        {
            Destroy(gameObject);
            return;
        }
        
        textComponent.text = message;
        textComponent.color = originalColor;
        displayDuration = duration;
        timer = 0f;
        isPermanent = permanent;
        shouldBlink = blink;
        isInitialized = true;
    }

    private void Update()
    {
        // Don't update if not properly initialized
        if (!isInitialized || textComponent == null)
        {
            return;
        }
        
        // Handle blinking effect
        if (shouldBlink)
        {
            blinkTimer += Time.deltaTime;
            
            if (blinkTimer >= blinkInterval)
            {
                blinkTimer = 0f;
                
                // Toggle between red and white
                if (textComponent.color == Color.red)
                {
                    textComponent.color = Color.white;
                }
                else
                {
                    textComponent.color = Color.red;
                }
            }
        }
        
        // If permanent, don't fade or destroy
        if (isPermanent)
        {
            return;
        }
        
        timer += Time.deltaTime;

        // Start fading after half the display duration
        if (timer > displayDuration / 2f)
        {
            float fadeAmount = (timer - displayDuration / 2f) / (displayDuration / 2f);
            Color newColor = originalColor;
            newColor.a = Mathf.Lerp(1f, 0f, fadeAmount);
            textComponent.color = newColor;
        }

        // Destroy after full duration
        if (timer >= displayDuration)
        {
            Destroy(gameObject);
        }
    }
}
