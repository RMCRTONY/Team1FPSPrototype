using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;

public class UserInput : MonoBehaviour
{
    public static UserInput instance;

    // detect movement
    public Vector2 MoveInput { get; private set; }
    //public Vector2 MoveInputHeld { get; private set; }
    //public Vector2 MoveInputReleased { get; private set; }
    // detect jump
    public bool JumpPressed { get; private set; }
    // detect interact
    public bool InteractPressed { get; private set; }
    // primary fire
    public bool PrimaryFireInput { get; private set; }
    // alt fire
    public bool AltFireInput { get; private set; }
    // swap weapon
    public float SwapWeaponInput { get; private set; }
    // swap ability
    public bool SwapAbilityUpInput { get; private set; }
    public bool SwapAbilityDownInput { get; private set; }
    // sprint pressed
    public bool SprintPressed { get; private set; }
    // sprint held
    public bool SprintHeld { get; private set; }
    // sprint released
    public bool SprintReleased { get; private set; }
    // menu open close
    public bool MenuOpenCloseInput { get; private set; }

    private PlayerInput _playerInput;

    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _interactAction;
    private InputAction _primaryFireAction;
    private InputAction _altFireAction;
    private InputAction _swapWeaponAction;
    private InputAction _swapAbilityUpAction;
    private InputAction _swapAbilityDownAction;
    private InputAction _sprintAction;
    private InputAction _menuOpenCloseAction;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        _playerInput = GetComponent<PlayerInput>();

        SetupInputActions();
    }

    private void Update()
    {
        UpdateInputs();
    }

    private void UpdateInputs()
    {
        MoveInput = _moveAction.ReadValue<Vector2>();
        JumpPressed = _jumpAction.WasPressedThisFrame();
        InteractPressed = _interactAction.WasPressedThisFrame();
        PrimaryFireInput = _primaryFireAction.WasPressedThisFrame();
        AltFireInput = _altFireAction.WasPressedThisFrame();
        SwapWeaponInput = _swapWeaponAction.ReadValue<float>();
        SwapAbilityUpInput = _swapAbilityUpAction.WasPressedThisFrame();
        SwapAbilityDownInput = _swapAbilityDownAction.WasPressedThisFrame();
        SprintPressed = _sprintAction.WasPressedThisFrame();
        SprintHeld = _sprintAction.IsPressed();
        SprintReleased = _sprintAction.WasReleasedThisFrame();
        MenuOpenCloseInput = _menuOpenCloseAction.WasPressedThisFrame();
    }

    private void SetupInputActions()
    {
        _moveAction = _playerInput.actions["Move"];
        _jumpAction = _playerInput.actions["Jump"];
        _interactAction = _playerInput.actions["Interact"];
        _primaryFireAction = _playerInput.actions["Primary Fire"];
        _altFireAction = _playerInput.actions["Alt Fire"];
        _swapWeaponAction = _playerInput.actions["Swap Weapon"];
        _swapAbilityUpAction = _playerInput.actions["Swap Ability Up"];
        _swapAbilityDownAction = _playerInput.actions["Swap Ability Down"];
        _sprintAction = _playerInput.actions["Sprint"];
        _menuOpenCloseAction = _playerInput.actions["MenuOpenClose"];
    }
}
