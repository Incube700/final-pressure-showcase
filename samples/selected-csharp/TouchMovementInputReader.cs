// Curated FINAL PRESSURE portfolio sample. Public namespace adapted for presentation.
using UnityEngine;

namespace FinalPressure.Gameplay.Features.Player
{
    public sealed class TouchMovementInputReader : IResettablePlayerMovementInput
    {
        private readonly IPlayerMovementInput _fallbackInput;
        private readonly FloatingJoystickView _joystickView;

        public TouchMovementInputReader(
            IPlayerMovementInput fallbackInput,
            FloatingJoystickView joystickView)
        {
            _fallbackInput = fallbackInput;
            _joystickView = joystickView;
        }

        public Vector2 MovementInput
        {
            get
            {
                if (_joystickView != null)
                {
                    _joystickView.RefreshActivePointer();
                    if (_joystickView.HasActivePointer)
                        return _joystickView.Direction;
                }

                return _fallbackInput?.MovementInput ?? Vector2.zero;
            }
        }

        public void SetInputEnabled(bool isEnabled, bool preserveActivePointer = false)
        {
            if (_joystickView != null)
                _joystickView.SetInputEnabled(isEnabled, preserveActivePointer);
        }

        public void Reset()
        {
            if (_joystickView != null)
                _joystickView.ResetInput();
        }

        public void ResetInput()
        {
            Reset();
        }
    }
}
