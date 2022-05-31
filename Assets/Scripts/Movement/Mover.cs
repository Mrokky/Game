using UnityEngine;
using UnityEngine.AI;
using RPG.Core;
using RPG.Saving;
using System.Collections.Generic;
using RPG.Attributes;

namespace RPG.Movement
{
    public class Mover : MonoBehaviour, IAction, ISaveable
    {
        [SerializeField] Transform target;
        [SerializeField] float maxSpeed = 6;
        [SerializeField] float maxNavPathLength = 40f;

        NavMeshAgent navMeshAgent;
        Health health;

        void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            health = GetComponent<Health>();
        }

        void Update()
        {
            navMeshAgent.enabled = !health.IsDead();
            UpdateAnimator();
        }

        public void StartMoveAction(Vector3 destination, float speedFraction)
        {
            GetComponent<ActionScheduler>().StartAction(this);
            MoveTo(destination, speedFraction);
        }

        public bool CanMoveTo(Vector3 destination)
        {
            NavMeshPath path = new NavMeshPath();
            bool hasPath = NavMesh.CalculatePath(transform.position, destination, NavMesh.AllAreas, path);

            if(!hasPath) return false;

            if(path.status != NavMeshPathStatus.PathComplete) return false;

            if(GetPathLength(path) > maxNavPathLength) return false;

            return true;
        }

        public void MoveTo(Vector3 destination, float speedFraction)
        {
            navMeshAgent.destination = destination;
            navMeshAgent.speed = maxSpeed * Mathf.Clamp01(speedFraction);
            navMeshAgent.isStopped = false;
        }

        public void Cancel()
        {
            navMeshAgent.isStopped = true;
        }

        public object CaptureState()
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["position"] = new SerializableVector3(transform.position);
            data["rotation"] = new SerializableVector3(transform.eulerAngles);
            return data;
        }

        public void RestoreState(object state)
        {
            Dictionary<string, object> data = (Dictionary<string, object>)state;
            navMeshAgent.enabled = false;
            transform.position = ((SerializableVector3)data["position"]).ToVector();
            transform.eulerAngles = ((SerializableVector3)data["rotation"]).ToVector();
            navMeshAgent.enabled = true;
        }

        private void UpdateAnimator()
        {
            Vector3 velocity = navMeshAgent.velocity;   //getting global velocity from NavMeshAgent
            Vector3 localVelocity = transform.InverseTransformDirection(velocity);  //converting from global velocity to local value relative to the character
            float speed = localVelocity.z;
            GetComponent<Animator>().SetFloat("forwardSpeed", speed);   //setting animator's blend value to be equal to our desired speed on the Z axis
        }

        private float GetPathLength(NavMeshPath path)
        {
            float total = 0;

            if(path.corners.Length < 2) return total;

            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                total += Vector3.Distance(path.corners[i], path.corners[i + 1]);
            }
            
            return total;
        }
    }
}
