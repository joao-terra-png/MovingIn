using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MovingIn.Hub{
    ///<summary>
    /// representa um comodo da casa
    /// vai no objeto PAI do comodo (ex: Casa/Quarto)
    /// tudo que pertence ao comodo (chao, paredes, caixas, portas) fica dentro dele
    /// o RoomManager liga so o comodo atual e desliga os outros
    /// </summary>
    public class Room : MonoBehaviour
    {
        [SerializeField] string _roomName = "Comodo";

        [Tooltip("Ponto que a camera enquadra. Se ficar vazio, usa a posicao deste objeto")]
        [SerializeField] Transform cameraFocus;

        [Tooltip("Size da camera ortografica neste comodo (maior = camera mais afastada)")]
        [SerializeField] float _cameraSize = 6f;

        public string RoomName => _roomName;
        public Vector3 FocusPoint => cameraFocus != null ? cameraFocus.position : transform.position;
        public float CameraSize => _cameraSize;

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);
    }
}
