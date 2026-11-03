using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class FinishGameEvent : MonoBehaviour
{
    public UnityEvent ShowFinishPanel = new UnityEvent();
    public void FinishGame()
    {
        ShowFinishPanel.Invoke();
        Time.timeScale = 0f;
    }
}
