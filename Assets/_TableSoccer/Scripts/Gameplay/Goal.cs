using UnityEngine;


namespace YRA {
    public class Goal : MonoBehaviour
    {
        public bool isPlayerGoal;
        [SerializeField] ParticleSystem _goalEffect;

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out Ball ball))
            {
                // Goal was scored
                if (_goalEffect != null)
                    _goalEffect.Play();

                // Notify the game manager
                GameManager.Instance.ScorePoint(ball.curHolder.teamController);
            }
        }
    }
}
