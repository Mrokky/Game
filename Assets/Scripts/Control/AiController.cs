using UnityEngine;
using RPG.Combat;
using RPG.Attributes;
using RPG.Movement;
using RPG.Core;
using GameDevTV.Utils;

namespace RPG.Control
{
    public class AiController : MonoBehaviour
    {
        [SerializeField] float chaseDistance = 5f;
        [SerializeField] float suspicionTime = 3f;
        [SerializeField] float agroCooldownTime = 5f;
        [SerializeField] PatrolPath patrolPath;
        [SerializeField] float waypointTolerance = 1f;
        [SerializeField] float waypointDwellTime = 2f;
        [Range(0, 1)][SerializeField] float patrolSpeedFraction = 0.2f;
        [SerializeField] float shoutDistance = 5f;

        Health health;
        Fighter fighter;
        Mover mover;
        GameObject player;

        LazyValue<Vector3> guardPosition;
        float timeSinceLastSawPlayer = Mathf.Infinity;
        float timeSinceAggrevated = Mathf.Infinity;
        float timeSinceArrivedAtWaypoint = Mathf.Infinity;
        int currentWaypointIndex;

        void Awake()
        {
            health = GetComponent<Health>();
            fighter = GetComponent<Fighter>();
            player = GameObject.FindWithTag("Player");
            mover = GetComponent<Mover>();
            guardPosition = new LazyValue<Vector3>(GetGuardPosition);
        }

        void Start()
        {
            guardPosition.ForceInit();
        }

        void Update()
        {
            if (health.IsDead())
            {
                return;
            }

            if (IsAggrevated() && fighter.CanAttack(player))
            {
                AttackBehaviour();
            }
            else if (timeSinceLastSawPlayer < suspicionTime)
            {
                SuspicionBehaviour();
            }
            else
            {
                PatrolBehaviour();
            }

            UpdateTimers();
        }

        public void Aggrevate()
        {
            timeSinceAggrevated = 0;
        }

        private Vector3 GetGuardPosition()
        {
            return transform.position;
        }

        private void UpdateTimers()
        {
            timeSinceAggrevated += Time.deltaTime;
            timeSinceLastSawPlayer += Time.deltaTime;
            timeSinceArrivedAtWaypoint += Time.deltaTime;
        }

        private void AttackBehaviour()
        {
            timeSinceLastSawPlayer = 0;
            fighter.Attack(player);

            AgrrevateNearbyEnemies();
        }

        private void AgrrevateNearbyEnemies()
        {
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, shoutDistance, Vector3.up, 0);
            foreach (RaycastHit hit in hits)
            {
                AiController ai = hit.collider.GetComponent<AiController>();
                if(ai == null) continue;
                ai.Aggrevate();
            }
        }

        private void SuspicionBehaviour()
        {
            GetComponent<ActionScheduler>().CancelCurrentAction();
        }

        private void PatrolBehaviour()
        {
            Vector3 nextPosition = guardPosition.value;

            if(patrolPath != null){
                if(AtWaypoint())
                {
                    timeSinceArrivedAtWaypoint = 0;
                    CycleWaypoint();
                }
                nextPosition = GetCurrentWaypoint();
            }
            
            if(timeSinceArrivedAtWaypoint > waypointDwellTime)
            {
                mover.StartMoveAction(nextPosition, patrolSpeedFraction);
            }
        }

        private bool AtWaypoint()
        {
            float distanceToWaypoint = Vector3.Distance(transform.position, GetCurrentWaypoint());
            return distanceToWaypoint < waypointTolerance;
        }

        private void CycleWaypoint()
        {
            currentWaypointIndex = patrolPath.GetNextIndex(currentWaypointIndex);
        }

        private Vector3 GetCurrentWaypoint()
        {
            return patrolPath.GetWaypoint(currentWaypointIndex);
        }

        private bool IsAggrevated()
        {
            float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);
            return distanceToPlayer < chaseDistance || timeSinceAggrevated < agroCooldownTime;
        }

        //Called by Unity
        private void OnDrawGizmosSelected() 
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, chaseDistance);
        }
    }   
}
