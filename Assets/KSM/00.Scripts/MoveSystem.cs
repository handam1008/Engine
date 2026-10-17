
using UnityEngine;


    public class MoveSystem : MonoBehaviour
    {
        public float Dir { get; private set; }
        private float _speed;
        public Rigidbody2D RbCompo{get; private set;}

        private void Awake()
        {
            RbCompo = GetComponent<Rigidbody2D>();
        }

        public void InitMove( float speed, float dir)
        {
            _speed = speed;
            Dir = dir;
        }

        private void FixedUpdate()
        {
            RbCompo.linearVelocityX = Dir * _speed;
        }
    }