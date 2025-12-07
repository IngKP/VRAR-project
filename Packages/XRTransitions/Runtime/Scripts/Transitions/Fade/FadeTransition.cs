using System;
using System.Threading.Tasks;
using Scripts;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Scripts.Transitions.Fade
{
    public class FadeTransition : Transition
    {
        [SerializeField] private Context _startContext;
        [SerializeField] private GameObject _fadePrefab;
        [SerializeField] private float _duration = 1.0f; // Default 1 second

        private global::Fade _fade;

        internal override async Task OnInitialization()
        {
            await Task.CompletedTask;
        }

        internal override async Task OnDeinitialization()
        {
            if (_fade != null) Object.Destroy(_fade.gameObject);
            await Task.CompletedTask;
        }

        // internal override async Task OnTriggerTransition()
        // {
        //     if (_fadePrefab == null) return;

        //     // 1. Setup
        //     _fade = Object.Instantiate(_fadePrefab).GetComponent<global::Fade>();
        //     _fade.Initialize(this);

        //     // 2. Fade OUT (Screen goes Black)
        //     // We use half the duration for the fade out
        //     await _fade.FadeOut(_duration / 2f);

        //     // 3. TELEPORT (While screen is black)
        //     TransitionManager.XROrigin.transform.position = Destination.transform.position;
            
        //     // Optional: Slight pause at black to reduce motion sickness
        //     await Task.Delay(100);

        //     // 4. Fade IN (Screen goes Clear)
        //     await _fade.FadeIn(_duration / 2f);

        //     // 5. Cleanup
        //     Object.Destroy(_fade.gameObject);
        //     _fade = null;
        // }

        internal override async Task OnTriggerTransition()
        {
            if (_fadePrefab == null) return;

            // 1. Setup Fade
            _fade = Object.Instantiate(_fadePrefab).GetComponent<global::Fade>();
            _fade.Initialize(this);

            // 2. Fade OUT
            await _fade.FadeOut(_duration / 2f);

            // --- CRITICAL FIX START ---
            
            // Step A: Find the components that are breaking
            CharacterController charController = TransitionManager.XROrigin.GetComponent<CharacterController>();
            UnityEngine.XR.Interaction.Toolkit.ActionBasedContinuousMoveProvider moveProvider = 
                TransitionManager.XROrigin.GetComponent<UnityEngine.XR.Interaction.Toolkit.ActionBasedContinuousMoveProvider>();

            // Step B: Turn them OFF to stop the "Stuck" behavior and Physics locks
            if (charController != null) charController.enabled = false;
            if (moveProvider != null) moveProvider.enabled = false;

            // Step C: Teleport with Safety Height (+0.2m to avoid floor sticking)
            Vector3 safePosition = Destination.transform.position;
            safePosition.y += 0.2f; 
            TransitionManager.XROrigin.transform.position = safePosition;

            // Step D: Wait for Unity to register the move (Important!)
            await Task.Yield(); 

            // Step E: Turn them back ON (This "Reboots" the joystick connection)
            if (charController != null) charController.enabled = true;
            if (moveProvider != null) moveProvider.enabled = true;

            // --- CRITICAL FIX END ---

            // Optional: Pause slightly
            await Task.Delay(100);

            // 4. Fade IN
            await _fade.FadeIn(_duration / 2f);

            // 5. Cleanup
            Object.Destroy(_fade.gameObject);
            _fade = null;
        }

        internal override async Task OnActionDown(bool isRight)
        {
            await TriggerTransition();
        }

        internal override async Task OnActionUp()
        {
            await Task.CompletedTask;
        }

        public override Context GetStartContext()
        {
            return _startContext;
        }
    }
}