using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RatMovement : MonoBehaviour
{
    //Acesso por referencia ao Raio de Visão 
    public RatVision _ratVision;
    public MatchPipe _matchPipe;
    void Start()
    {
        _ratVision = GetComponent<RatVision>();
    }
    void Update()
    {
        if (_ratVision.FloorVision() && !(_ratVision.StraighVision()))
        {
            ForwardMove();
        }
        else if (_ratVision.StraighVision())
        {
            YMove();
        }
    }

    void ForwardMove() // Movimento unidirecional para frente
    {
        transform.position += 0.5f * Vector3.right;
    }

    void YMove() // Movimento em canos curvos  (no eixo Y)
    {
        GameObject CanoCurvo = _ratVision.CollisionPoint.collider.gameObject;
        string tagProxCano = CanoCurvo.GetComponent<MatchPipe>().ProxCano();
        bool encaixe = CanoCurvo.GetComponent<MatchPipe>().TestaEncaixe(tagProxCano);
        Vector3 Movement; 
        if(encaixe){
            switch (tagProxCano) // Apenas movimenta o hamster caso haja uma plataforma com parte horizontal
            {
                case "UpToRightPipe":
                    Movement = Vector3.up + 0.5f * Vector3.right;
                    break;
                case "DownToRightPipe":
                    Movement = Vector3.down + 0.5f * Vector3.right;
                    break;
                default:
                    Movement = 0 * Vector3.zero;
                    break;
            }
        }
        else 
            Movement = Vector3.zero;
        transform.position += Movement;
    }
}
