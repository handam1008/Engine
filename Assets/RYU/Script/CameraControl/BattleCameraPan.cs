using UnityEngine;
using UnityEngine.InputSystem;

namespace RYU.Script.CameraControl
{
    //전장 카메라를 좌우로만 움직인다. A/D·방향키로 이동, 마우스 휠 한 칸마다 _wheelStep만큼 이동
    //화면이 전장 양 끝(_leftEdge, _rightEdge) 밖을 비추지 않게 막는다
    [RequireComponent(typeof(Camera))]
    public class BattleCameraPan : MonoBehaviour
    {
        [SerializeField] private float _leftEdge = -12.4444f;
        [SerializeField] private float _rightEdge = 37.3333f;
        [SerializeField] private float _keySpeed = 15f;
        [SerializeField] private float _wheelStep = 2f;

        private Camera _camera;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            float move = 0f;

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) move -= _keySpeed * Time.unscaledDeltaTime;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) move += _keySpeed * Time.unscaledDeltaTime;
            }

            Mouse mouse = Mouse.current;
            if (mouse != null)
            {
                float wheel = mouse.scroll.ReadValue().y;
                if (wheel != 0f) move -= Mathf.Sign(wheel) * _wheelStep;
            }

            float halfWidth = _camera.orthographicSize * _camera.aspect;
            float minX = _leftEdge + halfWidth;
            float maxX = Mathf.Max(minX, _rightEdge - halfWidth);

            Vector3 pos = transform.position;
            pos.x = Mathf.Clamp(pos.x + move, minX, maxX);
            transform.position = pos;
        }
    }
}
