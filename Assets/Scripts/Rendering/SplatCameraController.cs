using UnityEngine;

namespace TeleportNative.Rendering
{
    /// <summary>
    /// Camera do viewer: orbita (1 dedo/mouse), pinch/wheel zoom, e caminhada por joystick
    /// virtual (UI passa vetor). Navegacao livre como no Teleport. Usa Input antigo (defina
    /// Active Input Handling = Both p/ funcionar tambem com o Input System) — ver BUILD.md.
    /// </summary>
    public sealed class SplatCameraController : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private float _rotateSpeed = 0.35f;
        [SerializeField] private float _moveSpeed = 1.6f;
        [SerializeField] private float _minDist = 0.6f;
        [SerializeField] private float _maxDist = 24f;
        [SerializeField] private float _minPitch = -25f;
        [SerializeField] private float _maxPitch = 80f;

        private float _yaw, _pitch = 18f, _dist = 4f;

        private void LateUpdate()
        {
            HandleInput();
            ApplyTransform();
        }

        public void SetTarget(Transform t) { _target = t; }

        public void Orbit(Vector2 delta)
        {
            _yaw -= delta.x * _rotateSpeed;
            _pitch = Mathf.Clamp(_pitch + delta.y * _rotateSpeed, _minPitch, _maxPitch);
        }

        public void Zoom(float delta) => _dist = Mathf.Clamp(_dist + delta, _minDist, _maxDist);

        /// <summary>Caminhada no plano (joystick). move em coords de mundo projetadas no chao.</summary>
        public void Walk(Vector2 stick01)
        {
            if (_target == null) return;
            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
            _target.position += (forward * stick01.y + right * stick01.x) * _moveSpeed * Time.deltaTime;
        }

        public void ResetView()
        {
            _yaw = 0; _pitch = 18f; _dist = 4f;
            if (_target != null) _target.position = Vector3.zero;
        }

        private void HandleInput()
        {
            if (Input.touchCount == 1)
            {
                var t = Input.GetTouch(0);
                if (t.phase == TouchPhase.Moved) Orbit(t.deltaPosition);
            }
            else if (Input.touchCount == 2)
            {
                var a = Input.GetTouch(0); var b = Input.GetTouch(1);
                var prev = Vector2.Distance(a.position - a.deltaPosition, b.position - b.deltaPosition);
                var now = Vector2.Distance(a.position, b.position);
                Zoom((prev - now) * 0.01f);
            }

            if (Input.GetMouseButton(0)) Orbit(new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")));
            if (Mathf.Abs(Input.mouseScrollDelta.y) > 0.001f) Zoom(-Input.mouseScrollDelta.y * 0.5f);
        }

        private void ApplyTransform()
        {
            var t = _target != null ? _target : transform;
            var rot = Quaternion.Euler(_pitch, _yaw, 0f);
            transform.position = t.position + rot * (Vector3.back * _dist);
            transform.LookAt(t);
        }
    }
}
