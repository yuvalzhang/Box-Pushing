using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> where T : class
{
    private static T instance;
    
    //用于加锁的对象
    protected static readonly object lockObject = new object();

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        //通过反射获取私有的无参构造函数
                        Type type = typeof(T);
                        ConstructorInfo info = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance,
                            null,
                            Type.EmptyTypes,
                            null);

                        if (info != null)
                            instance = info.Invoke(null) as T;
                        else
                            Debug.LogError("没有找到对应的无参构造函数：" + typeof(T).Name);
                    }
                }
            }
            return instance;
        }
    }
    
    // 构造函数：当 T 被 new 出来时会自动调用父类构造函数
    protected Singleton()
    {
        // 自动注册退出事件
        Application.quitting += HandleApplicationQuit;
    }

    // 处理退出逻辑
    private void HandleApplicationQuit()
    {
        // 1. 移除监听，防止内存泄漏（虽然程序都要退出了，但养成好习惯）
        Application.quitting -= HandleApplicationQuit;

        // 2. 调用子类可能需要的自定义清理逻辑
        OnDestroy();

        // 3. 【核心】彻底清除静态引用，确保下次运行（或重新加载）时执行新的构造函数
        instance = null;
        
        Debug.Log($"[Singleton] {typeof(T).Name} 已自动清理/重置。");
    }

    // 虚方法：子类如果需要手动销毁内部变量（如移除事件监听、清空List），可以重写此方法
    protected virtual void OnDestroy() 
    { 
        // 默认什么都不做
    }
}

public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    public static T Instance => instance;

    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            //只移除脚本而不是gameobject
            Destroy(this);
            return;
        }
        instance = this as T;
        Application.quitting += HandleApplicationQuit;
    }
    
    private void HandleApplicationQuit()
    {
        Application.quitting -= HandleApplicationQuit;

        OnDestroy();

        if (instance == this)
        {
            instance = null;
        }
        
        Debug.Log($"[Singleton] {typeof(T).Name} 已自动清理/重置。");
    }
    
    protected virtual void OnDestroy()
    {

    }
}
