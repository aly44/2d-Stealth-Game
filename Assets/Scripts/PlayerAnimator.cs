using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private AnimationClip idleDown;
    [SerializeField] private AnimationClip idleUp;
    [SerializeField] private AnimationClip idleLeft;
    [SerializeField] private AnimationClip idleRight;
    [SerializeField] private AnimationClip runDown;
    [SerializeField] private AnimationClip runUp;
    [SerializeField] private AnimationClip runLeft;
    [SerializeField] private AnimationClip runRight;

    private Animator animator;
    private AnimationClip lastIdle; // remembers which way the player was facing so idle matches

    private void Awake()
    {
        animator = GetComponent<Animator>();
        lastIdle = idleDown;
    }

    private void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        if (x > 0)
        {
            Play(runRight);
            lastIdle = idleRight;
        }
        else if (x < 0)
        {
            Play(runLeft);
            lastIdle = idleLeft;
        }
        else if (y > 0)
        {
            Play(runUp);
            lastIdle = idleUp;
        }
        else if (y < 0)
        {
            Play(runDown);
            lastIdle = idleDown;
        }
        else
        {
            Play(lastIdle);
        }
    }

    // don't restart the clip if its already playing
    private void Play(AnimationClip clip)
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName(clip.name))
        {
            return;
        }
        animator.Play(clip.name, 0);
    }
}
