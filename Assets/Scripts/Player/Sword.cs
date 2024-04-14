using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Sword : MonoBehaviour
{
    [SerializeField] SwordType swordType;
    [SerializeField] LayerMask enemyLayer;

    [Space]
    [SerializeField] float damage = 5;
    [SerializeField, HideInInspector] float pogoJumpForce = 4.5f;

    enum SwordType
    {
        forward,
        up,
        down
    };

    bool dealDamage = true;

    bool side;
    PlayerController playerController;

#if UNITY_EDITOR
    [CustomEditor(typeof(Sword))]
    public class SwordEditor : Editor
    {
        SerializedProperty pogoJumpForce;

        protected virtual void OnEnable()
        {
            setvars();
        }

        void setvars()
        {
            pogoJumpForce = serializedObject.FindProperty("pogoJumpForce");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            Sword sword = (Sword)target;

            if(sword.swordType == SwordType.down)
            {
                EditorGUILayout.PropertyField(pogoJumpForce);
            }
                
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif

    private void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
    }



    bool HitParticle(HitTarget hitTarget, Vector2 direction)
    {
        float dist = 3.5f;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, dist, enemyLayer);

        if(hit.collider != null)
        {
            PlayParticle(hitTarget, hit);

            Debug.DrawLine(new Vector2(transform.position.x - dist, transform.position.y), new Vector2(transform.position.x + dist, transform.position.y), Color.red, 2);
            Debug.DrawLine(new Vector2(transform.position.x, transform.position.y - dist), new Vector2(transform.position.x, transform.position.y + dist), Color.red, 2);
        }
        else
        {
            hit = Physics2D.Linecast(transform.position, hitTarget.transform.position, enemyLayer);

            PlayParticle(hitTarget, hit);

            Debug.DrawLine(transform.position, hitTarget.transform.position, Color.red, 2);
        }

        return hit.collider.GetComponent<HitTarget>().enemyType != HitTarget.EnemyType.damageZone;
    }



    void PlayParticle(HitTarget hitTarget, RaycastHit2D hit)
    {
        ParticleSystem ps = hitTarget.hitParticle;

        ps.transform.parent = null;
        ps.transform.localScale = Vector3.one;
        ps.transform.position = hit.point;

        if (hitTarget.enemyType == HitTarget.EnemyType.destructibleObject)
        {
            var main = ps.main;
            main.stopAction = ParticleSystemStopAction.Destroy;
        }

        ps.Play();
    }



    bool HitMechanic(HitTarget hitTarget, Vector2 particleDir, Vector2 knockBackDir)
    {
        StartCoroutine(playerController.KnockBackMechanic(knockBackDir));

        return HitParticle(hitTarget, particleDir);
    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            side = playerController.GetComponent<SpriteRenderer>().flipX;

            SwordManager swordManager = transform.parent.GetComponent<SwordManager>();

            HitTarget hitTarget = collision.GetComponent<HitTarget>();

            switch (swordType)
            {
                case SwordType.forward:

                    dealDamage = HitMechanic(hitTarget, !side ? Vector2.right : Vector2.left, (!side ? Vector2.left : Vector2.right) * playerController.knockBackSpeed);

                    break;

                case SwordType.down:

                    dealDamage = HitMechanic(hitTarget, Vector2.down, Vector2.up * pogoJumpForce);
                    playerController.AbilitiesReset(5);

                    break;

                case SwordType.up:

                    dealDamage = HitMechanic(hitTarget, Vector2.up, Vector2.down);

                    break;
            }

            if (!hitTarget.CanMove)
            {
                return;
            }

            if(dealDamage) hitTarget.TakeDamage(damage);

            swordManager.manaIndex++;
            if (swordManager.manaIndex == swordManager.manaRefillMax)
            {
                swordManager.manaIndex = 0;
                playerController.GainMana(1);
            }
        }
    }
}
