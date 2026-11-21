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
        [SerializeField] GameObject Manager;
        Arvore tree;
        [SerializeField] EnergyManager energia;

        bool IsCamOn {get; set;}
        // Start is called before the first frame update
        void Start()
        {
            idCurrentRoom = 60;
            currentRoom = Manager.GetComponent<GameManager>().Tree.GetRaiz();
            SecurityCamera = transform.GetChild(0).gameObject;

            if(energia != null)
                energia.EnergiaAcabou += DesligarCamera;
            
            tree = Manager.GetComponent<GameManager>().Tree;
        }

        //Chamado ao clicar no botão da camera (evento)
            //cada botão na camera tem um valor assinado para sua
        public void SwitchCamera(int value)
        {
            if(tree.GetQuarto(value) != null)
                idCurrentRoom = value;

            if(currentRoom.GetID() != idCurrentRoom){
                currentRoom.GetRoom().gameObject.SetActive(false);
                currentRoom = tree.GetQuarto(idCurrentRoom);
                currentRoom.GetRoom().gameObject.SetActive(true);
                SecurityCamera.GetComponent<IsoCamera>().SnapTo(currentRoom.GetRoom());
            }
        }

        void Update()
        {
            //A princípio, o sinal é de fato a camera ligada ou não
            IsCamOn = SecurityCamera.GetComponent<Camera>().enabled;

            if(energia != null)
                energia.SetCameraAtiva(IsCamOn);

            if(currentRoom != null && currentRoom.GetBoneco() != null)
                currentRoom.GetBoneco().movEnable = !IsCamOn;
        }

        void DesligarCamera()
        {
            SecurityCamera.GetComponent<Camera>().enabled = false;
        }

        void OnDestroy()
        {
            if(energia != null)
                energia.EnergiaAcabou -= DesligarCamera;
        }

    }
}