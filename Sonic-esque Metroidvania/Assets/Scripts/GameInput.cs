using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public event EventHandler OnPulo;
    private PlayerInputActions playerInputActions;

    private bool apertouBot�oPulo;

    private void Awake() {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        playerInputActions.Player.Pulo.performed += Pulo_performed;
    }

    public Vector2 GetMovementVector() {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        return inputVector;
    }

    public void Pulo_performed(InputAction.CallbackContext context) {
        OnPulo?.Invoke(this, EventArgs.Empty);
        //Debug.Log(context);
        if (context.performed)
        {
            apertouBot�oPulo = true;
            Debug.Log("JUMP!" + context.phase);
        }
        else {
            apertouBot�oPulo = false;
            Debug.Log("N�O PULOU!" + context.phase);
        }

    }

    public bool checkApertouBot�oPulo()
    {
        return apertouBot�oPulo;
    }
}
