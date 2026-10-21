using System.Collections;
using UnityEngine;

namespace MovingIn.Minigames.Kitchen{
    public class Movement : MonoBehaviour
    {
        [Tooltip("Quanto tempo o movimento leva, em segundos")]
        [SerializeField] float _duration = 0.15f;

        [Tooltip("Formato do movimento: acelera no comeco e desacelera no fim")]
        [SerializeField] AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        Coroutine _routine;

        public void MoveTo(Vector3 destination)
        {
            Stop();
            _routine = StartCoroutine(MoveRoutine(destination));
        }
 
        public void Stop()
        {
            if (_routine != null) StopCoroutine(_routine);
            _routine = null;
        }

        IEnumerator MoveRoutine(Vector3 destination)
        {
            Vector3 start = transform.position;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / _duration;
                transform.position = Vector3.Lerp(start, destination, curve.Evaluate(t));
                yield return null;   
            }
            transform.position = destination;   
            _routine = null;
        }
    }
}
