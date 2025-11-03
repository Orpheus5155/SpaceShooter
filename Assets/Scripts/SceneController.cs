using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    [SerializeField] private SceneFade _sceneFade; // Optional explicit reference; falls back to child lookup
    [SerializeField] private float _sceneFadeDuration = 1f;
    [SerializeField] private bool _fadeOnStart = false; // Toggle per scene

    private void Awake()
    {
        if (_sceneFade == null)
        {
            // Find even if the overlay child is inactive
            _sceneFade = GetComponentInChildren<SceneFade>(true);
        }
    }

    private IEnumerator Start()
    {
        if (_fadeOnStart && _sceneFade != null)
        {
            yield return _sceneFade.FadeInCoroutine(_sceneFadeDuration);
        }
        // If not fading on start or no SceneFade assigned, do nothing
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        if (_sceneFade != null)
        {
            yield return _sceneFade.FadeOutCoroutine(_sceneFadeDuration);
        }
        yield return SceneManager.LoadSceneAsync(sceneName);
    }
}
