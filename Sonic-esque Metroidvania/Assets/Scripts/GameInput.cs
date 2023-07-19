using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    private PlayerInputActions playerInputActions;

    private void Awake() {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        playerInputActions.Player.Pulo.performed += Jump;
    }

    public Vector2 GetMovementVector() {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        return inputVector;
    }

    public void Jump(InputAction.CallbackContext context) {
        Debug.Log(context);
        if (context.performed) {
            Debug.Log("JUMP!" + context.phase);
        }        
    }
}
