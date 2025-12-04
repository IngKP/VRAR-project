using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Scripts;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using Scripts.Transitions.Fade;
//test
public class TransitionManager : MonoBehaviour
{
    [SerializeReference] public List<Transition> Transitions;

    public XROrigin XROrigin => _xrOrigin;
    public Camera MainCamera => _mainCamera;
    public Vector3 CenterEyePosition => (_leftEyeTransform.position + _rightEyeTransform.position) / 2f; 
    public Transform LeftEyeTransform => _leftEyeTransform;
    public Transform RightEyeTransform => _rightEyeTransform;

    [field: SerializeField] public Context CurrentContext { set; get; }

    public bool IsTransitioning { get; private set; }
    public Transition CurrentTransition { get; private set; }

    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Transform _leftEyeTransform;
    [SerializeField] private Transform _rightEyeTransform;
    [SerializeField] private InputActionProperty _initiateAction;
    
    private XROrigin _xrOrigin;

    private void Awake()
    {
        _xrOrigin = FindObjectOfType<XROrigin>();

        Context.OnExit += context =>
        {
            if (CurrentContext == context) CurrentContext = null;
        };

        Context.OnEnter += context => { CurrentContext = context; };

        Transition.OnStartTransition += t =>
        {
            IsTransitioning = true;
            CurrentTransition = t;
        };

        Transition.OnEndTransition += t =>
        {
            IsTransitioning = false;
            if (t == CurrentTransition) CurrentTransition = null;
        };

        // NOTE: We removed the InputSystem.onAfterUpdate line because it was failing to fire.
    }

    // --- FIX 1: FORCE INITIALIZATION ---
    private async void Start()
    {
        // This forces the transitions to be ready immediately
        // await InitializeTransitionType(typeof(Scripts.Transitions.Cut.CutTransition));
        // Debug.Log("[TM] Auto-Initialized Cut Transitions! Ready for input.");
        await InitializeTransitionType(typeof(Scripts.Transitions.Fade.FadeTransition));
        Debug.Log("[TM] Auto-Initialized Fade Transitions! Ready for input.");
    }

    // --- FIX 2: USE STANDARD UPDATE LOOP ---
    private void Update()
    {
        // 1. CHECK FOR KEYBOARD INPUT (Force Trigger)
        // We check both 'New Input' and 'Legacy Input' to make sure Spacebar works 100%
        bool keyboardTriggered = false;

        // Check New Input System
        // if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.digit1Key.wasPressedThisFrame))
        if (Keyboard.current != null && (Keyboard.current.digit1Key.wasPressedThisFrame))
        {
            keyboardTriggered = true;
        }
        
        // Check Legacy Input (Backup)
        // if (UnityEngine.Input.GetKeyDown(KeyCode.Space) || UnityEngine.Input.GetKeyDown(KeyCode.Alpha1))
        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1))
        {
            keyboardTriggered = true;
        }

        // 2. EXECUTE TRANSITION IF TRIGGERED
        if (keyboardTriggered)
        {
            Debug.Log("[TM] Keyboard Input Detected in Update Loop!");
            TriggerTransitionLogic(true); // 'true' simulates right hand
        }
    }

    private async void TriggerTransitionLogic(bool isRight)
    {
        if (IsTransitioning) 
        {
            Debug.LogWarning("[TM] Already transitioning. Ignoring input.");
            return;
        }

        // Find the transition valid for the CURRENT room
        var transition = Transitions.FirstOrDefault(t => t.IsInitialized && t.GetStartContext() == CurrentContext);

        if (transition == null)
        {
            Debug.LogError($"[TM] Failed: No valid transition found starting from context '{CurrentContext?.name}'. Check your Inspector links!");
            return;
        }

        Debug.Log($"[TM] Starting Transition to: {transition.GetTargetContext()?.name ?? "Unknown"}");

        // --- FIX 3: SAFE CONTROLLER HAPTICS ---
        // Only try to vibrate if we actually found a VR controller, otherwise skip it to prevent crash
        try 
        {
             var controller = FindObjectsOfType<ActionBasedController>().FirstOrDefault(c => c.name.ToLower().Contains("right"));
             if (controller != null) controller.SendHapticImpulse(0.3f, 0.1f);
        }
        catch { /* Ignore haptic errors on keyboard */ }

        // Execute the logic
        Transition.OnActionPressed?.Invoke(transition, isRight);
        await transition.OnActionDown(isRight);
        
        // Simulate button release after a short delay (since we used keyboard tap)
        await Task.Delay(100); 
        await transition.OnActionUp();
        Transition.OnActionReleased?.Invoke(transition);
    }

    public List<Transition> GetActiveTransitions()
    {
        return Transitions.Where(transition => transition.IsInitialized).ToList();
    }

    public async Task InitializeTransitionType(Type type)
    {
        await Task.WhenAll(Transitions.Where(transition => transition.GetType() != type && transition.IsInitialized)
            .Select(transition => transition.Deinitialize()));
        await Task.WhenAll(Transitions.Where(transition => transition.GetType() == type)
            .Select(transition => transition.Initialize()));
    }

    public async Task DisableTransitions()
    {
        await Task.WhenAll(Transitions.Where(transition => transition.IsInitialized)
            .Select(transition => transition.Deinitialize()));
    }
}