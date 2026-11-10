using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OutLimits : MonoBehaviour
{
    public UnityEvent FinishGameEvent = new UnityEvent();
    void OnTriggerEnter(Collider collider)
    {
        Debug.Log("Passou dos limites do mapa!");
        FinishGameEvent.Invoke();
        Time.timeScale = 0f;
    }
}
