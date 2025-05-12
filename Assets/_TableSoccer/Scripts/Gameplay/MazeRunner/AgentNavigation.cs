using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace YRA
{
    public class AgentNavigation : MonoBehaviour
    {
        [SerializeField]
        private GameObject _ballAsTarget;
        [SerializeField]
        private GameObject _goalAsTarget;
        private Vector3 _desiredDestination;
        private NavMeshAgent _navAgent;
        private Animator _anim;

        // Start is called before the first frame update
        void Start()
        {
            _anim = GetComponentInChildren<Animator>();
            _navAgent = GetComponent<NavMeshAgent>();
            _ballAsTarget.transform.position = new Vector3(Random.Range(-15, 15), -1, Random.Range(-15, 15));
            StartCoroutine(BeginRoutine());
        }

        IEnumerator BeginRoutine()
        {
            float temp = _navAgent.speed;
            _navAgent.speed = 0;
            _navAgent.destination = _ballAsTarget.transform.position;
            yield return new WaitForSeconds(1.5f);
            _anim.SetTrigger("Running");
            _navAgent.speed = temp;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out Ball ball))
            {            
                _navAgent.destination = _goalAsTarget.transform.position;
                transform.SetParent(transform);
                transform.localPosition = new Vector3 (0,-0.5f,1);
            }
            if (other.TryGetComponent(out Goal goal))
            {
                //DONE
                
            }
        }
    }
}
