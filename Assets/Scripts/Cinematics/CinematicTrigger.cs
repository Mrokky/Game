using UnityEngine;
using UnityEngine.Playables;

namespace RPG.Cinematics
{
    public class CinematicTrigger : MonoBehaviour
    {
        bool wasTriggered;

        void OnTriggerEnter(Collider other) 
        {
            if(!wasTriggered && other.CompareTag("Player")){
                GetComponent<PlayableDirector>().Play();
                wasTriggered = true;
            }
        }

    }
}
