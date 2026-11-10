using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using MovingIn.Minigames.CanRat;


namespace MovingIn.Minigames.CanRat.RatMov
{
    public class RatVision : MonoBehaviour
    {
        Vector3 ComponenteHorizontal;
        Vector3 ComponenteVertical;
        Vector3 vetorSolo;
        RaycastHit FloorPoint;
        public RaycastHit CollisionWall;


        void Start()
        {
            ComponenteVertical = Vector3.down;
            ComponenteHorizontal = 0.2f *Vector3.right;
            //Calculo da distancia necessária para andar pro proximo cano (vetor), scale pelo seu tamanho
            vetorSolo = (transform.lossyScale.y/1.5f * ComponenteVertical) +
                ComponenteHorizontal;
        }
        /*
        Função: FloorVision
        @param: void
        @return: 1, se há solo(cano) disponível à frente, e 0 se não
        */
        public bool FloorVision()
        {
            if(Physics.Raycast(transform.position, vetorSolo, out FloorPoint,0.25f))
            {
                Debug.DrawLine(transform.position, FloorPoint.point);
                return true;    
            } 
            else
                return false;
        }

        /*
        Função: StraighVision
        @param: void
        @return: 1, se há obstáculo à frente, 0 senão
        */
        public bool StraighVision()
        {
            if(Physics.Raycast(transform.position, Vector3.right, out CollisionWall, 0.25f))
            {
                Debug.DrawLine(transform.position, Vector3.right);
                return true;    
            } 
            else
            {
                return false;
            }
        }

    }
}