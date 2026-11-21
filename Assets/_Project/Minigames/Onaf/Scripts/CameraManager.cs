using System.Collections;
using System.Collections.Generic;
using MovingIn.Core.Structures;
using MovingIn.Hub;
using UnityEngine;
namespace MovingIn.Minigames.Onaf{
    public class CameraManager : MonoBehaviour
    {

        [SerializeField] QuartoNode currentRoom;
        [SerializeField] int idCurrentRoom;
        [SerializeField] GameObject SecurityCamera;
        [SerializeField] Arvore Tree;

        bool IsCamOn {get; set;}
        // Start is called before the first frame update
        void Start()
        {
            idCurrentRoom = 1;
            currentRoom = null;
            SecurityCamera = transform.GetChild(0).gameObject;
        }

        //Chamado ao clicar no botão da camera (evento)
            //cada botão na camera tem um valor assinado para sua
        public void SwitchCamera(int value)
        {
            if(Tree.GetQuarto(value) != null)
                idCurrentRoom = value;

            if(currentRoom.GetID() != idCurrentRoom){
                currentRoom = Tree.GetQuarto(idCurrentRoom);
                SecurityCamera.GetComponent<IsoCamera>().SnapTo(currentRoom.GetRoom());
            }
        }

        void Update()
        {
            //A princípio, o sinal é de fato a camera ligada ou não
            IsCamOn = SecurityCamera.GetComponent<Camera>().enabled;
            if(currentRoom.GetBoneco() != null)
                currentRoom.GetBoneco().movEnable = !IsCamOn;
        }
    }
}