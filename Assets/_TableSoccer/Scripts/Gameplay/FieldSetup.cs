using UnityEngine;

namespace YRA {
    public class FieldSetup : Singleton<FieldSetup>
    {
        [Header("Field Settings")]
        [SerializeField] GameObject fieldGO;
        [SerializeField] float fieldScaleZ = 4f;
        [SerializeField] float fieldScaleX = 2f;
        public float fieldLength { get; private set; }
        public float fieldWidth { get; private set; }
        [SerializeField] Material fieldMaterial;
        [SerializeField] Material lineMaterial;
        Bounds bounds;

        [Header("References")]
        [SerializeField] GameManager _gameManager;
        TeamController _playerTeamController;
        TeamController _enemyTeamController;
        [SerializeField] Ball _ball;

        void Start()
        {
            // if (_ball == null) _ball = Ball.Instance;
            // if (_gameManager == null) _gameManager = GameManager.Instance;
            // if (_playerTeamController == null) _playerTeamController = _gameManager.playerTeam;
            // if (_enemyTeamController == null) _enemyTeamController  = _gameManager.enemyTeam;
        }

        public void SetupField()
        {
            if (_ball == null) _ball = Ball.Instance;
            if (_gameManager == null) _gameManager = GameManager.Instance;
            if (_playerTeamController == null) _playerTeamController = _gameManager.playerTeam;
            if (_enemyTeamController == null) _enemyTeamController  = _gameManager.enemyTeam;
            CreateField();
            SetupBall();
            SetupTeams();
            
            // Connect everything to the game manager
            if (_gameManager != null)
            {
                _gameManager.playerTeam  = _playerTeamController;
                _gameManager.enemyTeam   = _enemyTeamController;
            }
        }

        private void CreateField(){
            // fieldGO.transform.localScale = new Vector3(fieldScaleX, 1, fieldScaleZ);
            bounds = fieldGO.GetComponent<Collider>().bounds;
            float fieldOffset = _ball.ballRadius;
            fieldLength = bounds.extents.z - fieldOffset;
            fieldWidth  = bounds.extents.x  - fieldOffset;
        }

        private void SetupTeams(){

        }
        
        private void SetupBall(){
            float offsetX = Random.Range(-fieldWidth, fieldWidth);
            float offsetY = Random.Range(-bounds.extents.y, bounds.extents.y);
            float multiplier =_playerTeamController.currentRole == TeamRole.Attacking? -1 : 1; 
            float offsetZ = Random.Range(bounds.center.z, multiplier * fieldLength);
            _ball.transform.position = bounds.center + new Vector3(offsetX, .5f, offsetZ);
            Debug.Log(_ball.transform.position);
        }
    }
}
