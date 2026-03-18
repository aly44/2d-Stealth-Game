using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(EnemyController))]
public class EnemyAnimator : MonoBehaviour
{
    // animations
    [SerializeField] private AnimationClip idleDown;
    [SerializeField] private AnimationClip idleUp;
    [SerializeField] private AnimationClip idleLeft;
    [SerializeField] private AnimationClip idleRight;
    [SerializeField] private AnimationClip runDown;
    [SerializeField] private AnimationClip runUp;
    [SerializeField] private AnimationClip runLeft;
    [SerializeField] private AnimationClip runRight;

    private Animator animator;
    private EnemyController enemyController;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        enemyController = GetComponent<EnemyController>();
    }

    private void Update()
    {
        Vector2 velocity = enemyController.velocity;
        bool isMoving = velocity.magnitude > 0.1f;

        // use where its going if moving, otherwise use aim direction
        Vector2 facing;
        if (isMoving)
        {
            facing = velocity;
        }
        else
        {
            facing = enemyController.GetAimDirection();
        }

        AnimationClip clip = GetClip(facing, isMoving);
        Play(clip);
    }

    // direction switch
    private AnimationClip GetClip(Vector2 facing, bool running)
    {
        if (Mathf.Abs(facing.x) >= Mathf.Abs(facing.y))
        {
            if (facing.x >= 0)
            {
                if (running)
                {
                    return runRight;
                }
                return idleRight;
            }
            else
            {
                if (running)
                {
                    return runLeft;
                }
                return idleLeft;
            }
        }
        else
        {
            if (facing.y >= 0)
            {
                if (running)
                {
                    return runUp;
                }
                return idleUp;
            }
            else
            {
                if (running)
                {
                    return runDown;
                }
                return idleDown;
            }
        }
    }
    // animation switch
    private void Play(AnimationClip clip)
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName(clip.name))
        {
            return;
        }
        animator.Play(clip.name, 0);
    }
}
