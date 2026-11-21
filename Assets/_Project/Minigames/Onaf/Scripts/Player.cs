using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MovingIn.Minigames.Onaf{
    public class Player : MonoBehaviour
    {
        // Start is called before the first frame update

        [SerializeField] EnergyManager _energyManager;
        [SerializeField] Camera _cameraPlayer;
        [SerializeField] Camera _secCamera; //Inscrito no inspetor
        
        private bool onCamera;

        void Start()
        {
            _cameraPlayer = transform.GetChild(0).gameObject.GetComponent<Camera>();
            onCamera = false;
            _cameraPlayer.enabled = true;
            _secCamera.enabled = false;
        }

        // Update is called once per frame
        void Update()
        {
            if(_energyManager.TemEnergia() && !onCamera && Input.GetKeyDown(KeyCode.Space))
            {
                _secCamera.enabled = true;
                _cameraPlayer.enabled = false;
                onCamera = true;
            }
            else if(onCamera && Input.GetKeyDown(KeyCode.Space))
            {
                _cameraPlayer.enabled = true;
                _secCamera.enabled = false;
                onCamera = false;
            }

        }
    }
}