using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RatVision : MonoBehaviour
{
    Vector3 ComponenteHorizontal = Vector3.right;
    Vector3 ComponenteVertical = Vector3.down;
    Vector3 vetorSolo;
    RaycastHit FloorPoint;
    public RaycastHit CollisionPoint;
    

    void Start()
    {
        //Calculo da distancia necessária para andar pro proximo cano (vetor)
        vetorSolo = ((transform.localScale.y/2) * ComponenteVertical) +
            ComponenteHorizontal;
    }
    /*
    Função: FloorVision
    @param: void
    @return: 1, se há solo(cano) disponível à frente, e 0 se não
    */
    public bool FloorVision()
    {
        if(Physics.Raycast(transform.position, vetorSolo, out FloorPoint,3))
        {
            Debug.DrawLine(transform.position, FloorPoint.point);
            return true;    
        } 
        else
        {
            return false;
        }
    }

    /*
    Função: StraighVision
    @param: void
    @return: 1, se há obstáculo à frente, 0 senão
    */
    public bool StraighVision()
    {
        if(Physics.Raycast(transform.position, Vector3.right, out CollisionPoint, 0.5f))
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
