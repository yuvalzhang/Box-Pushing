using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum InputId
{
    W,A,S,D,
    Q,R,
    Menu
}

public enum InputListenPhase
{
    Started,
    Performed,
    Canceled,
}

public class InputManager : SingletonMono<InputManager>
{
    private PlayerInputControl inputControl;
    private Dictionary<InputId, InputAction> actionMap;

    private bool isQuitting = false;
    
    #region Life Cycle

    public void InitInputAction()
    {
        // 防止重复初始化
        if (inputControl != null)
            return;

        inputControl = new PlayerInputControl();

        actionMap = new Dictionary<InputId, InputAction>
        {
            { InputId.W, inputControl.Player.Up },
            { InputId.A, inputControl.Player.Left },
            { InputId.S, inputControl.Player.Down },
            { InputId.D, inputControl.Player.Right },
            { InputId.Q, inputControl.Player.Withdraw },
            { InputId.R, inputControl.Player.RestartLevel },
            
            { InputId.Menu, inputControl.Player.Back },
            
        };
    }
    
    public void EnablePlayerInputMap()
    {
        if (inputControl == null) return;
        inputControl.Player.Enable();
    }
    
    public void EnableUIInputMap()
    {
        if (inputControl == null) return;
        inputControl.UI.Enable();
    }

    public void DisablePlayerInputMap()
    {
        if (inputControl == null) return;
        inputControl.Player.Disable();
    }

    private void DisableInputAction()
    {
        if (inputControl == null) return;
        inputControl.Player.Disable();
        inputControl.UI.Disable();
    }
    
    #endregion
    
    #region Input Listeners

    /// <summary>
    /// 往自定义的输入添加指定的回调事件
    /// </summary>
    /// <param name="inputId"></param>
    /// <param name="inputListener">输入上下文</param>
    /// <param name="inputListenPhase">输入读取方式</param>
    public void AddInputListener(
        InputId inputId,
        System.Action<InputAction.CallbackContext> inputListener,
        InputListenPhase inputListenPhase = InputListenPhase.Performed)
    {
        if (inputListener == null)
        {
            Debug.LogWarning("AddPlayerInputListener: inputListener is null");
            return;
        }

        if (inputControl == null || actionMap == null)
        {
            Debug.LogWarning("AddPlayerInputListener: input system not initialized");
            return;
        }

        if (!actionMap.TryGetValue(inputId, out var action))
        {
            // Debug.LogWarning("AddPlayerInputListener: inputId not found " + inputId);
            return;
        }
        // Debug.LogWarning("AddPlayerInputListener: Adding input listener for " + inputId);
        switch (inputListenPhase)
        {
            case InputListenPhase.Started:
                action.started += inputListener;
                break;

            case InputListenPhase.Performed:
                action.performed += inputListener;
                break;

            case InputListenPhase.Canceled:
                action.canceled += inputListener;
                break;
        }
    }
    
    public void RemoveInputListener(
        InputId inputId,
        System.Action<InputAction.CallbackContext> inputListener,
        InputListenPhase inputListenPhase = InputListenPhase.Performed)
    {
        if (inputListener == null)
        {
            Debug.LogWarning("RemovePlayerInputListener: inputListener is null");
            return;
        }

        if (actionMap == null)
            return;

        if (!actionMap.TryGetValue(inputId, out var action))
            return;

        switch (inputListenPhase)
        {
            case InputListenPhase.Started:
                action.started -= inputListener;
                break;

            case InputListenPhase.Performed:
                action.performed -= inputListener;
                break;

            case InputListenPhase.Canceled:
                action.canceled -= inputListener;
                break;
        }
    }

    #endregion
    
    #region Unity Lifecycle
    
    public void CancelAllInputActions()
    {
        if (actionMap == null)
            return;

        foreach (var action in actionMap.Values)
        {
            if (action == null)
                continue;

            // 如果 action 正在运行，Disable 会触发 canceled
            if (action.enabled)
            {
                action.Disable();
            }
            
        }
    }

    void OnApplicationQuit()
    {
        isQuitting = true;
    }

    protected override void OnDestroy()
    {
        // 避免 PlayMode 退出顺序问题
        if (!isQuitting)
        {
            if (EventCenter.Instance != null)
            {
                EventCenter.Instance.EventTrigger("取消输入监听");
            }
        }

        // 先触发所有 canceled
        CancelAllInputActions();

        // 再关闭整个 InputSystem
        DisableInputAction();

        if (inputControl != null)
        {
            inputControl.Dispose();
            inputControl = null;
        }

        actionMap?.Clear();
        actionMap = null;

        base.OnDestroy();
    }

    #endregion
}
