using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    public Transform background_1;
    public Transform background_2;
    public float generatePosX = 21.79f;
    public float triggerPosX = -21.79f;

    public float scrollSpeed = 0.5f;

    public void Update()
    {
        Vector3 targetPosition1 = new Vector3(
            background_1.transform.position.x - scrollSpeed * Time.deltaTime, 
            background_1.transform.position.y, 
            background_1.transform.position.z);
        background_1.transform.position = targetPosition1;
        
        Vector3 targetPosition2 = new Vector3(
            background_2.transform.position.x - scrollSpeed * Time.deltaTime, 
            background_2.transform.position.y, 
            background_2.transform.position.z);
        background_2.transform.position = targetPosition2;

        
        ChangePosition();
    }

    private void ChangePosition()
    {
        if (background_1.transform.position.x < triggerPosX)
        {
            background_1.transform.position = new Vector3(generatePosX, 0, 0);
        }
        if(background_2.transform.position.x < triggerPosX)
        {
            background_2.transform.position = new Vector3(generatePosX, 0, 0);
        }
    }
}
