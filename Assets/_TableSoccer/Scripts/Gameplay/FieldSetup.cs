using UnityEngine;

namespace YRA {
    public class FieldSetup : Singleton<FieldSetup>
    {
        public enum PlaneSide
        {
            Player,
            Enemy
        }
        public TeamRole fieldSideRole;

        [Header("Field Settings")]
        [SerializeField] private bool showDivisionLine = false;
        [SerializeField] private float divisionY = 0f;
        [SerializeField] private float divisionZ = 0f;
        [SerializeField] private GameObject fieldGO;
        [SerializeField] private float fieldScaleZ = 4f;
        [SerializeField] private float fieldScaleX = 2f;
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
        [SerializeField] Bounds _playerField;
        [SerializeField] Bounds _enemyField;
        private LineRenderer lineRenderer;
        [SerializeField] private Color divisionLineColor = Color.red;
        [SerializeField] private float lineWidth = 0.1f;
        [SerializeField] private float lineLength = 100f;
        float multiplier;
        void Start()
        {
           // Create line renderer if visualization is enabled
            if (showDivisionLine)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
                lineRenderer.startWidth = lineWidth;
                lineRenderer.endWidth = lineWidth;
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                lineRenderer.startColor = divisionLineColor;
                lineRenderer.endColor = divisionLineColor;
                lineRenderer.positionCount = 2;
                
                // Set the line positions (horizontal line at divisionY)
                lineRenderer.SetPosition(0, new Vector3(-lineLength / 2, divisionY, 0));
                lineRenderer.SetPosition(1, new Vector3(lineLength / 2, divisionY, 0));
            }    
        }
        
        public void SetupField()
        {
            if (_ball == null) _ball = Ball.Instance;
            if (_gameManager == null) _gameManager = GameManager.Instance;
            if (_playerTeamController == null) _playerTeamController = _gameManager._teamPlayer;
            if (_enemyTeamController == null) _enemyTeamController  = _gameManager._teamEnemy;
            CreateField();
            SetupBall();
            SetupTeams();
            
            // Connect everything to the game manager
            if (_gameManager != null)
            {
                _gameManager._teamPlayer  = _playerTeamController;
                _gameManager._teamEnemy   = _enemyTeamController;
            }
        }

        private void CreateField(){
            // fieldGO.transform.localScale = new Vector3(fieldScaleX, 1, fieldScaleZ);
            bounds = fieldGO.GetComponent<Collider>().bounds;
            float fieldOffset = _ball.ballRadius*2;
            fieldLength = bounds.extents.z - fieldOffset;
            fieldWidth  = bounds.extents.x  - fieldOffset;
        }

        public PlaneSide GetSideForPoint(Vector3 point)
        {
            // Vector2 x = new Vector2(-fieldWidth, fieldWidth);
            // Vector2 z = new Vector2(bounds.center.z, fieldWidth);
            // return point.z > bounds.center.z && point.z < fieldLength ? PlaneSide.Player : PlaneSide.Enemy;
            return point.z < divisionZ ? PlaneSide.Player : PlaneSide.Enemy;
        }

        private void SetupTeams(){

        }
        
        private void SetupBall(){
            float offsetX = Random.Range(-fieldWidth, fieldWidth);
            float offsetY = Random.Range(-bounds.extents.y, bounds.extents.y);
            float multiplier =_playerTeamController.currentRole == TeamRole.Attacking? -1 : 1; 
            multiplier = FindAnyObjectByType<ARSupportChecker>().isARAvailable()? multiplier/100 : multiplier;
            float offsetZ = Random.Range(0, multiplier * fieldLength);
            _ball.transform.position = bounds.center + new Vector3(offsetX, 0, offsetZ);
            // _ball.transform.position = new Vector3(_ball.transform.position.x, 0, _ball.transform.position.z);
        }
    }
}
