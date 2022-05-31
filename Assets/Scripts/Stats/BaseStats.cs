using System;
using GameDevTV.Utils;
using UnityEngine;

namespace RPG.Stats
{
    public class BaseStats : MonoBehaviour
    {
        [Range(1, 99)][SerializeField] int startingLevel = 1;
        [SerializeField] CharacterClass characterClass;
        [SerializeField] Progression progression;
        [SerializeField] GameObject levelUpParticleEffect;
        [SerializeField] bool shouldUseModifiers;

        public event Action onLevelUp;

        LazyValue<int> currentLevel;
        Experience experience;

        void Awake(){
            experience = GetComponent<Experience>();
            currentLevel = new LazyValue<int>(CalculateLevel);
        }

        void Start() {
            currentLevel.ForceInit();
        }

        void OnEnable() {
            if(experience != null){
                experience.onExperienceGained += UpdateLevel;
            }
        }

        void OnDisable() {
            if(experience != null){
                experience.onExperienceGained -= UpdateLevel;
            }
        }

        public float GetStat(Stat stat)
        {
            return (GetBaseStat(stat) + GetAdditiveModifier(stat)) * (1 + GetPercentageModifier(stat)/100);
        }

        public int GetLevel(){
            return currentLevel.value;
        }

        private void UpdateLevel() {
            int newLevel = CalculateLevel();
            if(newLevel > currentLevel.value){
                currentLevel.value = newLevel;
                LevelUpEffect();
            }
        }

        private void LevelUpEffect()
        {
            Instantiate(levelUpParticleEffect, transform);
            onLevelUp();
        }

        private float GetBaseStat(Stat stat)
        {
            return progression.GetStat(stat, characterClass, GetLevel());
        }

        private float GetAdditiveModifier(Stat stat)
        {
            float total = 0;
            if(!shouldUseModifiers) return 0;

            foreach(IModifierProvider provider in GetComponents<IModifierProvider>()){
                foreach(float modifier in provider.GetAdditiveModifiers(stat)){
                    total += modifier;
                }
            }
            return total;
        }

        float GetPercentageModifier(Stat stat)
        {
            float total = 0;
            if(!shouldUseModifiers) return 0;

            foreach(IModifierProvider provider in GetComponents<IModifierProvider>()){
                foreach(float modifier in provider.GetPercentageModifiers(stat)){
                    total += modifier;
                }
            }
            return total;
        }

        int CalculateLevel(){
            if(experience == null) return startingLevel;

            float currentXP = experience.GetPoints();
            int penultimateLevel = progression.GetLevels(Stat.ExperienceToLevelUp, characterClass);

            for (int level = 1; level <= penultimateLevel; level++){
                float XPToLevelUp = progression.GetStat(Stat.ExperienceToLevelUp, characterClass, level);
                if(XPToLevelUp > currentXP){
                    return level;
                }
            }

            return penultimateLevel + 1;
        }
    }    
}
