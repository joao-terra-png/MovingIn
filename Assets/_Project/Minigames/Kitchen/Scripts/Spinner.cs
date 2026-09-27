using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MovingIn.Minigames.Kitchen{

    public class Spinner : MonoBehaviour
    {

        [SerializeField] private float deegresPerSecond = 90f;

        private void Awake()
        {
        Debug.Log($"{name}: Awake");
        }

        private void Start()
        {
        Debug.Log($"{name}: Start");
        }

        private void Update()
        {
        // Time.deltaTime = segundos que passaram desde o último frame
        transform.Rotate(0f, 0f, deegresPerSecond * Time.deltaTime);
        }
    }

}