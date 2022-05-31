using UnityEngine;

namespace RPG.Core
{
    public class ActionScheduler : MonoBehaviour
    {
        public void CancelCurrentAction()
        {
            StartAction(null);
        }

        IAction currentAction;
        public void StartAction(IAction action)
        {
            //if current action is already running we don't need to do anything
            if(currentAction == action)
            {
                return;
            }

            if(currentAction != null)
            {
                currentAction.Cancel();
            }

            currentAction = action;
        }
    }
}