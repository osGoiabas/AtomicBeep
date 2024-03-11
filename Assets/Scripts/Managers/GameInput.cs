using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public static GameInput instance;
    public static PlayerInput PlayerInput {get; private set; }
    public static Vector2 MoveInput {get; private set; }

    public static bool WasJumpPressed {get; private set; }
    public static bool IsJumpBeingPressed {get; private set; }
    public static bool WasJumpReleased {get; private set; }

    public static bool WasAttackPressed {get; private set; }
    public static bool WasBulletTimePressed {get; private set; }
    public static bool WasDebugPressed {get; private set; }
    public static bool WasPausePressed {get; private set; }
    public static bool WasUnpausePressed {get; private set; }

    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _attackAction;
    private InputAction _bulletTimeAction;
    private InputAction _debugAction;
    private InputAction _pauseAction;
    private InputAction _unpauseAction;



    private void Awake() {
        PlayerInput = GetComponent<PlayerInput>();

        _moveAction = PlayerInput.actions["Move"];
        _jumpAction = PlayerInput.actions["Pulo"];
        _attackAction = PlayerInput.actions["Ataque"];
        _bulletTimeAction = PlayerInput.actions["BulletTime"];
        _debugAction = PlayerInput.actions["Debug"];
        _pauseAction = PlayerInput.actions["Pause"];
        _unpauseAction = PlayerInput.actions["Unpause"];
    }


    private void Update()
    {
        MoveInput = _moveAction.ReadValue<Vector2>();

        WasJumpPressed = _jumpAction.WasPressedThisFrame();
        IsJumpBeingPressed = _jumpAction.IsPressed();
        WasJumpReleased = _jumpAction.WasReleasedThisFrame();

        WasAttackPressed = _attackAction.WasPressedThisFrame();
        WasBulletTimePressed = _bulletTimeAction.WasPressedThisFrame();
        WasDebugPressed = _debugAction.WasPressedThisFrame();
        WasPausePressed = _pauseAction.WasPressedThisFrame();    
        WasUnpausePressed = _unpauseAction.WasPressedThisFrame();       
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
}
