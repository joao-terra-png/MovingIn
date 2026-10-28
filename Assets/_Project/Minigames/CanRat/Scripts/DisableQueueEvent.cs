using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using MovingIn.Minigames.CanRat;

public class DisableQueueEvent : MonoBehaviour
{
    public UnityEvent DisableQueue;

    void ChegouAoDestino()
    {
        if(!gameObject.GetComponent<EmptyPipe>().IsEmpty())
            DisableQueue.Invoke();
    }
}
