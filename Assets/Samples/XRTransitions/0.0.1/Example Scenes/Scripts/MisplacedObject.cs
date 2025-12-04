using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class MisplacedObject : MonoBehaviour
{
    [Tooltip("The particle effect to play when found.")]
    public GameObject successEffect;
    
    private UnityEngine.XR.Interaction.Toolkit.XRSimpleInteractable interactable;

    private void Awake()
    {
        // Get or Add the XRSimpleInteractable component
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.XRSimpleInteractable>();
        if (interactable == null) interactable = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.XRSimpleInteractable>();
    }

    private void OnEnable()
    {
        interactable.selectEntered.AddListener(OnFound);
    }

    private void OnDisable()
    {
        interactable.selectEntered.RemoveListener(OnFound);
    }

    private void OnFound(SelectEnterEventArgs args)
    {
        Debug.Log($"Found misplaced object: {gameObject.name}!");

        // 1. Visual Feedback (Optional)
        if (successEffect != null)
        {
            Instantiate(successEffect, transform.position, Quaternion.identity);
        }

        // 2. Game Logic (Add to score, etc.)
        // GameManager.Instance.AddScore(1); // (If you implement a score manager)

        // 3. Remove object
        Destroy(gameObject);
    }
}