using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Cinemachine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour, IDataPersistence
{
    [HideInInspector] public Vector2 savedPos;
    //Transform camPos => transform.GetChild(1);
    public bool canMove = true;
    [HideInInspector] public static int enemiesKilled = 0;

    Rigidbody2D _playerRigidbody;
    [HideInInspector] public Animator _playerAnimator;
    SpriteRenderer _spriteRenderer;
    BoxCollider2D _playerCollider;

    [Header("Unlock Abilities")]
    [Space]
    public bool wallJump = false;
    public bool dash = false;
    public bool dobleJump = false;

    [Space]

    [SerializeField] float resetAbilitiesCooldown = .5f;

    [Header("Health Parameters")]
    [Space]
    public int maxHealth = 5;
    [SerializeField] Transform healthBar;
    [SerializeField] GameObject healthPoint;
    public bool canHeal = true;
    [SerializeField] float healingCooldown = .5f;
    [SerializeField] float healTime = 1f;
    [Space]
    public bool canTakeDamage = true;

    public int currentHealth;

    [Header("Mana Parameters")]
    [Space]
    public int maxMana = 5;
    [SerializeField] Transform manaBar;
    [SerializeField] GameObject manaPoint;
    public int currentMana;

    [Header("Speeds")]
    [Space]
    [SerializeField] float speed = 30f;
    [SerializeField] public float jumpSpeed = 20f;
    [SerializeField] float dashSpeed = 20f;
    [SerializeField] public float knockBackSpeed = 20f;
    [SerializeField] float airMultiplyer;
    public float moveAxisThreshold = .3f;
    public float xAxisThreshold = .3f;
    public float yAxisThreshold = .3f;

    [Header("Raycast")]
    [Space]
    [SerializeField] float groundRayOffset = .2f;
    [SerializeField] float wallRayOffset = .2f;
    [SerializeField] float wallDetectDist = .2f;

    [SerializeField] float TopBottomAirDetectionDist = .1f;

    [Space]

    [SerializeField] private LayerMask whatIsGorund;
    [SerializeField] private LayerMask whatIsWalls;

    [Header("Wall Parametres")]
    [Space]
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float wallJumpCooldown;

    [Header("Dash Parameters")]
    [Space]
    [SerializeField] float dashTime;
    [SerializeField] float dashCooldown;

    [Header("Attack Parametres")]
    [Space]
    [SerializeField] Transform forwardSword;
    [Space]
    [SerializeField] float attackCooldown = .5f;
    int attackIndex = 1;

    [HideInInspector] public float verticalInput;
    [HideInInspector] public float horizontalInput;
    Vector3 moveDirection;
    float viewDirection = 0;
    
    private float playerHight => GetComponent<BoxCollider2D>().size.y;
    private float playerWidth => GetComponent<BoxCollider2D>().size.x;
    [HideInInspector] public float gravityScale;

    bool activateSpeedControl = true;
    bool canJump = true;
    bool canDobleJump = true;
    bool canWallJump = true;
    bool jumpCancel = false;
    [HideInInspector] public bool isGrounded = true;
    bool isWalledRight = false;
    bool isWalledLeft = false;
    bool isBodyAiredLeft = true;
    bool isBodyAiredRight = true;
    bool canAttack = true;
    bool canDownAttack = true;
    bool canUpAttack = true;
    bool canDash = true;
    public bool isDashing = false;
    bool canResetAbilities = true;
    bool inputEdgeGoDown = false;
    bool canEdgeGoDown = false;
    bool dashInput = true;

    bool respawn = false;
    bool isRespawnSaved = false;

    string lastSpawnPoint;

    private void Awake()
    {
        _playerRigidbody = GetComponent<Rigidbody2D>();
        _playerAnimator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _playerCollider = GetComponent<BoxCollider2D>();

        gravityScale = _playerRigidbody.gravityScale;

        if (PlayerPrefs.HasKey("health") && PlayerPrefs.GetInt("health") > 0)
        {
            currentHealth = PlayerPrefs.GetInt("health");
        }
        else
        {
            currentHealth = maxHealth;
        }

        if (PlayerPrefs.HasKey("mana"))
        {
            currentMana = PlayerPrefs.GetInt("mana");
        }
        else
        {
            currentMana = maxMana;
        }
    }

    private void Start()
    {
        UpdateHealthBar();
        UpdateManaBar();

        if (PlayerPrefs.HasKey("flip"))
        {
            _spriteRenderer.flipX = PlayerPrefs.GetInt("flip") == 1 ? true : false;
            viewDirection = PlayerPrefs.GetInt("flip") == 1 ? 0 : 1;
        }

        if (PlayerPrefs.HasKey(SceneManager.GetActiveScene().path + "StartPosX") && !respawn)
        {
            Vector2 savedPos = new Vector2(PlayerPrefs.GetFloat(SceneManager.GetActiveScene().path + "StartPosX"), PlayerPrefs.GetFloat(SceneManager.GetActiveScene().path + "StartPosY"));

            transform.position = new Vector3(savedPos.x, savedPos.y, 0);
        }

        if(SceneManager.GetActiveScene().name == lastSpawnPoint && isRespawnSaved && respawn)
        {
            transform.position = savedPos;
        }

        StartCoroutine(MoveStartDelay());
    }

    IEnumerator MoveStartDelay()
    {
        canMove = false;

        yield return new WaitForSeconds(1f);

        canMove = true;
    }

    public void SaveData(GameData data)
    {

    }

    public void LoadData(GameData data)
    {
        this.wallJump = data.wallJump;
        this.dash = data.dash;
        this.dobleJump = data.dobleJump;

        this.respawn = data.respawn;
        this.lastSpawnPoint = data.lastSpawnPoint;
        this.savedPos = data.savePos;
        this.isRespawnSaved = data.isRespawnSaved;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            PlayerPrefs.DeleteAll();
        }

        if (isDashing)
        {
            _playerRigidbody.gravityScale = 0;
            _playerRigidbody.velocity = new Vector2(_playerRigidbody.velocity.x, 0);
        }
        else
        {
            _playerRigidbody.gravityScale = gravityScale;
        }

        if (!canMove)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            dash = dash ? false : true;
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            wallJump = wallJump ? false : true;
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            dobleJump = dobleJump ? false : true;
        }

        if (currentHealth <= 0 && canMove)
        {
            canMove = false;
            respawn = true;

            SaveStats();
            StartCoroutine(ChangeScene.instance.ChangeSceneFunc(1.5f, lastSpawnPoint, this, Vector2.zero));

            _playerAnimator.Play("Player_Despawn");
        }

        if (canMove)
        {
            PlayerInput();
            StartCoroutine(SpeedControl());
        }
    }

    private void FixedUpdate()
    {
        if (!canMove)
        {
            return;
        }

        isGrounded = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y - (playerHight / 2)), Vector2.down, .05f, whatIsGorund)
            || Physics2D.Raycast(new Vector2(transform.position.x + groundRayOffset, transform.position.y - (playerHight / 2)), Vector2.down, .05f, whatIsGorund)
            || Physics2D.Raycast(new Vector2(transform.position.x - groundRayOffset, transform.position.y - (playerHight / 2)), Vector2.down, .05f, whatIsGorund);

        isWalledRight = (Physics2D.Raycast(transform.position, Vector2.right, (playerWidth / 2) + wallDetectDist, whatIsWalls)
            || Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y + wallRayOffset), Vector2.right, (playerWidth / 2) + wallDetectDist, whatIsWalls)
            || Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y - wallRayOffset), Vector2.right, (playerWidth / 2) + wallDetectDist, whatIsWalls))
            && canAttack && horizontalInput > 0;

        isWalledLeft = (Physics2D.Raycast(transform.position, Vector2.left, (playerWidth / 2) + wallDetectDist, whatIsWalls)
            || Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y + wallRayOffset), Vector2.left, (playerWidth / 2) + wallDetectDist, whatIsWalls)
            || Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y - wallRayOffset), Vector2.left, (playerWidth / 2) + wallDetectDist, whatIsWalls))
            && canAttack && horizontalInput < 0;

        isBodyAiredLeft = !Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y + (playerHight / 2 - TopBottomAirDetectionDist)), Vector2.left, (playerWidth / 2) + wallDetectDist, whatIsWalls)
            || !Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y - (playerHight / 2 - TopBottomAirDetectionDist)), Vector2.left, (playerWidth / 2) + wallDetectDist, whatIsWalls);

        isBodyAiredRight = !Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y + (playerHight / 2 - TopBottomAirDetectionDist)), Vector2.right, (playerWidth / 2) + wallDetectDist, whatIsWalls)
            || !Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y - (playerHight / 2 - TopBottomAirDetectionDist)), Vector2.right, (playerWidth / 2) + wallDetectDist, whatIsWalls);

        if (canMove)
        {
            MovePlayer();
        }
    }

    private void LateUpdate()
    {
        if (!canMove)
        {
            return;
        }

        if (Mathf.Abs(horizontalInput) > moveAxisThreshold)
        {
            _playerAnimator.SetBool("IsWalking", true);
        }
        else
        {
            _playerAnimator.SetBool("IsWalking", false);
        }

        if(canJump && isGrounded)
        {
            _playerAnimator.SetBool("IsJumping", false);
        }

        if(!isGrounded && !_playerAnimator.GetBool("IsJumping"))
        {
            _playerAnimator.SetBool("IsFalling", true);
        }
        else
        {
            _playerAnimator.SetBool("IsFalling", false);
        }

        if((isWalledLeft || isWalledRight) && !isGrounded && (!isBodyAiredLeft || !isBodyAiredRight) && canJump)
        {
            _playerAnimator.SetBool("IsWallSlideing", true);
        }
        else
        {
            _playerAnimator.SetBool("IsWallSlideing", false);
        }

        _playerAnimator.SetBool("IsAttaking", !canAttack);
        _playerAnimator.SetBool("IsDashing", isDashing && !canDash);
    }

    void PlayerInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if(Mathf.Abs(horizontalInput) > moveAxisThreshold)
        {
            horizontalInput = horizontalInput > 0 ? 1 : -1;
        }

        inputEdgeGoDown = verticalInput < -yAxisThreshold;

        if (!isDashing && canAttack && canDownAttack && canUpAttack)
        {
            if (!isWalledRight || !isWalledLeft)
            {
                if (Mathf.Abs(horizontalInput) > moveAxisThreshold)
                {
                    viewDirection = horizontalInput;
                }

                if (viewDirection < 0)
                {
                    _spriteRenderer.flipX = true;
                }
                else
                {
                    _spriteRenderer.flipX = false;
                }
            }

            if (isWalledLeft && !isGrounded)
            {
                viewDirection = 1;
                _spriteRenderer.flipX = false;
            }

            if (isWalledRight && !isGrounded)
            {
                viewDirection = -1;
                _spriteRenderer.flipX = true;
            }
        }
        
        if (isWalledRight && !isBodyAiredRight)
        {
            _playerRigidbody.gravityScale = gravityScale / 2;
        }
        else if(isWalledLeft && !isBodyAiredLeft)
        {
            _playerRigidbody.gravityScale = gravityScale / 2;
        }
        else
        {
            _playerRigidbody.gravityScale = gravityScale;
        }

        if (canResetAbilities)
        {
            if ((isGrounded || isWalledLeft || isWalledRight) && ((!canDash && !isDashing) || !canDobleJump))
            {
                canResetAbilities = false;

                AbilitiesReset(5);
            }
        }

        if(Input.GetMouseButton(1) || Input.GetKey(KeyCode.Joystick1Button1))
        {
            if (canHeal && currentHealth != maxHealth && isGrounded && currentMana > 0)
            {
                canHeal = false;

                Heal(1);
            }
        }
        else if(Input.GetMouseButtonUp(1) || Input.GetKeyUp(KeyCode.Joystick1Button1))
        {
            if(!canHeal) canHeal = true;
        }

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.JoystickButton2))
        {
            if (canAttack)
            {
                canAttack = false;

                if (verticalInput < -yAxisThreshold)
                {
                    AttackMechanic(Vector2.one);

                    _playerAnimator.Play("Player_Down_Attack");
                }
                else if (verticalInput > yAxisThreshold)
                {
                    AttackMechanic(Vector2.one);

                    _playerAnimator.Play("Player_Up_Attack");
                }

                if ((verticalInput > -yAxisThreshold && verticalInput < yAxisThreshold) || Mathf.Abs(horizontalInput) > xAxisThreshold)
                {
                    attackIndex++;
                    if (attackIndex >= 3)
                    {
                        attackIndex = 1;
                    }

                    if (!_spriteRenderer.flipX)
                    {
                        AttackMechanic(Vector2.right);
                    }
                    else
                    {
                        AttackMechanic(Vector2.left);
                    }

                    _playerAnimator.Play(attackIndex == 1 ? "Player_Attack1" : "Player_Attack2");
                }

                Invoke(nameof(AttackReset), attackCooldown);
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetAxisRaw("Right Trigger") >= xAxisThreshold)
        {
            if (canDash && dash && dashInput && !isWalledLeft && !isWalledRight)
            {
                dashInput = false;
                canDash = false;
                isDashing = true;

                if (horizontalInput <= -xAxisThreshold)
                {
                    _spriteRenderer.flipX = true;
                    DashMechanic(Vector2.left);
                }
                else if(horizontalInput >= xAxisThreshold)
                {
                    _spriteRenderer.flipX = false;
                    DashMechanic(Vector2.right);
                }
                else if(horizontalInput < xAxisThreshold && horizontalInput > -xAxisThreshold)
                {
                    DashMechanic(_spriteRenderer.flipX ? Vector2.left : Vector2.right);
                }

                _playerAnimator.Play("Player_Dash");

                StartCoroutine(DashFinish());
                Invoke(nameof(ResetAbilities), resetAbilitiesCooldown);
            }
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetAxisRaw("Right Trigger") <= 0)
        {
            if (!dashInput)
            {
                dashInput = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            if (!isDashing)
            {
                if (canJump && isGrounded)
                {
                    canJump = false;

                    JumpMechanic();

                    Invoke(nameof(JumpReset), jumpCooldown);
                }

                if (!isGrounded && canWallJump && wallJump)
                {
                    canWallJump = false;

                    if (isWalledRight && !isBodyAiredRight)
                    {
                        StartCoroutine(WallJumpMechanic(Vector2.left));
                    }
                    if (isWalledLeft && !isBodyAiredLeft)
                    {
                        StartCoroutine(WallJumpMechanic(Vector2.right));
                    }

                    Invoke(nameof(ResetAbilities), resetAbilitiesCooldown);

                    Invoke(nameof(WallJumpReset), jumpCooldown);
                }

                if (canDobleJump && !isGrounded && !isWalledLeft && !isWalledRight && dobleJump)
                {
                    canDobleJump = false;

                    JumpMechanic();

                    Invoke(nameof(ResetAbilities), resetAbilitiesCooldown);
                }
            }
        }
        else if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.JoystickButton0))
        {
            if ((!canJump || !canDobleJump) && jumpCancel)
            {
                jumpCancel = false;
                _playerRigidbody.velocity = new Vector2(_playerRigidbody.velocity.x, _playerRigidbody.velocity.y / 8);
            }
        }
    }
    
    public void TakeDamage(int damage, Vector2 direction)
    {
        if (canTakeDamage)
        {
            currentHealth -= damage;
            StartCoroutine(KnockBackMechanic(direction * knockBackSpeed * 3f));
            UpdateHealthBar();

            StartCoroutine(DamageTakenAnim());

            _playerAnimator.Play("Take_Damage");

            StartCoroutine(CameraShake.instance.Shake(.2f, .3f));
            StartCoroutine(TimeScale(.2f));
        }
    }

    public IEnumerator TimeScale(float decreasSpeed)
    {
        float originalScale = Time.timeScale;

        float time = 0.0f;

        while (time > 0)
        {
            print(Time.timeScale);

            Time.timeScale = time;

            time -= Time.deltaTime * decreasSpeed;

            yield return null;
        }

        Time.timeScale = originalScale;
    }

    IEnumerator DamageTakenAnim()
    {
        Color color = _spriteRenderer.color;
        canTakeDamage = false;
        color = new Color(1, 1, 1, 0);
        _spriteRenderer.color = color;
        yield return new WaitForSeconds(.1f);
        color = new Color(1, 1, 1, 1);
        _spriteRenderer.color = color;
        yield return new WaitForSeconds(.1f);
        color = new Color(1, 1, 1, 0);
        _spriteRenderer.color = color;
        yield return new WaitForSeconds(.1f);
        color = new Color(1, 1, 1, 1);
        _spriteRenderer.color = color;
        yield return new WaitForSeconds(.25f);
        canTakeDamage = true;
    }

    public void GainMana(int manaPoints)
    {
        if (currentMana < maxMana)
        {
            currentMana += manaPoints;
            UpdateManaBar();
        }
    }

    public void Heal(int healPoints)
    {
        if(canTakeDamage)
        {
            StartCoroutine(HealZoom(healTime, .35f, 4, healPoints));
        }
    }

    public IEnumerator HealZoom(float duration, float zoomSpeed, float maxZoom, int healPoints)
    {
        CinemachineVirtualCamera virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        float originalCameraSize = virtualCamera.m_Lens.OrthographicSize;

        Vector2 keepPos = transform.position;
        float elapsed = 0.0f;

        _playerAnimator.SetBool("CanHeal", false);
        _playerAnimator.Play("Player_Heal");

        while (elapsed < duration && !canHeal && canTakeDamage)
        {
            horizontalInput = 0;
            verticalInput = 0;
            ChangeActiveState(false);
            transform.position = keepPos;

            if (virtualCamera.m_Lens.OrthographicSize > maxZoom)
            {
                virtualCamera.m_Lens.OrthographicSize -= Time.deltaTime * zoomSpeed;
            }

            elapsed += Time.deltaTime;

            yield return null;
        }

        if (!canHeal)
        {
            RegenerateHealth(healPoints);
        }

        _playerAnimator.SetBool("CanHeal", true);
        _playerAnimator.Play("Player_Idle");

        ChangeActiveState(true);
        StartCoroutine(HealZoomReset(virtualCamera, originalCameraSize));
    }

    public IEnumerator HealZoomReset(CinemachineVirtualCamera virtualCamera, float originalCameraSize)
    {
        while (virtualCamera.m_Lens.OrthographicSize < originalCameraSize)
        {
            virtualCamera.m_Lens.OrthographicSize += Time.deltaTime * 2f;

            yield return null;
        }

        Invoke(nameof(HealReset), healingCooldown);

        virtualCamera.m_Lens.OrthographicSize = originalCameraSize;
    }

    void RegenerateHealth(int healPoints)
    {
        currentMana--;
        UpdateManaBar();
        currentHealth += healPoints;
        UpdateHealthBar();
    }

    public void UpdateHealthBar()
    {
        foreach (Transform t in healthBar)
        {
            Destroy(t.gameObject);
        }

        for (int i = 0; i < currentHealth; i++)
        {
            Transform t = Instantiate(healthPoint, healthBar).transform;
        }
    }

    public void UpdateManaBar()
    {
        foreach (Transform t in manaBar)
        {
            Destroy(t.gameObject);
        }

        for (int i = 0; i < currentMana; i++)
        {
            Transform t = Instantiate(manaPoint, manaBar).transform;
        }
    }

    void ChangeActiveState(bool b)
    {
        canAttack = b;
        canJump = b;
        canDash = b;
    }

    IEnumerator EdgeGoDown(EdgeCollider2D edgeCollider2D)
    {
        yield return new WaitForSeconds(.2f);

        if (inputEdgeGoDown)
        {
            canEdgeGoDown = false;
            edgeCollider2D.enabled = false;
        }
        else
        {
            yield break;
        }
        yield return new WaitForSeconds(.2f);

        edgeCollider2D.enabled = true;
    }

    public IEnumerator KnockBackMechanic(Vector2 direction)
    {
        canJump = canJump ? true : true;
        
        yield return new WaitForFixedUpdate();

        _playerRigidbody.velocity = Vector2.zero;

        _playerRigidbody.AddForce(direction, ForceMode2D.Impulse);
    }

    public void DashMechanic(Vector2 direction)
    {
        _playerRigidbody.velocity = Vector2.zero;

        canMove = false;
        canTakeDamage = false;

        _playerRigidbody.AddForce(direction * dashSpeed, ForceMode2D.Impulse);
    }

    void AttackMechanic(Vector2 direction)
    {
        isDashing = false;
        
        forwardSword.localScale = new Vector3(direction.x, 1, 1);
    }

    public void JumpMechanic()
    {
        jumpCancel = true;

        StartCoroutine(Jump());

        if (!canAttack) { return; }

        _playerAnimator.SetBool("IsJumping", false);

        _playerAnimator.Play("Player_Fall");
        _playerAnimator.Play("Player_Jump");

        _playerAnimator.SetBool("IsJumping", true);
    }

    public IEnumerator WallJumpMechanic(Vector2 direction)
    {
        jumpCancel = true;

        yield return new WaitForFixedUpdate();

        _playerRigidbody.velocity = new Vector3(_playerRigidbody.velocity.x, 0f);

        _playerRigidbody.AddForce(Vector2.up * jumpSpeed * 1.1f, ForceMode2D.Impulse);
        _playerRigidbody.AddForce(direction * jumpSpeed * 0.5f, ForceMode2D.Impulse);
    }

    IEnumerator Jump()
    {
        yield return new WaitForFixedUpdate();

        _playerRigidbody.velocity = new Vector3(_playerRigidbody.velocity.x / 2, 0f);

        _playerRigidbody.AddForce(Vector2.up * jumpSpeed, ForceMode2D.Impulse);
    }

    void MovePlayer()
    {
        moveDirection = transform.right * (Mathf.Abs(horizontalInput) > moveAxisThreshold ? horizontalInput : 0);

        if (isGrounded)
        {
            _playerRigidbody.AddForce(moveDirection.normalized * speed, ForceMode2D.Force);
        }
        else
        {
            _playerRigidbody.AddForce(moveDirection.normalized * speed * airMultiplyer, ForceMode2D.Force);
        }
    }

    IEnumerator SpeedControl()
    {
        if (activateSpeedControl)
        {
            Vector2 flatVel = new Vector3(_playerRigidbody.velocity.x, 0);

            float newForce = speed / 2;

            if (flatVel.magnitude > newForce)
            {
                Vector2 limitedVel = flatVel.normalized * newForce;

                yield return new WaitForFixedUpdate();

                _playerRigidbody.velocity = new Vector2(limitedVel.x, _playerRigidbody.velocity.y);
            }
        }
    }

    void HealReset()
    {
        if(!canHeal) canHeal = true;
    }

    public void AbilitiesReset(float divider)
    {
        Invoke(nameof(DashReset), dashCooldown / divider);
        Invoke(nameof(DobleJumpReset), jumpCooldown / divider);
    }

    void ResetAbilities()
    {
        canResetAbilities = true;
    }

    void JumpReset()
    {
        canJump = true;
    }

    void DobleJumpReset()
    {
        canDobleJump = true;
    }

    void WallJumpReset()
    {
        canWallJump = true;
    }

    void AttackReset()
    {
        canAttack = true;
        canDownAttack = true;
        canUpAttack = true;
    }

    void DashReset()
    {
        canDash = true;
    }

    IEnumerator DashFinish()
    {
        yield return new WaitForSeconds(dashTime / 2);

        canMove = true;
        canTakeDamage = true;

        yield return new WaitForSeconds(dashTime / 2);

        _playerAnimator.Play("Player_Fall");
        isDashing = false;
        _playerRigidbody.velocity = new Vector2(_playerRigidbody.velocity.x / 4, 0);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(new Vector3(transform.position.x, transform.position.y - (playerHight / 2), 0), new Vector3(transform.position.x, transform.position.y - ((playerHight / 2) + .05f), 0));
        Gizmos.DrawLine(new Vector3(transform.position.x + groundRayOffset, transform.position.y - (playerHight / 2), 0), new Vector3(transform.position.x + groundRayOffset, transform.position.y - ((playerHight / 2) + .05f), 0));
        Gizmos.DrawLine(new Vector3(transform.position.x - groundRayOffset, transform.position.y - (playerHight / 2), 0), new Vector3(transform.position.x - groundRayOffset, transform.position.y - ((playerHight / 2) + .05f), 0));

        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x + (playerWidth / 2) + wallDetectDist, transform.position.y , 0));
        Gizmos.DrawLine(new Vector3(transform.position.x, transform.position.y + wallRayOffset, 0), new Vector3(transform.position.x + (playerWidth / 2) + wallDetectDist, transform.position.y + wallRayOffset, 0));
        Gizmos.DrawLine(new Vector3(transform.position.x, transform.position.y - wallRayOffset, 0), new Vector3(transform.position.x + (playerWidth / 2) + wallDetectDist, transform.position.y - wallRayOffset, 0));

        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x - ((playerWidth / 2) + wallDetectDist), transform.position.y, 0));
        Gizmos.DrawLine(new Vector3(transform.position.x, transform.position.y + wallRayOffset, 0), new Vector3(transform.position.x - ((playerWidth / 2) + wallDetectDist), transform.position.y + wallRayOffset, 0));
        Gizmos.DrawLine(new Vector3(transform.position.x, transform.position.y - wallRayOffset, 0), new Vector3(transform.position.x - ((playerWidth / 2) + wallDetectDist), transform.position.y - wallRayOffset, 0));

        Gizmos.DrawLine(new Vector3(transform.position.x, transform.position.y + (playerHight / 2 - TopBottomAirDetectionDist), 0), new Vector3(transform.position.x + ((playerWidth / 2) + wallDetectDist), transform.position.y + (playerHight / 2 - TopBottomAirDetectionDist), 0));
        Gizmos.DrawLine(new Vector3(transform.position.x, transform.position.y - (playerHight / 2 - TopBottomAirDetectionDist), 0), new Vector3(transform.position.x + ((playerWidth / 2) + wallDetectDist), transform.position.y - (playerHight / 2 - TopBottomAirDetectionDist), 0));
                                                                                                                                                                                                                                                                                   
        Gizmos.DrawLine(new Vector3(transform.position.x, transform.position.y + (playerHight / 2 - TopBottomAirDetectionDist), 0), new Vector3(transform.position.x - ((playerWidth / 2) + wallDetectDist), transform.position.y + (playerHight / 2 - TopBottomAirDetectionDist), 0));
        Gizmos.DrawLine(new Vector3(transform.position.x, transform.position.y - (playerHight / 2 - TopBottomAirDetectionDist), 0), new Vector3(transform.position.x - ((playerWidth / 2) + wallDetectDist), transform.position.y - (playerHight / 2 - TopBottomAirDetectionDist), 0));
    }
    /*
    private void OnCollisionStay2D(Collision2D other)
    {
        if ((whatIsGorund.value & (1 << other.transform.gameObject.layer)) > 0)
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if ((whatIsGorund.value & (1 << other.transform.gameObject.layer)) > 0)
        {
            isGrounded = false;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((whatIsGorund.value & (1 << collision.transform.gameObject.layer)) > 0)
        {
            _playerRigidbody.velocity = Vector2.zero;
            _playerCollider.isTrigger = false;
        }
    }
    */

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Coin"))
        {
            DroppedCoin droppedCoin = collision.transform.parent.GetComponent<DroppedCoin>();

            Coins.instance.PickUpCoin(droppedCoin.coinValue);

            Destroy(collision.transform.parent.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Edge"))
        {
            canEdgeGoDown = true;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Edge") && inputEdgeGoDown && canEdgeGoDown)
        {
            StartCoroutine(EdgeGoDown(collision.transform.GetComponent<EdgeCollider2D>()));
        }
    }

    public void SaveStats()
    {
        PlayerPrefs.SetInt("health", currentHealth);
        PlayerPrefs.SetInt("mana", currentMana);

        if(currentHealth <= 0)
        {
            PlayerPrefs.DeleteKey("health");
            PlayerPrefs.DeleteKey("mana");
        }
    }

    private void OnDisable()
    {
        SaveStats();
    }
}
