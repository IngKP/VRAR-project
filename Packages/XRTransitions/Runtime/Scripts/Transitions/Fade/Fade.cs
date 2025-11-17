using System.Threading.Tasks;
using Scripts;
using Scripts.Transitions.Fade;
using UnityEngine;

public class Fade : MonoBehaviour
{
    [SerializeField] private Renderer _planeRenderer;
    private TransitionManager _transitionManager;

    private void Awake()
    {
        if (_planeRenderer == null) _planeRenderer = GetComponentInChildren<Renderer>();
        _transitionManager = FindObjectOfType<TransitionManager>();
    }

    public void Initialize(FadeTransition transition)
    {
        // 1. Stick the Black Quad to the Main Camera
        transform.parent = _transitionManager.MainCamera.transform;
        transform.localPosition = new Vector3(0f, 0f, _transitionManager.MainCamera.nearClipPlane + 0.1f);
        transform.localRotation = Quaternion.identity;
        
        // 2. Ensure it starts completely transparent
        SetAlpha(0f);
    }

    public async Task FadeOut(float duration)
    {
        float startTime = Time.time;
        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            SetAlpha(t); // Fade from 0 (Clear) to 1 (Black)
            await Task.Yield();
        }
        SetAlpha(1f);
    }

    public async Task FadeIn(float duration)
    {
        float startTime = Time.time;
        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            SetAlpha(1f - t); // Fade from 1 (Black) to 0 (Clear)
            await Task.Yield();
        }
        SetAlpha(0f);
    }

    private void SetAlpha(float alpha)
    {
        if (_planeRenderer != null && _planeRenderer.material != null)
        {
            Color c = _planeRenderer.material.color;
            c.a = alpha;
            _planeRenderer.material.color = c;
        }
    }
}