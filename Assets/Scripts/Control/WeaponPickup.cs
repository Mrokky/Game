using System.Collections;
using RPG.Attributes;
using RPG.Control;
using UnityEngine;

namespace RPG.Combat
{
    public class WeaponPickup : MonoBehaviour, IRaycastable
    {
        [SerializeField] WeaponConfig weapon;
        [SerializeField] float healthToRestore;
        [SerializeField] float respawnTime = 5;
        
        void OnTriggerEnter(Collider other) {
            if(other.CompareTag("Player"))
            {
                Pickup(other.gameObject);
            }
        }

        public bool HandleRaycast(PlayerController callingController)
        {
            if(Input.GetMouseButtonDown(0)){
                Pickup(callingController.gameObject);
            }
            return true;
        }

        public CursorType GetCursorType()
        {
            return CursorType.Pickup;
        }

        private void Pickup(GameObject subject)
        {
            if(weapon != null){
                subject.GetComponent<Fighter>().EquipWeapon(weapon);
            }

            if(healthToRestore > 0){
                subject.GetComponent<Health>().Heal(healthToRestore);
            }

            StartCoroutine(HideForSeconds(respawnTime));
        }

        private IEnumerator HideForSeconds(float seconds){
            ShowPickup(false);
            yield return new WaitForSeconds(seconds);
            ShowPickup(true);
        }

        private void ShowPickup(bool shouldShow){
            GetComponent<Collider>().enabled = shouldShow;
            for(int i = 0; i < transform.childCount; i++){
                transform.GetChild(i).gameObject.SetActive(shouldShow);
            }
        }
    }
}
