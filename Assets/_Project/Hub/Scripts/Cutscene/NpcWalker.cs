using System.Collections;
using UnityEngine;

namespace MovingIn.Hub{
    public class NpcWalker : MonoBehaviour
    {
        [SerializeField] GameObject visual;
        [SerializeField] SpriteRenderer box;
        [SerializeField] PlayerSpriteVisual sprite;
        [SerializeField] Transform[] toHouse;
        [SerializeField] Transform exitPoint;
        [SerializeField] float _speed = 2.2f;
        [SerializeField] float _insideTime = 1.4f;
        [SerializeField] float _loadTime = 0.9f;
        [SerializeField] float _startDelay;

        Coroutine _loop;

        public bool Hidden => visual != null && !visual.activeSelf;

        void LateUpdate()
        {
            if (box == null || sprite == null || !box.gameObject.activeSelf) return;
            box.sortingOrder = sprite.FacingBack ? 0 : 2;
        }

        public void StartLoop()
        {
            StopAll();
            _loop = StartCoroutine(Loop());
        }

        public void StopAll()
        {
            if (_loop != null) StopCoroutine(_loop);
            _loop = null;
        }

        public void Show(bool on)
        {
            if (visual != null) visual.SetActive(on);
        }

        public IEnumerator Leave()
        {
            StopAll();
            if (Hidden)
            {
                transform.position = Flat(toHouse[toHouse.Length - 1].position);
                SetBox(false);
                Show(true);
            }
            for (int i = toHouse.Length - 2; i >= 0; i--)
                yield return Cutscene.Walk(transform, toHouse[i].position, _speed * 1.3f);
            if (exitPoint != null) yield return Cutscene.Walk(transform, exitPoint.position, _speed * 1.3f);
            Show(false);
        }

        public IEnumerator WalkTo(Vector3 point)
        {
            yield return Cutscene.Walk(transform, point, _speed);
        }

        IEnumerator Loop()
        {
            transform.position = Flat(toHouse[0].position);
            Show(true);
            SetBox(true);
            yield return new WaitForSeconds(_startDelay);
            while (true)
            {
                SetBox(true);
                for (int i = 1; i < toHouse.Length; i++)
                    yield return Cutscene.Walk(transform, toHouse[i].position, _speed);
                Show(false);
                yield return new WaitForSeconds(_insideTime);
                Show(true);
                SetBox(false);
                for (int i = toHouse.Length - 2; i >= 0; i--)
                    yield return Cutscene.Walk(transform, toHouse[i].position, _speed);
                Show(false);
                yield return new WaitForSeconds(_loadTime);
                Show(true);
            }
        }

        void SetBox(bool on)
        {
            if (box != null) box.gameObject.SetActive(on);
        }

        Vector3 Flat(Vector3 p)
        {
            p.y = transform.position.y;
            return p;
        }
    }
}
