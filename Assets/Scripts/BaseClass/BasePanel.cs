using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasePanel :  UIBindable
{
    
    public virtual void ShowMe()
    {
        if(!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }
        
        btnHasTriggered = false;
        
    }

    public  virtual void HideMe()
    {
        if(gameObject.activeInHierarchy)
        {
            gameObject.SetActive(false);
        }
    }
    
}
