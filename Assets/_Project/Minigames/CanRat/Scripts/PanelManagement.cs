using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PanelManagement : MonoBehaviour
{
    public UnityEvent PauseGame = new UnityEvent();
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
            PauseGame.Invoke(); 
    }
}
