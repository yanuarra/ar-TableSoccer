using System;
using UnityEngine;

namespace YRA {
    public enum BallState{
        idle,
        held
        }

    public class Ball : Singleton<Ball> {

        [Header("Ball settings")]
        public BallState curState;
        public float ballRadius {get; private set;}
        [HideInInspector] 
        public Soldier curHolder { get; private set; }

        [Header("References")]
        Rigidbody _ballRigidbody;
        SphereCollider _ballCollider;
        private Vector3 movementDirection;
        private Vector3 startPosition;
        private Movement _movement;

        private void Start()
        {
            if (_ballRigidbody == null) _ballRigidbody = GetComponent<Rigidbody>();
            if (_ballCollider == null) _ballCollider = GetComponent<SphereCollider>();
            if (_movement == null) _movement = GetComponent<Movement>();
            
            ballRadius = _ballCollider.radius;
        }

        public void SetHolder(Soldier soldier)
        {
            curHolder = soldier;
            _ballRigidbody.isKinematic = true;
            transform.SetParent(soldier.transform);
            // transform.localPosition = Vector3.zero;
            transform.localPosition = new Vector3 (0,-0.5f,1);
            _movement.StopMoving();
        }
        
        public void PassBall(Transform target, float speed)
        {
            // _movement.MoveToTarget(gameObject, target, speed);
            _movement.MoveFollowObject(target.transform, speed);
            // movementDirection = direction.normalized;
            _ballRigidbody.isKinematic = true;
        }

        public void Release()
        {
            // transform.SetParent(curHolder.transform.parent);
            transform.parent = null;
            curHolder = null;
            _ballRigidbody.isKinematic = true;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            // if (other.TryGetComponent(out Goal goal))
                // GameManager.Instance.ScorePoint(curHolder.teamController);
            
           //RESET
        }
    }
}
