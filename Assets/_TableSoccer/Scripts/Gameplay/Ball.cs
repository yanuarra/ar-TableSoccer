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

        private void Start()
        {
            if (_ballRigidbody == null) _ballRigidbody = GetComponent<Rigidbody>();
            if (_ballCollider == null) _ballCollider = GetComponent<SphereCollider>();
            
            ballRadius = _ballCollider.radius;
        }

        public void SetHolder(Soldier soldier)
        {
            curHolder = soldier;
            _ballRigidbody.isKinematic = true;
            transform.SetParent(soldier.transform);
            transform.localPosition = Vector3.zero;
            // transform.position += soldier.transform.forward;
        }
        
        public void PassBall(Vector3 direction, float speed)
        {
            movementDirection = direction.normalized;
            _ballRigidbody.isKinematic = false;
        }

        public void Release()
        {
            curHolder = null;
            _ballRigidbody.isKinematic = false;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out Goal goal))
                GameManager.Instance.ScoreGoal(goal.isPlayerGoal);
            
            // Check if a free ball should be picked up by an Defender
            if (other.TryGetComponent(out Soldier soldier) && 
                soldier.curSoldierRole == SoldierRole.Attacker && 
                soldier.curSoldierState == SoldierState.Active)
            {
                soldier.HoldBall(this);
            }
            // if (curHolder == null )
            // {
            //     Soldier soldier = other.GetComponent<Soldier>();
            //     if (soldier != null && 
            //     soldier.curSoldierRole == SoldierRole.Defender && 
            //     soldier.IsActive)
            //     {
            //         soldier.HoldBall(this);
            //     }
            // }
        }
    }
}
