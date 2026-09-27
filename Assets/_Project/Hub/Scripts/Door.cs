using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MovingIn.Hub{
    ///<summary>
    /// porta entre dois comodos
    /// fica no vao da porta, dentro do comodo de ORIGEM
    /// quando o Player entra no trigger, o RoomManager troca pro comodo de destino
    /// e coloca o Player no spawnPoint (que fica dentro do comodo de destino)
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class Door : MonoBehaviour
    {
        [Tooltip("Comodo para onde esta porta leva")]
        [SerializeField] Room targetRoom;

        [Tooltip("Onde o Player aparece no comodo de destino (longe do trigger da porta de volta!)")]
        [SerializeField] Transform spawnPoint;

        // chamado pela Unity quando o componente e adicionado: ja marca o collider como trigger
        void Reset()
        {
            GetComponent<BoxCollider>().isTrigger = true;
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            if (RoomManager.Instance == null)
            {
                Debug.LogError("[Door] Nao existe RoomManager na cena.");
                return;
            }
            RoomManager.Instance.GoToRoom(targetRoom, spawnPoint);
        }

        // desenha a porta (verde) e uma linha ate o spawn na janela Scene
        void OnDrawGizmos()
        {
            BoxCollider box = GetComponent<BoxCollider>();
            if (box == null) return;

            Gizmos.color = new Color(0f, 1f, 0.4f, 0.35f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.matrix = Matrix4x4.identity;

            if (spawnPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, spawnPoint.position);
                Gizmos.DrawWireSphere(spawnPoint.position, 0.3f);
            }
        }
    }
}
