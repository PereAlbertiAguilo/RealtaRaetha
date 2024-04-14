using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pathfinding;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class HitTarget : MonoBehaviour
{
    [HideInInspector] public enum EnemyType
    {
        damageZone,
        normalEnemy,
        bossEnemy,
        destructibleObject
    };

    public EnemyType enemyType = EnemyType.normalEnemy;

    [SerializeField, HideInInspector] float maxHealth;
    [SerializeField, HideInInspector] int damage;
    [SerializeField, HideInInspector] int coinsAmountToDrop = 3;
    [SerializeField, HideInInspector] GameObject[] coin;
    [SerializeField, HideInInspector] public LayerMask whatIsCollider;
    [SerializeField, HideInInspector] public float aggroThreshold;
    [SerializeField, HideInInspector] bool followForever;
    [SerializeField, HideInInspector] bool isBigEnemy;
    [SerializeField, HideInInspector] public bool patrol = true;

    [SerializeField, HideInInspector] public SpriteRenderer spriteRenderer;
    [SerializeField, HideInInspector] public Rigidbody2D rb;
    AIDestinationSetter aIDestinationSetter;
    [SerializeField, HideInInspector] public AIPath aIPath;
    [SerializeField, HideInInspector] UnityEvent destroyFunction;

    float currentHealth;
    bool follow = false;
    float interpolationFactor;
    [SerializeField, HideInInspector] public Transform player;

    [Space]
    public bool CanMove = true;
    [SerializeField, HideInInspector] bool canDropCoins = false;
    [Space]
    public ParticleSystem hitParticle;

#if UNITY_EDITOR
    [CustomEditor(typeof(HitTarget)), CanEditMultipleObjects]
    public class EnemyEditor : Editor
    {
        SerializedProperty maxHealth, damage, whatIsCollider, aggroThreshold, followForever, isBigEnemy, patrol, coinsAmountToDrop, coin, canDropCoins, destroyFunction;

        protected virtual void OnEnable()
        {
            setvars();
        }

        void setvars()
        {
            maxHealth = serializedObject.FindProperty("maxHealth");
            damage = serializedObject.FindProperty("damage");
            whatIsCollider = serializedObject.FindProperty("whatIsCollider");
            aggroThreshold = serializedObject.FindProperty("aggroThreshold");
            followForever = serializedObject.FindProperty("followForever");
            isBigEnemy = serializedObject.FindProperty("isBigEnemy");
            patrol = serializedObject.FindProperty("patrol");
            coinsAmountToDrop = serializedObject.FindProperty("coinsAmountToDrop");
            coin = serializedObject.FindProperty("coin");
            canDropCoins = serializedObject.FindProperty("canDropCoins");
            destroyFunction = serializedObject.FindProperty("destroyFunction");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            HitTarget enemy = (HitTarget)target;

            switch (enemy.enemyType)
            {
                case EnemyType.damageZone:

                    DamageZone();

                    break;
                case EnemyType.normalEnemy:

                    NormalEnemy();

                    break;
                case EnemyType.bossEnemy:

                    BossEnemy();

                    break;
                case EnemyType.destructibleObject:

                    DestructibleObject();

                    break;
            }
            
            serializedObject.ApplyModifiedProperties();

            void NormalEnemy()
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Dynamic Enemy Properties");
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(maxHealth);
                EditorGUILayout.PropertyField(damage);

                if (enemy.GetComponent<Patrol>() == null)
                {
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(whatIsCollider);
                    EditorGUILayout.PropertyField(aggroThreshold);
                    EditorGUILayout.PropertyField(followForever);
                }

                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(isBigEnemy);

                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(canDropCoins);

                if (canDropCoins.boolValue)
                {
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(coinsAmountToDrop);
                    EditorGUILayout.PropertyField(coin);
                }

                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(destroyFunction);
            }

            void BossEnemy()
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Dynamic Boss Properties");
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(maxHealth);
                EditorGUILayout.PropertyField(damage);
                EditorGUILayout.PropertyField(aggroThreshold);

                EditorGUILayout.Space();

                //EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(isBigEnemy);
                //EditorGUI.EndDisabledGroup();

                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(patrol);

                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(canDropCoins);

                if (canDropCoins.boolValue)
                {
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(coinsAmountToDrop);
                    EditorGUILayout.PropertyField(coin);
                }

                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(destroyFunction);
            }

            void DamageZone()
            {
                enemy.CanMove = false;

                EditorGUILayout.PropertyField(damage);
            }

            void DestructibleObject()
            {
                EditorGUILayout.PropertyField(maxHealth);

                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(canDropCoins);

                if (canDropCoins.boolValue)
                {
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(coinsAmountToDrop);
                    EditorGUILayout.PropertyField(coin);
                }

                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(destroyFunction);
            }
        }
    }
#endif

    private void Awake()
    {
        if (!CanMove)
        {
            return;
        }

        spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        aIDestinationSetter = GetComponent<AIDestinationSetter>();
        aIPath = GetComponent<AIPath>();
    }

    void Start()
    {
        player = FindObjectOfType<PlayerController>().transform;

        if (!CanMove)
        {
            return;
        }

        spriteRenderer.color = new Vector4(1, 1, 1, 0);

        StartCoroutine(Spawn());

        currentHealth = maxHealth;
    }

    public void KnockBackMechanic(Vector2 direction)
    {
        if (isBigEnemy)
        {
            return;
        }

        StartCoroutine(AIPathReset(aIPath));

        rb.velocity = new Vector3(rb.velocity.x, 0f);
        rb.AddForce(direction, ForceMode2D.Impulse);
    }

    IEnumerator AIPathReset(AIPath aIPath)
    {
        aIPath.canMove = false;
        yield return new WaitForSeconds(.2f);
        aIPath.canMove = true;
    }

    public void TakeDamage(float damage)
    {
        if (!CanMove)
        {
            return;
        }

        currentHealth -= damage;
        StartCoroutine(DamageTakenAnim());
        KnockBackMechanic(((transform.position.x < player.position.x ? Vector2.left : Vector2.right) + (GetComponent<Patrol>() == null ? (transform.position.y < player.position.y ? Vector2.down : Vector2.up) : Vector2.zero)) * 15);

        if (currentHealth <= 0)
        {
            DestroyTarget(0, destroyFunction);
        }
    }

    IEnumerator DamageTakenAnim()
    {
        spriteRenderer.color = new Vector4(1, 1, 1, 0);
        yield return new WaitForSeconds(.1f);
        spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(.1f);
        spriteRenderer.color = new Vector4(1, 1, 1, 0);
        yield return new WaitForSeconds(.1f);
        spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(.1f);
    }

    void DestroyTarget(float destroyTime, UnityEvent unityEvent)
    {
        if(unityEvent != null) unityEvent.Invoke();

        DropCoins(coinsAmountToDrop, transform.position);

        Destroy(this.gameObject, destroyTime);
    }

    public void DropCoins(int coinsAmount, Vector3 spawnPos)
    {
        if (canDropCoins)
        {
            int tenthPercent = coinsAmount % 10;

            int tenth = coinsAmount / 10;
            int fifth = tenthPercent / 5;
            int onece = tenthPercent % 5;

            InstantiateCoins(0, tenth, spawnPos);
            InstantiateCoins(1, fifth, spawnPos);
            InstantiateCoins(2, onece, spawnPos);
        }
    }

    void InstantiateCoins(int index, int type, Vector3 spawnPos)
    {
        if (type >= 1)
        {
            for (int i = 0; i < type; i++)
            {
                Instantiate(coin[index], spawnPos, Quaternion.identity);
            }
        }
    }

    IEnumerator Spawn()
    {
        yield return new WaitForSeconds(.5f);

        spriteRenderer.color = new Vector4(1, 1, 1, 1);
    }

    void Update()
    {
        if (!CanMove)
        {
            return;
        }

        if(aIPath != null)
        {
            if (aIPath.canMove)
            {
                spriteRenderer.flipX = aIPath.desiredVelocity.x > 0 ? false : true;
            }
        }

        if (aIDestinationSetter != null && aIPath != null)
        {
            if (Physics2D.Linecast(transform.position, player.position, whatIsCollider))
            {
                Debug.DrawLine(transform.position, player.position, Color.red);

                StopPathFindinding();
            }
            else
            {
                float dst = Vector3.Distance(player.position, transform.position);

                if (dst <= aggroThreshold)
                {
                    StartPathFindinding();
                }
                else if (!followForever)
                {
                    StopPathFindinding();
                }
            }
        }
    }

    void StopPathFindinding()
    {
        if (follow)
        {
            follow = false;
            aIDestinationSetter.target = null;
            interpolationFactor = 0;
        }
        if (aIPath.maxSpeed > 0)
        {
            aIPath.maxSpeed = Mathf.Lerp(3, 0, interpolationFactor);

            interpolationFactor += .5f * Time.deltaTime;
        }
        else
        {
            interpolationFactor = 0;
        }
    }
    void StartPathFindinding()
    {
        if (!follow)
        {
            follow = true;
            aIDestinationSetter.target = player;
            interpolationFactor = 0;
        }
        if (aIPath.maxSpeed < 3)
        {
            aIPath.maxSpeed = Mathf.Lerp(0, 3, interpolationFactor);

            interpolationFactor += Time.deltaTime;
        }
        else
        {
            interpolationFactor = 0;
        }
    }

    private void FixedUpdate()
    {
        /*
        isClipped = Physics2D.Raycast(transform.position, Vector2.right, 2f, whatIsCollider)
            && Physics2D.Raycast(transform.position, Vector2.left, 2f, whatIsCollider)
            && Physics2D.Raycast(transform.position, Vector2.up, 2f, whatIsCollider)
            && Physics2D.Raycast(transform.position, Vector2.down, 2f, whatIsCollider);
        */
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Sword") && enemyType == EnemyType.destructibleObject)
        {
            DestroyTarget(.05f, destroyFunction);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Player") && enemyType != EnemyType.destructibleObject)
        {
            PlayerController playerController = collision.transform.GetComponent<PlayerController>();

            if (!playerController.canMove)
            {
                return;
            }

            Transform playerPos = collision.transform;

            Vector2 direction = new Vector2(playerPos.position.x > transform.position.x ? 1 : -1, 1);

            playerController.TakeDamage(damage, direction);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Handles.color = Color.green;
        Handles.DrawWireDisc(transform.position, Vector3.forward, aggroThreshold);
    }
#endif
}
