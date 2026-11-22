using UnityEngine;

namespace MovingIn.Minigames.Onaf
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private CameraManager cameraManager;

        private void Update()
        {   
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (cameraManager != null)
                {
                    cameraManager.ToggleCamera();
                }
                else
                {
                    Debug.LogError("CameraManager não foi atribuído!");
                }
            }
        }
    }
}