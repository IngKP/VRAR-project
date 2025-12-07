// using System;
// using System.Threading.Tasks;
// using Scripts;
// using UnityEngine;
// using Object = UnityEngine.Object;

// namespace Scripts.Transitions.Fade
// {
//     public class FadeTransition : Transition
//     {
//         [SerializeField] private Context _startContext;
//         [SerializeField] private GameObject _fadePrefab;
//         [SerializeField] private float _duration = 1.0f; // Default 1 second

//         private global::Fade _fade;

//         internal override async Task OnInitialization()
//         {
//             await Task.CompletedTask;
//         }

//         internal override async Task OnDeinitialization()
//         {
//             if (_fade != null) Object.Destroy(_fade.gameObject);
//             await Task.CompletedTask;
//         }

//         // internal override async Task OnTriggerTransition()
//         // {
//         //     if (_fadePrefab == null) return;

//         //     // 1. Setup
//         //     _fade = Object.Instantiate(_fadePrefab).GetComponent<global::Fade>();
//         //     _fade.Initialize(this);

//         //     // 2. Fade OUT (Screen goes Black)
//         //     // We use half the duration for the fade out
//         //     await _fade.FadeOut(_duration / 2f);

//         //     // 3. TELEPORT (While screen is black)
//         //     TransitionManager.XROrigin.transform.position = Destination.transform.position;
            
//         //     // Optional: Slight pause at black to reduce motion sickness
//         //     await Task.Delay(100);

//         //     // 4. Fade IN (Screen goes Clear)
//         //     await _fade.FadeIn(_duration / 2f);

//         //     // 5. Cleanup
//         //     Object.Destroy(_fade.gameObject);
//         //     _fade = null;
//         // }

//         internal override async Task OnTriggerTransition()
//         {
//             if (_fadePrefab == null) return;

//             // 1. Setup Fade
//             _fade = Object.Instantiate(_fadePrefab).GetComponent<global::Fade>();
//             _fade.Initialize(this);

//             // 2. Fade OUT
//             await _fade.FadeOut(_duration / 2f);

//             // --- CRITICAL FIX START ---
            
//             var xrOrigin = TransitionManager.XROrigin;
//             var charController = xrOrigin.GetComponent<CharacterController>();
//             var driver = xrOrigin.GetComponent<UnityEngine.XR.Interaction.Toolkit.CharacterControllerDriver>();
//             var moveProvider = xrOrigin.GetComponent<UnityEngine.XR.Interaction.Toolkit.ActionBasedContinuousMoveProvider>();

//             // A. DISABLE EVERYTHING TO PREVENT CONFLICTS
//             if (moveProvider != null) moveProvider.enabled = false;
//             if (driver != null) driver.enabled = false;
//             if (charController != null) charController.enabled = false;

//             // B. CALCULATE HEAD-RELATIVE POSITION (Prevents spawning in walls)
//             // We want the CAMERA to land at the Destination, not the Rig Center.
//             Vector3 rigPos = xrOrigin.transform.position;
//             Vector3 camPos = TransitionManager.MainCamera.transform.position;
            
//             // Calculate the vector from Camera to Rig (ignoring height)
//             Vector3 playerOffset = rigPos - camPos;
//             playerOffset.y = 0; 

//             // The final position puts the Rig at a spot where the Camera will be at Destination
//             Vector3 targetPosition = Destination.transform.position + playerOffset;
            
//             // Ensure we don't snap to floor level if the destination is elevated, 
//             // but respect the Rig's original Y if the destination is just a marker.
//             // (Adjust 'y' based on your game's needs. Usually Destination.y is floor level).
//             targetPosition.y = Destination.transform.position.y;

//             // C. APPLY MOVE & SYNC PHYSICS
//             xrOrigin.transform.position = targetPosition;
            
//             // If you want to match rotation (facing direction):
//             // Rotate the Rig so the Camera faces the Destination's forward
//             // float rotationDiff = Destination.transform.eulerAngles.y - TransitionManager.MainCamera.transform.eulerAngles.y;
//             // xrOrigin.transform.Rotate(0, rotationDiff, 0);

//             Physics.SyncTransforms(); // Force Unity to accept the new position NOW

//             // D. WAIT FOR PHYSICS ENGINE CATCH-UP
//             // We wait 2 frames to ensure the physics collision world updates
//             await Task.Yield();
//             await Task.Yield();

//             // E. RE-ENABLE IN SAFE ORDER
//             if (driver != null) driver.enabled = true;       // 1. Driver updates internal height first
//             if (charController != null) charController.enabled = true; // 2. Controller wakes up in valid spot
//             if (moveProvider != null) moveProvider.enabled = true;     // 3. Inputs resume

//             // --- CRITICAL FIX END ---

//             // Optional: Pause slightly
//             await Task.Delay(100);

//             // 4. Fade IN
//             await _fade.FadeIn(_duration / 2f);

//             // 5. Cleanup
//             Object.Destroy(_fade.gameObject);
//             _fade = null;
//         }

        

//         internal override async Task OnActionDown(bool isRight)
//         {
//             await TriggerTransition();
//         }

//         internal override async Task OnActionUp()
//         {
//             await Task.CompletedTask;
//         }

//         public override Context GetStartContext()
//         {
//             return _startContext;
//         }
//     }
// }


using System;
using System.Threading.Tasks;
using Scripts;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem; 
using Object = UnityEngine.Object;

namespace Scripts.Transitions.Fade
{
    public class FadeTransition : Transition
    {
        [SerializeField] private Context _startContext;
        [SerializeField] private GameObject _fadePrefab;
        [SerializeField] private float _duration = 1.0f; 

        private global::Fade _fade;
        
        // Static reference ensures the action survives scene changes
        private static InputAction _universalMoveAction;

        internal override async Task OnInitialization() { await Task.CompletedTask; }
        internal override async Task OnDeinitialization() 
        {
            if (_fade != null) Object.Destroy(_fade.gameObject);
            await Task.CompletedTask;
        }

        internal override async Task OnTriggerTransition()
        {
            if (_fadePrefab == null) return;

            // 1. FADE OUT
            _fade = Object.Instantiate(_fadePrefab).GetComponent<global::Fade>();
            _fade.Initialize(this);
            await _fade.FadeOut(_duration / 2f);

            // 2. MOVE RIG
            var xrOrigin = TransitionManager.XROrigin;
            if (xrOrigin != null)
            {
                var moveProvider = Object.FindObjectOfType<ActionBasedContinuousMoveProvider>();
                var charController = xrOrigin.GetComponent<CharacterController>();
                var driver = xrOrigin.GetComponent<CharacterControllerDriver>();

                // A. Disable to prevent physics conflicts
                if (moveProvider != null) moveProvider.enabled = false;
                if (driver != null) driver.enabled = false;
                if (charController != null) charController.enabled = false;

                // B. Teleport (Head-Relative Logic)
                Vector3 rigPos = xrOrigin.transform.position;
                Vector3 camPos = xrOrigin.Camera.transform.position;
                Vector3 offset = rigPos - new Vector3(camPos.x, rigPos.y, camPos.z);
                
                Vector3 targetPos = Destination.position + offset;
                targetPos.y = Destination.position.y; // Exact floor level (Physics will handle the rest)

                xrOrigin.transform.position = targetPos;
                Physics.SyncTransforms();
                await Task.Yield();

                // C. Re-Enable Physics
                if (driver != null) driver.enabled = true;
                if (charController != null) charController.enabled = true;

                // 3. SETUP HYBRID INPUT (Headset + Editor Support)
                if (moveProvider != null)
                {
                    ConfigureHybridInput(moveProvider);
                }
            }

            // 4. FADE IN
            await Task.Delay(100);
            await _fade.FadeIn(_duration / 2f);
            Object.Destroy(_fade.gameObject);
            _fade = null;
        }

        private void ConfigureHybridInput(ActionBasedContinuousMoveProvider provider)
        {
            // Only create the action once to save performance
            if (_universalMoveAction == null)
            {
                // Create a generic Vector2 action
                _universalMoveAction = new InputAction("UniversalMove", type: InputActionType.Value);
                
                // 1. BINDING FOR VR HEADSET (The real thumbstick)
                _universalMoveAction.AddBinding("<XRController>{LeftHand}/thumbstick");

                // 2. BINDING FOR EDITOR (WASD Keys)
                _universalMoveAction.AddCompositeBinding("2DVector")
                    .With("Up", "<Keyboard>/w")
                    .With("Down", "<Keyboard>/s")
                    .With("Left", "<Keyboard>/a")
                    .With("Right", "<Keyboard>/d");
                
                // 3. BINDING FOR ARROW KEYS (Backup)
                _universalMoveAction.AddCompositeBinding("2DVector")
                    .With("Up", "<Keyboard>/upArrow")
                    .With("Down", "<Keyboard>/downArrow")
                    .With("Left", "<Keyboard>/leftArrow")
                    .With("Right", "<Keyboard>/rightArrow");
            }

            // Ensure it is active
            _universalMoveAction.Enable();

            // Assign it to the provider
            provider.leftHandMoveAction = new InputActionProperty(_universalMoveAction);
            
            // --- RESTORE SETTINGS ---
            // I removed the line "provider.moveSpeed = 4.0f;" 
            // It will now use whatever speed you set in the Inspector.
            
            provider.useGravity = true; 
            provider.enableFly = false; // Set to FALSE for walking, TRUE if you want to fly
            
            // Toggle to apply
            provider.enabled = false;
            provider.enabled = true;
        }

        internal override async Task OnActionDown(bool isRight) { await TriggerTransition(); }
        internal override async Task OnActionUp() { await Task.CompletedTask; }
        public override Context GetStartContext() { return _startContext; }
    }
}