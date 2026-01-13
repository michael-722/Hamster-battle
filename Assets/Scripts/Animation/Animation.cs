using UnityEngine;

public class Animation : MonoBehaviour
{
    

    private Animator animator;


    void Start()
    {
        animator = GetComponent<Animator>();
        //TriggerWinAnimation();
    }

    public void TriggerWinAnimation()
    {
        int anim = Random.Range(0, 3);

        switch (anim)
        {
            case 0:
                animator.SetBool("Finger", true);
                break;
            case 1:
                animator.SetBool("Cheer", true);
                return;
            case 2:
                animator.SetBool("Flex", true);
                return;
        }
    }

    public void TriggerLoseAnimation()
    {
        int anim = Random.Range(0, 2);

        switch (anim)
        {
            case 0:
                animator.SetBool("Facepalm", true);
                break;
            case 1:
                animator.SetBool("Cry", true);
                return;
        }
    }
}
