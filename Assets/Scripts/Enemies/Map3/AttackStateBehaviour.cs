using UnityEngine;

public class AttackStateBehaviour : StateMachineBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private MinibossAIMap3 aiScript;

    // This function is called when the animator enters this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Get the main AI script from the parent object
        if (aiScript == null)
        {
            aiScript = animator.GetComponentInParent<MinibossAIMap3>();
        }

        // Tell the AI that it is attacking
        aiScript.SetAttackingState(true);
    }

    // This function is called when the animator exits this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Tell the AI that the attack animation is finished
        aiScript.SetAttackingState(false);
    }
}
