using UnityEngine;

namespace YRA
{
    public class Movement : MonoBehaviour
    {
        bool _isMoving = false;
        GameObject _origin;
        [SerializeField] GameObject _moveIndicator;
        Vector3 _direction;
        float _speed;

        void Update()
        {
            if (!_isMoving) return;
            DoMoving();
        }

        public void MoveDirection(GameObject origin, Vector3 direction, float speed)
        {
            _isMoving = true;
            _origin = origin;
            _direction = direction;
            _speed = speed;
        }

        public void StopMoving() {
           _isMoving = false;
            _moveIndicator.SetActive(false);
        }

        void DoMoving()
        {
            _moveIndicator.SetActive(true);
            // Apply movement using rigidbody
            // transform.Translate(direction* speed * Time.deltaTime);
            // Vector3 movement = direction * speed * Time.deltaTime;
            // rb.MovePosition(rb.position + movement);
           _origin.transform.position = Vector3.MoveTowards(
                _origin.transform.position, 
                transform.position + _direction,
                _speed * Time.deltaTime);
            _origin.transform.position = new Vector3(_origin.transform.position.x, 1, _origin.transform.position.z);
            // Rotate towards the movement direction
            Quaternion targetRotation = Quaternion.LookRotation(_direction,Vector3.up);
            _origin.transform.rotation = targetRotation;
            // transform.rotation = Quaternion.Slerp(_origin.transform.rotation, targetRotation, 45f * Time.deltaTime);
            // if (direction != Vector3.zero)
            // {
            //     transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 100f * Time.deltaTime);
            // } 
        }
    }
}
