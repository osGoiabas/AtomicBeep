using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public event EventHandler OnPulo;
    private PlayerInputActions playerInputActions;

    private bool apertouBotãoPulo;

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
            apertouBotãoPulo = true;
            Debug.Log("JUMP!" + context.phase);
        }
        else {
            apertouBotãoPulo = false;
            Debug.Log("NÃO PULOU!" + context.phase);
        }

    }

    public bool checkApertouBotãoPulo()
    {
        return apertouBotãoPulo;
    }
}
