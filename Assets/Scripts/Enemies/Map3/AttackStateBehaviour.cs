using UnityEngine;

public class AttackStateBehaviour : StateMachineBehaviour
{
    private IStatefulEnemy statefulEnemy;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (statefulEnemy == null)
        {
            statefulEnemy = animator.GetComponentInParent<IStatefulEnemy>();
        }

        if (statefulEnemy != null)
        {
            statefulEnemy.SetAttackingState(true);
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (statefulEnemy != null)
        {
            statefulEnemy.SetAttackingState(false);
        }
    }
}
