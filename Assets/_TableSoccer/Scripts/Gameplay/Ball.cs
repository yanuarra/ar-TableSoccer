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
        [HideInInspector] public Soldier CurHolder { get; private set; }

        [Header("References")]
        Rigidbody _ballRigidbody;
        SphereCollider _ballCollider;

        private void Start()
        {
            if (_ballRigidbody == null) _ballRigidbody = GetComponent<Rigidbody>();
            if (_ballCollider == null) _ballCollider = GetComponent<SphereCollider>();
            
            ballRadius = _ballCollider.radius;
        }

        public void ChangeHolder()
        {

        }
    }
}
