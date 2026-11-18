using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum ButtonType
{
    Normal,
    Deadlock
}

public class UIBindable : MonoBehaviour
{
    protected bool btnHasTriggered = false;
    private Dictionary<string , List<UIBehaviour>> controlDict = new ();
    
    private Dictionary<string, ButtonType> buttonRuleDict = new ();
    private Dictionary<string, float> lastClickTimeDict = new();
    private const float NORMAL_DEBOUNSE_TIME = 0.3f;
    
    //保存控件的默认名字，意味着这个控件只会起到显示作用
    private static HashSet<string> defaultNameList = new ()
    {
        "Image","Text (TMP)","RawImage","Background",
        "Checkmark","Label","Text (Legacy)","Arrow","Placeholder",
        "Fill","Handle","Viewport","Scrollbar Horizontal","Scrollbar Vertical"
    };

    protected virtual void Awake()
    {
        //增加button点击的规则
        InitButtonRules();
        
        //优先查找重要的组件
        //需要根据优先级排序
        FindAllChildrenControls<Button>(transform);
        FindAllChildrenControls<Image>(transform);
        FindAllChildrenControls<Text>(transform);
        FindAllChildrenControls<TMP_Text>(transform);
        FindAllChildrenControls<TextMeshProUGUI>(transform);
        FindAllChildrenControls<TextMeshPro>(transform);
        FindAllChildrenControls<Toggle>(transform);
        FindAllChildrenControls<Slider>(transform);
        FindAllChildrenControls<ScrollRect>(transform);
        FindAllChildrenControls<InputField>(transform);
        FindAllChildrenControls<Dropdown>(transform);
    }

    // protected virtual void OnEnable()
    // {
    //     btnHasTriggered = false;
    // }

    private void FindAllChildrenControls<T>(Transform currentTrans) where T : UIBehaviour
    {
        for (int i = 0; i < currentTrans.childCount; i++)
        {
            Transform childTrans = currentTrans.GetChild(i);

            
            // 如果这个子节点挂载了其他的 UIBindable，
            // 说明它是一个独立的 UI 模块，拥有自己的管辖权，直接跳过。
            if (childTrans.GetComponent<BasePanel>())
                continue; 

            // 在当前节点寻找所需类型的控件
            T control = childTrans.GetComponent<T>();
            if (control != null)
            {
                string objName = childTrans.gameObject.name;
                
                //过滤掉默认名字
                if (!defaultNameList.Contains(objName))
                {
                    if (controlDict.ContainsKey(objName))
                        controlDict[objName].Add(control);
                    else
                        controlDict.Add(objName, new List<UIBehaviour>() { control });

                    AddControlListener(control, objName);
                }
            }

            // 递归向下一层查找
            if (childTrans.childCount > 0)
            {
                FindAllChildrenControls<T>(childTrans);
            }
        }
    }
    
    private void AddControlListener(UIBehaviour control, string objName)
    {
        if(control is Button)
        {
            (control as Button).onClick.AddListener(() =>
            {
                if (btnHasTriggered) return;
                
                // 获取该按钮的专属类型（如果没有专门注册，默认就是 Normal）
                ButtonType currentType = ButtonType.Normal;
                if (buttonRuleDict.TryGetValue(objName, out ButtonType configuredType))
                {
                    currentType = configuredType;
                }

                switch (currentType)
                {
                    case ButtonType.Normal:
                        float currentTime = Time.unscaledTime;
                        if (lastClickTimeDict.TryGetValue(objName, out float lastClickTime))
                        {
                            if (currentTime - lastClickTime > NORMAL_DEBOUNSE_TIME)
                            {
                                //更新最新的点击时间戳
                                lastClickTimeDict[objName] = currentTime;
                                // OnClick(objName);
                            }
                        }

                        
                        break;
                     
                    case ButtonType.Deadlock: 
                        btnHasTriggered = true;
                        // OnClick(objName);
                        break;
                }
                OnClick(objName);
                
                
            });
        }
        else if (control is Toggle)
        {
            (control as Toggle).onValueChanged.AddListener((value) => { OnToggle(objName, value); });
        }
        else if (control is Slider)
        {
            (control as Slider).onValueChanged.AddListener((value) => { OnSliderValueChange(objName, value);});
        }
    }

    protected T GetControl<T>(string objName, bool isTolerant = false) where T : UIBehaviour
    {
        if (controlDict.ContainsKey(objName))
        {
            for (int i = 0; i < controlDict[objName].Count; i++)
            {
                T control = controlDict[objName][i] as T;
                if (control != null)
                    return control;
                
            }
        }
        if(!isTolerant)
            Debug.LogError($"(BasePanel.GetControl) Control not found or type mismatch: {objName} (Target Type: {typeof(T)})");
        return null;
    }
    
    protected virtual void OnClick(string objName)
    {
    }

    protected virtual void OnToggle(string objName, bool value)
    {
    }

    protected virtual void OnSliderValueChange(string objName, float value)
    {
    }

    protected virtual void InitButtonRules()
    {
        
    }

    protected void RegisterButtonRule(string objName, ButtonType type)
    {
        buttonRuleDict[objName] = type;
    }
}
