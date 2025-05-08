using UnityEngine;

namespace YRA
{
    public class Movement : MonoBehaviour
    {
        bool _isMoving = false;
        GameObject _origin;
        Vector3 _direction;
        float _speed;
        public void StopMoving() => _isMoving = false;

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

        void DoMoving()
        {
            // Apply movement using rigidbody
            // transform.Translate(direction* speed * Time.deltaTime);
            // Vector3 movement = direction * speed * Time.deltaTime;
            // rb.MovePosition(rb.position + movement);
           _origin.transform.position = Vector3.MoveTowards(
                _origin.transform.position, 
                transform.position + _direction,
                _speed * Time.deltaTime);
            // Rotate towards the movement direction
            Quaternion targetRotation = Quaternion.LookRotation(_direction);
            _origin.transform.rotation = targetRotation;
            // transform.rotation = Quaternion.Slerp(_origin.transform.rotation, targetRotation, 45f * Time.deltaTime);
            // if (direction != Vector3.zero)
            // {
            //     transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 100f * Time.deltaTime);
            // } 
        }
    }
}
