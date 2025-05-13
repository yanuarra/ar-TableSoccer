using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

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
        private ParticleSystem _goalVFX;

        // Start is called before the first frame update
        void Awake()
        {
            _anim = GetComponentInChildren<Animator>();
            _navAgent = GetComponent<NavMeshAgent>();
            _ballAsTarget.transform.position = new Vector3(Random.Range(-15, 15), -1, Random.Range(-15, 15));
             StartCoroutine(BeginDelayedRoutine());
        }

        IEnumerator BeginDelayedRoutine()
        {
            _navAgent.enabled = true;
            _navAgent.destination = _ballAsTarget.transform.position;
            float temp = _navAgent.speed;
            _navAgent.speed = 0;
            _anim.SetTrigger("Waving");
            yield return new WaitForSeconds(1.5f);
            _navAgent.speed = temp;
            _anim.SetTrigger("Running");
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out Ball ball))
            {            
                _navAgent.destination = _goalAsTarget.transform.position;
                ball.transform.SetParent(transform);
                ball.transform.localPosition = new Vector3 (0,-0.5f,1);
            }
            if (other.TryGetComponent(out Goal goal))
            {
                //DONE
                _goalVFX.gameObject.SetActive(true);
                _goalVFX.Play();
                _navAgent.speed = 0;
            }
        }
    }
}
