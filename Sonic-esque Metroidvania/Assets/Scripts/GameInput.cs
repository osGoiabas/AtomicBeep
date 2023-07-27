using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public event EventHandler OnPulo;
    public event EventHandler OnBulletTime;
    private PlayerInputActions playerInputActions;

    private void Awake() {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        playerInputActions.Player.Pulo.performed += Pulo_performed;
        playerInputActions.Player.Pulo.canceled += Pulo_canceled;
        playerInputActions.Player.BulletTime.performed += BulletTime_performed;
        playerInputActions.Player.BulletTime.canceled += BulletTime_canceled;
    }

    public Vector2 GetMovementVector() {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        return inputVector;
    }

    public void Pulo_performed(InputAction.CallbackContext context) {
        OnPulo?.Invoke(this, EventArgs.Empty);
    }

    public void Pulo_canceled(InputAction.CallbackContext context)
    { 
    
    }

    public void BulletTime_performed(InputAction.CallbackContext context)
    {
        OnBulletTime?.Invoke(this, EventArgs.Empty);
    }

    public void BulletTime_canceled(InputAction.CallbackContext context)
    { 
    
    }
}
