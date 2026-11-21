using System.Collections;
using System.Collections.Generic;
using MovingIn.Core.Structures;
using UnityEngine;
    namespace MovingIn.Minigames.Onaf{
    public class GameManager : MonoBehaviour
    {

        public Arvore HouseMap; 
        [SerializeField] GameObject House;

        void Awake()
        {
            int i = 0;
            while(House.transform.GetChild(i) != null)
            {

                i++;
            }

        }
        // Update is called once per frame
        void Update()
        {
            
        }
    }
}