using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : MonoBehaviour
{
    [SerializeField] float speed = 2f;
    [SerializeField] float attackRange = 3f;
    [SerializeField] float wallDetectDist = 5;
    [SerializeField] float rayYOffset = 3f;
    [SerializeField] LayerMask whatIsInFront;

    float dst;

    HitTarget enemy;
    Rigidbody2D rb;
    [Space]
    [SerializeField] Animator animator;

    public bool isNearCollider = false;
    string currentState;

    private void Awake()
    {
        enemy = GetComponent<HitTarget>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        ChangeAnimationState("Idle");
    }

    private void Update()
    {
        dst = Vector3.Distance(enemy.player.position, transform.position);

        enemy.spriteRenderer.flipX = enemy.player.position.x > transform.position.x ? false : true;

        AttackState(dst);

        IdleState(dst);

        if (isNearCollider && currentState != "SlamAttack")
        {
            ChangeAnimationState("Idle");
        }
    }

    private void FixedUpdate()
    {
        ColliderDetection();

        if (enemy.patrol && !isNearCollider)
        {
            PatrolState(dst);
        }
    }

    void ChangeAnimationState(string newState)
    {
        if (currentState == newState) { return; }

        animator.Play(newState);

        currentState = newState;
    }

    void AttackState(float dst)
    {
        if(dst < attackRange)
        {
            ChangeAnimationState("SlamAttack");

            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
            {
                ChangeAnimationState("Idle");
            }
        }
    }

    void IdleState(float dst)
    {
        if (dst > enemy.aggroThreshold)
        {
            ChangeAnimationState("Idle");
        }
    }

    public void PatrolState(float dst)
    {
        if (dst <= enemy.aggroThreshold && dst >= attackRange && !animator.GetCurrentAnimatorStateInfo(0).IsName("SlamAttack"))
        {
            ChangeAnimationState("Walk");

            Vector2 target = new Vector2(enemy.player.position.x, rb.position.y);
            Vector2 newPos = Vector2.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);
        }
    }

    void ColliderDetection()
    {
        Vector2 upperRay = new Vector2(transform.position.x, transform.position.y + rayYOffset);
        Vector2 lowerRay = new Vector2(transform.position.x, transform.position.y - rayYOffset + .1f);

        isNearCollider = Physics2D.Raycast(upperRay, enemy.spriteRenderer.flipX ? Vector2.left : Vector2.right, wallDetectDist, whatIsInFront)
            || Physics2D.Raycast(lowerRay, enemy.spriteRenderer.flipX ? Vector2.left : Vector2.right, wallDetectDist, whatIsInFront);
    }

    private void OnDrawGizmos()
    {
        Vector2 upperRay = new Vector2(transform.position.x, transform.position.y + rayYOffset);
        Vector2 lowerRay = new Vector2(transform.position.x, transform.position.y - rayYOffset + .1f);

        Gizmos.DrawLine(upperRay, new Vector2(upperRay.x + wallDetectDist, upperRay.y));
        Gizmos.DrawLine(upperRay, new Vector2(upperRay.x - wallDetectDist, upperRay.y));
        Gizmos.DrawLine(lowerRay, new Vector2(lowerRay.x + wallDetectDist, lowerRay.y));
        Gizmos.DrawLine(lowerRay, new Vector2(lowerRay.x - wallDetectDist, lowerRay.y));
    }
}
