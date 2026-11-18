using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IEventInfo { }

#region EventInfo

public class EventInfo : IEventInfo
{
    public event Action actions;

    public EventInfo(Action action)
    {
        actions += action;
    }
    /// <summary>
    /// 无 GC 的去重添加方式
    /// </summary>
    public void AddListener(Action action)
    {
        // 核心技巧：先减后加。如果原本不存在，-= 不会报错；
        // 如果已存在，-= 会将其移除，然后 += 会将其加到末尾。
        // 这样既保证了唯一性，又完全不产生 GetInvocationList 的数组分配。
        actions -= action;
        actions += action;
    }

    public void RemoveListener(Action action)
    {
        actions -= action;
    }

    public bool IsEmpty()
    {
        return actions == null;
    }

    public void InvokeAction()
    {
        
        actions?.Invoke();
    }
}

public class EventInfo<T> : IEventInfo
{
    public event Action<T> actions;

    public EventInfo(Action<T> action)
    {
        actions += action;
    }

    public void AddListener(Action<T> action)
    {
        actions -= action;
        actions += action;
    }

    public void RemoveListener(Action<T> action)
    {
        actions -= action;
    }

    public bool IsEmpty()
    {
        return actions == null;
    }
    
    public void InvokeAction(T arg)
    {
        actions?.Invoke(arg);
    }
}

#endregion

[DisallowMultipleComponent]
public class EventCenter : SingletonMono<EventCenter>
{
    private readonly Dictionary<string, IEventInfo> eventDictionary = new();
    
    // private bool isQuitting = false;

    #region Add Listener

    public void AddEventListener(string eventName, Action listener)
    {
        if (string.IsNullOrEmpty(eventName) || listener == null)
        {
            Debug.LogWarning("AddEventListener invalid parameter");
            return;
        }

        if (eventDictionary.TryGetValue(eventName, out var info))
        {
            EventInfo eventInfo = info as EventInfo;

            if (eventInfo == null)
            {
                Debug.LogError($"Event type mismatch: {eventName}");
                return;
            }

            eventInfo.AddListener(listener);
        }
        else
        {
            eventDictionary.Add(eventName, new EventInfo(listener));
        }
    }

    public void AddEventListener<T>(string eventName, Action<T> listener)
    {
        if (string.IsNullOrEmpty(eventName) || listener == null)
        {
            Debug.LogWarning("AddEventListener<T> invalid parameter");
            return;
        }

        if (eventDictionary.TryGetValue(eventName, out var info))
        {
            EventInfo<T> eventInfo = info as EventInfo<T>;

            if (eventInfo == null)
            {
                Debug.LogError($"Event type mismatch: {eventName}");
                return;
            }

            eventInfo.AddListener(listener);

        }
        else
        {
            eventDictionary.Add(eventName, new EventInfo<T>(listener));
        }
    }

    #endregion

    #region Remove Listener

    public void RemoveEventListener(string eventName, Action listener)
    {
        if (string.IsNullOrEmpty(eventName) || listener == null)
            return;

        if (!eventDictionary.TryGetValue(eventName, out var info))
            return;

        EventInfo eventInfo = info as EventInfo;

        if (eventInfo == null)
            return;

        eventInfo.RemoveListener(listener);

        //如果没有监听者，删除整个事件
        if (eventInfo.IsEmpty())
            eventDictionary.Remove(eventName);
    }

    public void RemoveEventListener<T>(string eventName, Action<T> listener)
    {
        if (string.IsNullOrEmpty(eventName) || listener == null)
            return;

        if (!eventDictionary.TryGetValue(eventName, out var info))
            return;

        EventInfo<T> eventInfo = info as EventInfo<T>;

        if (eventInfo == null)
            return;

        eventInfo.RemoveListener(listener);

        if (eventInfo.IsEmpty())
            eventDictionary.Remove(eventName);
    }

    #endregion

    #region Trigger Event

    public void EventTrigger(string eventName)
    {
        if (string.IsNullOrEmpty(eventName))
            return;

        if (!eventDictionary.TryGetValue(eventName, out var info))
            return;

        EventInfo eventInfo = info as EventInfo;

        if (eventInfo == null)
            return;

        eventInfo.InvokeAction();

        // 清理后如果为空删除
        if (eventInfo.IsEmpty())
            eventDictionary.Remove(eventName);
    }

    public void EventTrigger<T>(string eventName, T param)
    {
        if (string.IsNullOrEmpty(eventName))
            return;

        if (!eventDictionary.TryGetValue(eventName, out var info))
            return;

        EventInfo<T> eventInfo = info as EventInfo<T>;

        if (eventInfo == null)
            return;
        
        eventInfo.InvokeAction(param);

        if (eventInfo.IsEmpty())
            eventDictionary.Remove(eventName);
    }

    #endregion

    #region Utility

    public void RemoveEventCatalogue(string eventName)
    {
        eventDictionary.Remove(eventName);
    }

    public void ClearEvents()
    {
        eventDictionary.Clear();
    }

    #endregion

    #region Lifecycle

    // void OnApplicationQuit()
    // {
    //     isQuitting = true;
    // }
    //
    // protected override void OnDestroy()
    // {
    //     isQuitting = true;
    //     base.OnDestroy();
    // }

    #endregion
}