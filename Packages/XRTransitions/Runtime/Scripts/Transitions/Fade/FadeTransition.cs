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

        internal override async Task OnTriggerTransition()
        {
            if (_fadePrefab == null) return;

            // 1. Setup
            _fade = Object.Instantiate(_fadePrefab).GetComponent<global::Fade>();
            _fade.Initialize(this);

            // 2. Fade OUT (Screen goes Black)
            // We use half the duration for the fade out
            await _fade.FadeOut(_duration / 2f);

            // 3. TELEPORT (While screen is black)
            TransitionManager.XROrigin.transform.position = Destination.transform.position;
            
            // Optional: Slight pause at black to reduce motion sickness
            await Task.Delay(100);

            // 4. Fade IN (Screen goes Clear)
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