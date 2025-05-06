using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace YRA {
    public enum GameState
    {
        Idle,
        PlayerAttacking,
        PlayerDefending,
        MatchTransition,
        GameOver
    }

    public class GameManager : Singleton<GameManager>
    {
        [Header("Game Settings")]
        [SerializeField] int _matchCount = StaticData.MATCH_COUNT;
        [SerializeField] float _matchDuration = StaticData.MATCH_TIME_LIMIT;
        [SerializeField] float _transitionDelay = 3f;
        
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
            GetTeamControllers();
            _curState = GameState.Idle;
            playerTeam.SetTeamRole(TeamRole.Attacking);
            enemyTeam.SetTeamRole(TeamRole.Defending);
        }

        void GetTeamControllers()
        {
            TeamController[] teams = FindObjectsByType<TeamController>(FindObjectsSortMode.None);
            foreach (var team in teams)
            {
                enemyTeam = team;
                if (team.isPlayerTeam) playerTeam = team; 
            }
        
        }
    }
}