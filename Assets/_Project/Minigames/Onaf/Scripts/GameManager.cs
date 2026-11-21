using System.Collections;
using System.Collections.Generic;
using MovingIn.Core.Structures;
using MovingIn.Hub;
using UnityEngine;
namespace MovingIn.Minigames.Onaf{
    public class GameManager : MonoBehaviour
    {
        
        Arvore HouseMap;
        public Arvore Tree => HouseMap; 
        [SerializeField] GameObject House;

        void Awake()
        {   
            HouseMap = new Arvore();
            int i = 0;
            bool ok = true;
            while(ok)
            {
                HouseMap.Insere(ordem[i], House.transform.GetChild(i).gameObject.GetComponent<Room>(), out ok);
                i++;
            }
        }

        // Update is called once per frame
        const int Sotao = 10;
        const int Corredor = 20;
        const int Porao = 30;
        const int Escada = 40;
        const int Lavanderia = 50;
        const int QuartoGaroto = 60;
        const int Sala = 70;
        const int Quintal = 80;
        const int Cozinha = 90;
        const int Garagem = 100;
        static int[] ordem = {QuartoGaroto, Corredor, Quintal, Sotao, 
                Escada, Sala, Garagem, Porao, Lavanderia, Cozinha};
    }
}