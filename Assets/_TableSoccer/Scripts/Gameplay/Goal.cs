using UnityEngine;


namespace YRA {
    public class Goal : MonoBehaviour
    {
        public static Transform goalPost;
        public bool isPlayerGoal;
        [SerializeField] ParticleSystem _goalEffect;
        
        void Start()
        {
            goalPost = this.transform;
        }


        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out Ball ball))
            {
                // Goal was scored
                if (_goalEffect != null)
                {
                    _goalEffect.Play();
                }
                // Notify the game manager
                GameManager.Instance.ScoreGoal(ball.curHolder.isPlayerTeam);
            }
        }
    }
}
