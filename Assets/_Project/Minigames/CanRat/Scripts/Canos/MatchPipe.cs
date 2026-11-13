using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MovingIn.Minigames.CanRat
{
    public class MatchPipe : MonoBehaviour
    {
        /*
        Função: TestaEncaixe
        Recebe uma string (tag de um Cano) e verifica se ele pode ser encaixado
            de acordo com a tag do cano já posicionado
        
        */
        public bool TestaEncaixe(string tag)
        {
            switch (gameObject.tag)
            {
                case "RightToUpPipe":
                    return (tag == "VerticalPipe") || (tag == "UpToRightPipe");
                case "RightToDownPipe":
                    return (tag == "VerticalPipe") || (tag == "DownToRightPipe");
                case "UpToRightPipe":
                    return (tag == "HorizontalPipe") || (tag == "RightToUpPipe") || (tag == "RightToDownPipe");
                case "DownToRightPipe":
                    return (tag == "HorizontalPipe") || (tag == "RightToUpPipe") || (tag == "RightToDownPipe");
                case "HorizontalPipe":
                    return (tag == "RightToUpPipe") || (tag == "RightToDownPipe") || (tag == "HorizontalPipe");
                case "VerticalPipe":
                    return (tag == "UpToRightPipe") || (tag == "DownToRightPipe") || (tag == "VerticalPipe");
                default: 
                    return false;
            }
        }
    
        /*
        Descobre qual o Próximo cano e retorna sua tag
        Utiliza um rayCast para procurar o próximo cano, de acordo com o cano atual
        */
        public string ProxCano()
        {
            RaycastHit procuraProx;
    
            switch (gameObject.tag)
            {
                case "RightToUpPipe":
                    if (Physics.Raycast(transform.position, Vector3.up, out procuraProx, 1f))
                        return procuraProx.collider.gameObject.tag;
                    else
                        return null;
                case "RightToDownPipe":
                    if (Physics.Raycast((transform.position + 0.18f *Vector3.down), // +0.18f *Vector3.down para não conflitar com ele mesmo
                            Vector3.down, out procuraProx, 1f))                //Visto que a plataforma horizontal esta abaixo do centro
                        return procuraProx.collider.gameObject.tag; 
                    else
                        return null;
                case "UpToRightPipe":
                    if (Physics.Raycast(transform.position+ 0.18f * Vector3.down, Vector3.right, out procuraProx, 1f))
                        return procuraProx.collider.gameObject.tag;
                    else
                        return null;
                case "DownToRightPipe":
                    if (Physics.Raycast(transform.position+ 0.18f * Vector3.down, Vector3.right, out procuraProx, 1f))
                        return procuraProx.collider.gameObject.tag;
                    else
                        return null;
                case "HorizontalPipe":
                    if(Physics.Raycast(transform.position + 0.18f * Vector3.down, Vector3.right, out procuraProx, 1f))
                        return procuraProx.collider.gameObject.tag;
                    else
                        return null;
                default: 
                    return null;
            }
    
        }
    }
}