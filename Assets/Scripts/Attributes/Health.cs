using UnityEngine;
using RPG.Saving;
using RPG.Stats;
using RPG.Core;
using GameDevTV.Utils;
using UnityEngine.Events;

namespace RPG.Attributes
{   
    public class Health : MonoBehaviour, ISaveable
    {
        [SerializeField] float regenerationPercentage = 70;
        [SerializeField] UnityEvent<float> takeDamage;
        [SerializeField] UnityEvent onDie;

        BaseStats baseStats;
        LazyValue<float> healthPoints;
        bool isDead;

        void Awake() {
            healthPoints = new LazyValue<float>(GetInitialHealth);
            baseStats = GetComponent<BaseStats>();
        }

        void Start() {
            healthPoints.ForceInit();
        }

        void OnEnable() {
            baseStats.onLevelUp += RegenerateHealth;
        }

        void OnDisable() {
            baseStats.onLevelUp -= RegenerateHealth;
        }

        public bool IsDead(){
            return isDead;
        }
        
        public void TakeDamage(GameObject instagator, float damage){
            healthPoints.value = Mathf.Max(healthPoints.value - damage, 0);

            if(healthPoints.value == 0)
            {
                onDie.Invoke();
                Die();
                AwardExperience(instagator);
            }else{
                takeDamage.Invoke(damage);
            }
        }

        public void Heal(float healthToRestore){
            healthPoints.value = Mathf.Min(healthPoints.value + healthToRestore, GetMaxHealthPoints());
        }

        public float GetHealthPoints(){
            return healthPoints.value;
        }

        public float GetMaxHealthPoints(){
            return baseStats.GetStat(Stat.Health);
        }

        public float GetPercentage(){
            return 100 * GetFraction();
        }

        public float GetFraction(){
            return healthPoints.value / baseStats.GetStat(Stat.Health);
        }

        public void Die()
        {
            isDead = true;
            GetComponent<Animator>().SetTrigger("die");
            GetComponent<ActionScheduler>().CancelCurrentAction();
        }

        public object CaptureState()
        {
            return healthPoints.value;
        }

        public void RestoreState(object state)
        {
            healthPoints.value = (float)state;
            if(healthPoints.value == 0)
            {
                Die();
            }
        }

        private float GetInitialHealth(){
            return baseStats.GetStat(Stat.Health);
        }

        private void AwardExperience(GameObject instagator)
        {
            Experience experience = instagator.GetComponent<Experience>();
            if(experience == null) return;
            experience.GainExperience(baseStats.GetStat(Stat.ExperienceReward));
        }

        private void RegenerateHealth()
        {
            float regenHealthPoints = baseStats.GetStat(Stat.Health) * (regenerationPercentage / 100);
            healthPoints.value = Mathf.Max(healthPoints.value, regenHealthPoints);
        }
    }
}