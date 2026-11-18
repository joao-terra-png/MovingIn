using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MovingIn.Minigames.Kitchen
{
    public class BoxPulsate : MonoBehaviour
    {
        [SerializeField] float _amplitude = 0.1f;
        [SerializeField] float _speed = 3f;
  

        private Vector3 _originalScale;

        void Awake(){
            _originalScale = transform.localScale;
        }

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            float wave = (Mathf.Sin(Time.time * _speed) + 1f) / 2f;
            float increase = wave * _amplitude;
            float factor = 1f + increase;
            transform.localScale = _originalScale * factor;
        }
    }
}

