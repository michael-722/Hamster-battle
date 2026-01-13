using UnityEngine;

public class Animation : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private GameObject hamster;

    void Start()
    {
        animator = GetComponent<Animator>();
        hamster.SetActive(false);
    }

    public void TriggerWinAnimation()
    {
        hamster.SetActive(true);
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
        hamster.SetActive(true);
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

    public void ResetAnimations()
    {
        animator.SetBool("Finger", false);
        animator.SetBool("Cheer", false);
        animator.SetBool("Flex", false);
        animator.SetBool("Facepalm", false);
        animator.SetBool("Cry", false);
        hamster.SetActive(false);
    }   
}
