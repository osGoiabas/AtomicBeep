using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public event EventHandler OnPulo;
    public event EventHandler OnBulletTime;
    public event EventHandler OnAtaque;
    private PlayerInputActions playerInputActions;

    public static GameInput instance;

    private void Awake() {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        playerInputActions.Player.Pulo.performed += Pulo_performed;
        playerInputActions.Player.BulletTime.performed += BulletTime_performed;
        playerInputActions.Player.Ataque.performed += Ataque_performed;
    }



    void Start()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }

    public Vector2 GetMovementVector() {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        return inputVector;
    }

    public void Pulo_performed(InputAction.CallbackContext context) {
        OnPulo?.Invoke(this, EventArgs.Empty);
    }

    public void BulletTime_performed(InputAction.CallbackContext context)
    {
        OnBulletTime?.Invoke(this, EventArgs.Empty);
    }

    public void Ataque_performed(InputAction.CallbackContext context)
    {
        OnAtaque?.Invoke(this, EventArgs.Empty);
    }
}
