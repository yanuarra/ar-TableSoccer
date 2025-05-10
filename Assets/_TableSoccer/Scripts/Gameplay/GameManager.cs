using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace YRA {
    public enum GameState
    {
        Idle,
        Playing,
        MatchTransition,
        GameOver,
        Penalty
    }

    public class GameManager : Singleton<GameManager>
    {
        [Header("Game Settings")]
        int _matchCount = StaticData.MATCH_COUNT;
        float _matchDuration = StaticData.MATCH_TIME_LIMIT;
        float _transitionDelay = 3f;
        
        [Header("UI References")]
        [SerializeField] TextMeshProUGUI _stateText;
        [SerializeField] TextMeshProUGUI _timerText;
        [SerializeField] TextMeshProUGUI _scoreText;
            
        [Header("Team References")]
        public TeamController playerTeam;
        public TeamController enemyTeam;
        [SerializeField] Ball _ball;
        
        [Header("Game Progress")]
        [SerializeField] GameState _curState;
        int _curMatch = 0;
        float _matchTimer;
        private int _playerScore = 0;
        private int _enemyScore = 0;

        public GameState CurrentState 
        { 
            get { return _curState; }
            private set 
            {
                _curState = value;
                if (_stateText != null)
                    _stateText.text = "State: " + _curState.ToString();
            }
        }

        void Start()
        {
            if (_ball == null) _ball = Ball.Instance;
            _curState = GameState.Idle;
            GetTeamControllers();
            UpdateScoreUI();
            StartMatch();
        }

        private void Update()
        {
            if (CurrentState == GameState.Playing) UpdateMatchTimer();
        }

        private void UpdateMatchTimer()
        {
            _matchTimer -= Time.deltaTime;
            
            if (_timerText != null)
                _timerText.text = "" + Mathf.CeilToInt(_matchTimer).ToString();
            
            if (_matchTimer <= 0)
                EndCurrentMatch();
        }
        
        private void StartMatch()
        {
            _curMatch++;
            _matchTimer = _matchDuration;
            CurrentState = GameState.Playing;
            FieldSetup.Instance.SetupField();
            if (_curMatch % 2 == 1)
            {
                SetupAttackDefenseRoles(playerTeam, enemyTeam);
            }
            else
            {
                SetupAttackDefenseRoles(enemyTeam, playerTeam);
            }
            
            Debug.Log($"Match {_curMatch} started! Player is {CurrentState}");
            ResetMatch();
        }

        void SetupAttackDefenseRoles(TeamController attackingTeam, TeamController defendingTeam)
        {
            Debug.Log($"attack {attackingTeam} def {defendingTeam}");
            attackingTeam.SetTeamRole(TeamRole.Attacking);
            defendingTeam.SetTeamRole(TeamRole.Defending);
        }
        
        private IEnumerator TransitionToNextMatch()
        {
            if (_stateText != null)
                _stateText.text = "Preparing next match...";
            
            yield return new WaitForSeconds(_transitionDelay);
            StartMatch();
        }

        private void EndCurrentMatch()
        {
            CurrentState = GameState.MatchTransition;
            if (_curMatch < _matchCount)
            {
                StartCoroutine(TransitionToNextMatch());
            }
            else
            {
                EndGame();
            }
        }

        private void EndGame()
        {
            CurrentState = GameState.GameOver;
            
            if (_stateText != null)
                _stateText.text = "Game Over!";
                
            string result = _playerScore > _enemyScore ? "Player Wins!" : 
                            _playerScore < _enemyScore ? "Enemy Wins!" : "It's a Draw!";
                            
            Debug.Log("Game Over! " + result);
        }

        void ResetMatch()
        {
            playerTeam.Reset();
            enemyTeam.Reset();
        }

        void GetTeamControllers()
        {
            TeamController[] teams = FindObjectsByType<TeamController>(FindObjectsSortMode.None);
            foreach (var team in teams)
            {
                if (team.isPlayerTeam)
                {
                    playerTeam = team;
                }  
                else
                {
                    enemyTeam = team;
                }
            }
        }
        
        public void ScoreGoal(bool isPlayer)
        {
            if (isPlayer)
            {
                _playerScore++;
            }
            else
            {
                _enemyScore++;
            }
            UpdateScoreUI();
        }
            
        private void UpdateScoreUI()
        {
            if (_scoreText != null)
                _scoreText.text = $"{_playerScore} - {_enemyScore}";
        }
    }
}