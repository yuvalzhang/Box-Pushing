using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    private PlayerController playerController;
    private Dictionary<InputId, Action<InputAction.CallbackContext>> cachedInputActions = new();

    public void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        Action<InputAction.CallbackContext> inputAction = 
            (ctx) => { playerController.MoveUp(); };
        InputManager.Instance.AddInputListener(InputId.W, inputAction);
        cachedInputActions.Add(InputId.W, inputAction);
        
        inputAction = 
            (ctx) => { playerController.MoveLeft(); };
        InputManager.Instance.AddInputListener(InputId.A, inputAction);
        cachedInputActions.Add(InputId.A, inputAction);
        
        inputAction = 
            (ctx) => { playerController.MoveDown(); };
        InputManager.Instance.AddInputListener(InputId.S, inputAction);
        cachedInputActions.Add(InputId.S, inputAction);
        
        inputAction = 
            (ctx) => { playerController.MoveRight(); };
        InputManager.Instance.AddInputListener(InputId.D, inputAction);
        cachedInputActions.Add(InputId.D, inputAction);
        
        inputAction =
            (ctx) =>
            {
                if(StepManager.Instance != null)
                    StepManager.Instance.UndoStep();
            };
        InputManager.Instance.AddInputListener(InputId.Q, inputAction);
        cachedInputActions.Add(InputId.Q, inputAction);
        
        inputAction = 
            (ctx) => 
            { 
                if(GameManager.Instance != null)
                    GameManager.Instance.RestartLevel(); 
            };
        InputManager.Instance.AddInputListener(InputId.R, inputAction);
        cachedInputActions.Add(InputId.R, inputAction);
    }

    private void OnDisable()
    {
        if (InputManager.Instance == null) return;
        InputManager.Instance.RemoveInputListener(InputId.W, cachedInputActions[InputId.W]);
        InputManager.Instance.RemoveInputListener(InputId.A, cachedInputActions[InputId.A]);
        InputManager.Instance.RemoveInputListener(InputId.S, cachedInputActions[InputId.S]);
        InputManager.Instance.RemoveInputListener(InputId.D, cachedInputActions[InputId.D]);
        InputManager.Instance.RemoveInputListener(InputId.Q, cachedInputActions[InputId.Q]);
        InputManager.Instance.RemoveInputListener(InputId.R, cachedInputActions[InputId.R]);
        
    }
}
