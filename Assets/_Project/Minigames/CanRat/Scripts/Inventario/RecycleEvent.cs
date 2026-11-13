using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MovingIn.Minigames.CanRat{
public class RecycleEvent : MonoBehaviour
    {
        public UnityEvent PipeRecycle = new UnityEvent();
        PipeSelection _pipeSelection;
        void Start()
        {
            _pipeSelection = GetComponent<PipeSelection>();
        }

        // Update is called once per frame
        void Update()
        {
            if (_pipeSelection.GetIsSelected())
            {
                PipeRecycle.Invoke();
                _pipeSelection.SelectPipe();
                _pipeSelection.MarkSelected();

            }
        }
    }
}