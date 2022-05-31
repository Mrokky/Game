using UnityEngine;
using RPG.Movement;
using RPG.Core;
using RPG.Saving;
using RPG.Attributes;
using RPG.Stats;
using System.Collections.Generic;
using GameDevTV.Utils;
using System;

namespace RPG.Combat
{
    public class Fighter : MonoBehaviour, IAction, ISaveable, IModifierProvider
    {
        [SerializeField] float timeBetweenAttacks = 1.2f;
        [SerializeField] Transform rightHandTransform;
        [SerializeField] Transform leftHandTransform;
        [SerializeField] WeaponConfig defaultWeapon;

        Mover mover;
        Animator animator;
        Health target;
        WeaponConfig currentWeaponConfig;
        LazyValue<Weapon> currentWeapon;

        float timeSinceLastAttack = Mathf.Infinity;

        void Awake() {
            currentWeaponConfig = defaultWeapon;
            currentWeapon = new LazyValue<Weapon>(SetupDefaultWeapon);
            animator = GetComponent<Animator>();
            mover = GetComponent<Mover>();
        }

        void Start(){
            currentWeapon.ForceInit();
        }

        void Update()
        {
            timeSinceLastAttack += Time.deltaTime;

            if(target == null){
                return;
            }

            if(target.IsDead()){
                return;
            }

            if (!GetIsInRange(target.transform)){
                mover.MoveTo(target.transform.position, 1f);
            }else{
                mover.Cancel();
                AttackBehaviour();
            }
        }

        public void EquipWeapon(WeaponConfig weapon)
        {
            if (weapon == null) return;
            currentWeaponConfig = weapon;
            currentWeapon.value = AttachWeapon(weapon);
        }

        public Health GetTarget(){
            return target;
        }

        public bool CanAttack(GameObject combatTarget){
            if(combatTarget == null) return false;

            if(!mover.CanMoveTo(combatTarget.transform.position) && !GetIsInRange(combatTarget.transform)) return false;

            Health targetToTest = combatTarget.GetComponent<Health>();
            return targetToTest != null && !targetToTest.IsDead();
        }

        public void Attack(GameObject combatTarget){
            GetComponent<ActionScheduler>().StartAction(this);
            target = combatTarget.GetComponent<Health>();
        }

        public void Cancel()
        {
            StopAttack();
            target = null;
            mover.Cancel();
        }

        public IEnumerable<float> GetAdditiveModifiers(Stat stat)
        {
            if(stat == Stat.Damage){
                yield return currentWeaponConfig.GetWeaponDamage(); 
            }
        }

        public IEnumerable<float> GetPercentageModifiers(Stat stat)
        {
            if(stat == Stat.Damage){
                yield return currentWeaponConfig.GetPercentageBonus(); 
            }
        }

        public object CaptureState()
        {
            return currentWeaponConfig.name;
        }

        public void RestoreState(object state)
        {
            string weaponName = (string)state;
            //Look for the resource in the Resources folder that has the type of weapon 
            WeaponConfig weapon = Resources.Load<WeaponConfig>(weaponName);
            EquipWeapon(weapon);
        }

        private Weapon SetupDefaultWeapon()
        {
            return AttachWeapon(defaultWeapon);
        }

        private Weapon AttachWeapon(WeaponConfig weapon)
        {
            return weapon.Spawn(rightHandTransform, leftHandTransform, animator);
        }

        private void AttackBehaviour(){
            transform.LookAt(target.transform);
            if(timeSinceLastAttack > timeBetweenAttacks)
            {
                //This will trigger the Hit() event
                TriggerAttack();
                timeSinceLastAttack = 0;
            }
        }

        private void TriggerAttack()
        {
            animator.ResetTrigger("stopAttack");
            animator.SetTrigger("attack");
        }

        private void StopAttack()
        {
            animator.ResetTrigger("attack");
            animator.SetTrigger("stopAttack");
        }

        //Animation Event
        private void Hit(){
            if(target == null){
                return;
            }
            
            float damage = GetComponent<BaseStats>().GetStat(Stat.Damage);

            if(currentWeapon.value != null){
                currentWeapon.value.OnHit();
            }

            if(currentWeaponConfig.HasProjectile() == true){
                currentWeaponConfig.LaunchProjectile(rightHandTransform, leftHandTransform, target, gameObject, damage);
            }else{
                target.TakeDamage(gameObject, damage);
            }
        }

        private void Shoot(){
            Hit();
        }

        private bool GetIsInRange(Transform targetTransform){
            return Vector3.Distance(transform.position, targetTransform.position) < currentWeaponConfig.GetWeaponRange();
        }
    }
}
