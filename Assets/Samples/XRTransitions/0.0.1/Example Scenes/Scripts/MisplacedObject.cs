using System; // Required for Action
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class MisplacedObject : MonoBehaviour
{
    [Tooltip("The particle effect to play when found.")]
    public GameObject successEffect;
    
    // Event to notify the manager that THIS specific object was found
    public static event Action<MisplacedObject> OnObjectFound;

    private UnityEngine.XR.Interaction.Toolkit.XRSimpleInteractable interactable;

    private void Awake()
    {
        // Get or Add the XRSimpleInteractable component
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.XRSimpleInteractable>();
        if (interactable == null) interactable = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.XRSimpleInteractable>();
    }

    private void OnEnable()
    {
        if (interactable != null)
            interactable.selectEntered.AddListener(OnFound);
    }

    private void OnDisable()
    {
        if (interactable != null)
            interactable.selectEntered.RemoveListener(OnFound);
    }

    private void OnFound(SelectEnterEventArgs args)
    {
        Debug.Log($"Found misplaced object: {gameObject.name}!");

        // 1. Notify the Manager (BEFORE destroying)
        OnObjectFound?.Invoke(this);

        // 2. Visual Feedback
        if (successEffect != null)
        {
            Instantiate(successEffect, transform.position, Quaternion.identity);
        }

        // 3. Remove object
        // We delay destruction slightly or just disable to ensure the event processes safely
        gameObject.SetActive(false); 
        Destroy(gameObject, 0.1f);
    }
}