using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource sfxSource;
    [SerializeField] AudioSource sfxLoop;

    public AudioClip background;
    public AudioClip gunshot;
    public AudioClip enemyDestroy;
    public AudioClip lowHealthWarning;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        musicSource.clip = background;
        musicSource.Play();
        sfxLoop.loop = true;
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    public void PlaySFXLoop(AudioClip clip)
    {
        sfxLoop.clip = clip;
        sfxLoop.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
