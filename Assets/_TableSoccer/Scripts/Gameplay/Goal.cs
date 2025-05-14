using System.Collections.Generic;
using UnityEngine;


namespace YRA {
    public class Goal : MonoBehaviour
    {
        public bool isPlayerGoal;
        [SerializeField] ParticleSystem _goalEffect;
        [SerializeField] AudioClip _goalSFX;

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out Ball ball))
            {
                // Goal was scored
                if (_goalEffect != null)
                {
                    _goalEffect.gameObject.SetActive(true);
                    _goalEffect.Play();
                }
                if (SoldierManager.Instance)
                    SoldierManager.Instance.OnGoal();
                // Notify the game manager
                if (GameManager.Instance){
                    GameManager.Instance.ScorePoint(ball.curHolder.teamController);
                    if (_goalSFX != null)
                        AudioHandler.Instance.PlayAudioSfx(_goalSFX);
                }else 
                {
                    //MAZE MODE
                    MenuSystem.Instance.ShowWinScreen("You win! Thanks for playing");
                }
            }
        }
    }
}
