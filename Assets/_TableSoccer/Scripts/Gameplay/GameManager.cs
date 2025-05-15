using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEditor;

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
        float _frenzyTime = StaticData.FRENZY_TIME_START;
        float _transitionDelay = 3f;
        
        [Header("UI References")]
        [SerializeField] TextMeshProUGUI _stateText;
        [SerializeField] TextMeshProUGUI _timerText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _matchText;
        [SerializeField] TextMeshProUGUI _playerRoleText;
        [SerializeField] TextMeshProUGUI _enemyRoleText;
            
        [Header("Team References")]
        public TeamController _teamPlayer;
        public TeamController _teamEnemy;
        [SerializeField] Ball _ball;

        [Header("Game Progress")]
        bool _frenzyHasBegun = false;
         [SerializeField] GameState _curState;
        private int _curMatch = 0;
        private float _matchTimer;
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
            StartCoroutine(DelayedStartMatch());
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
            
            if (_matchTimer <= _frenzyTime && !_frenzyHasBegun)
            {
                StartFrenzyTime();
            }
            if (_matchTimer <= 0)
            {
                EndCurrentMatch();
            }
        }
        
        private IEnumerator DelayedStartMatch()
        {
            yield return new WaitForEndOfFrame();
            _curMatch++;
            _matchTimer = _matchDuration;
            CurrentState = GameState.Playing;
            if (_curMatch % 2 == 1)
            {
                SetupAttackDefenseRoles(_teamPlayer, _teamEnemy);
            }
            else
            {
                SetupAttackDefenseRoles(_teamEnemy, _teamPlayer);
            }
            Debug.Log($"Match {_curMatch} started! Player is {CurrentState}");
            FieldSetup.Instance.SetupField();
            UpdateRoleUI();
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
            ResetMatch();
            StartCoroutine(DelayedStartMatch());
        }

        private void StartFrenzyTime()
        {
            _frenzyHasBegun = true;
            SoldierManager.Instance.BeginFrenzy();
            MenuSystem.Instance.ShowFrenzyPanel();
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
                    
            MenuSystem.Instance.ShowWinScreen($"{result}! Thanks for playing");
            Debug.Log("Game Over! " + result);
            if (_playerScore == _enemyScore)
            {
                MenuSystem.Instance.ShowWinScreen($"{result}!\nStarting maze mode. . .");
                DelayedRoutine(delegate {SceneManager.Instance.OpenScene(3);}, 3f);
            }
        }

        IEnumerator DelayedRoutine(Action act, float duration)
        {
            yield return new WaitForSeconds(duration);
            act?.Invoke();

        }

        void ResetMatch()
        {
            SoldierManager.Instance.Reset();
            _teamPlayer.Reset();
            _teamEnemy.Reset();
            Ball.Instance.Release();
        }

        void GetTeamControllers()
        {
            TeamController[] teams = FindObjectsByType<TeamController>(FindObjectsSortMode.None);
            foreach (var team in teams)
            {
                if (team.isPlayerTeam)
                {
                    _teamPlayer = team;
                }  
                else
                {
                    _teamEnemy = team;
                }
            }
        }
        
        public void ScorePoint(TeamController team)
        {
            if (team == _teamPlayer)
            {
                _playerScore++;
            }
            else
            {
                _enemyScore++;
            }
            UpdateScoreUI();
            //RESTART MATCH
            EndCurrentMatch();
        }
            
        private void UpdateScoreUI()
        {
            if (_scoreText != null)
                _scoreText.text = $"{_playerScore} - {_enemyScore}";
        }
        
        private void UpdateRoleUI()
        {
            if (_playerRoleText != null)
                _playerRoleText.text = $"Player 1 - {_teamPlayer.currentRole}";
            if (_enemyRoleText != null)
                _enemyRoleText.text = $"Player 2 - {_teamEnemy.currentRole}";
            if (_matchText != null)
                _matchText.text = $"Match {_curMatch}/{_matchCount}";
        }
    }
}