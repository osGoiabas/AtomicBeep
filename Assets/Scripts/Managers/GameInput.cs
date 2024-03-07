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





    //TESTE PRA ORGANIZAR O INPUT
    public static PlayerInput PlayerInput;
    public static Vector2 MoveInput;

    public static bool WasJumpPressed;
    public static bool IsJumpBeingPressed;
    public static bool WasJumpReleased;

    public static bool WasAttackPressed;
    public static bool WasBulletTimePressed;
    public static bool WasDebugPressed;
    public static bool WasPausePressed {get; private set; }

    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _attackAction;
    private InputAction _bulletTimeAction;
    private InputAction _debugAction;
    private InputAction _pauseAction;



    private void Awake() {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        playerInputActions.Player.Pulo.performed += Pulo_performed;
        playerInputActions.Player.BulletTime.performed += BulletTime_performed;
        playerInputActions.Player.Ataque.performed += Ataque_performed;

        //TESTE PRA ORGANIZAR O INPUT
        PlayerInput = GetComponent<PlayerInput>();

        _moveAction = PlayerInput.actions["Move"];
        _jumpAction = PlayerInput.actions["Pulo"];
        _attackAction = PlayerInput.actions["Ataque"];
        _bulletTimeAction = PlayerInput.actions["BulletTime"];
        _debugAction = PlayerInput.actions["Debug"];
        _pauseAction = PlayerInput.actions["Pause"];
    }


    private void Update()
    {
        //TESTE PRA ORGANIZAR O INPUT

        MoveInput = _moveAction.ReadValue<Vector2>();

        WasJumpPressed = _jumpAction.WasPressedThisFrame();
        IsJumpBeingPressed = _jumpAction.IsPressed();
        WasJumpReleased = _jumpAction.WasReleasedThisFrame();

        WasAttackPressed = _attackAction.WasPressedThisFrame();
        WasBulletTimePressed = _bulletTimeAction.WasPressedThisFrame();
        WasDebugPressed = _debugAction.WasPressedThisFrame();
        WasPausePressed = _pauseAction.WasPressedThisFrame();         
    }

    private void Start()
    {
        
        if (instance != null)
        {
            //Debug.LogWarning($"Duplicate Instances found! First one: {instance.name} Second one: {name}. Destroying second one.");
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
        //DontDestroyOnLoad(gameObject);
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
