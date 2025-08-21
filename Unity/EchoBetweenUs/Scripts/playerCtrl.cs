//====================================================
// 파일: playerCtrl.cs
// 역할: 플레이어 이동/점프 입력 처리, 점프 차지·해제, 넉백 반응, 보행 SFX·애니메이션 동기화
// 패턴: Controller(MonoBehaviour), Time-split(Update/Input vs FixedUpdate/Physics)
// 협력자: Rigidbody2D, Animator, SoundManager, KnockbackSignal, startSceneManager
// 주의: Update(입력/상태)와 FixedUpdate(물리) 분리. 상태 플래그 간섭 주의.
// TODO: 클래스/필드 네이밍 PascalCase로 정리(PlayerController 등), 싱글톤 결합(Score/Sound) 추상화
//====================================================

using UnityEngine;

/// <summary>
/// 수평 이동과 점프 차지/발사를 관리하고, 지면 접촉·넉백·애니메이션을 조정합니다.
/// 입력은 Update에서 수집하고, 물리는 FixedUpdate에서 적용합니다.
/// </summary>
public class playerCtrl : MonoBehaviour
{
    // ---------- References ----------
    public Rigidbody2D playerBody;
    public Collider2D feetCollider;
    private SpriteRenderer playerSprite;
    public Animator animator;

    // ---------- SFX/Anim ----------
    private int walkSFXCnt = 0;
    private float walkSFXCooldown = 0.3f;
    private float walkSFXTimer = 0f;
    private bool isJumpChargingOnce = false;

    // ---------- Jump (Charge/Release) ----------
    public float jumpForce = 0;           // 누적 차지량(임펄스)
    public float jumpForceLimit = 30;     // 차지 상한
    public float jumpForceIncrease = 2.5f;// 프레임당 증가량(시작 시 재계산)
    public float jumpForceChargeTime = 1.5f; // 풀차지 목표 시간(초)
    private float clipLength = 4f / 12f;  // 애니메이션 클립 길이(프레임/12fps 가정)
    private float speedMultiplier;        // 애니 클립 속도 = clipLength / chargeTime

    // ---------- Action Flags ----------
    public bool isPressingAct = false;
    public bool isPressingLightAct = false;
    public bool isActing = false;
    public bool isActOnce = false;
    bool isActJump = false;

    // ---------- Move ----------
    public float moveInput;
    public float moveSpeed = 10f;
    public int facing = 1;

    // ---------- Ground Check ----------
    bool isGrounded;
    public Transform groundCheck;     // (미사용) feetCollider 기반으로 접촉 판정 중
    public LayerMask groundLayer;

    // ---------- Cooldowns ----------
    public float landingCooldown = 0f;
    public float coolDownSec = 0.2f;
    bool wasGrounded;

    // ---------- Knockback ----------
    private float knockbackTimer = 0f;
    [SerializeField] private float knockbackDuration = 0.2f;

    // ---------- Move Charge ----------
    [SerializeField] private float moveChargeTime = 0f;
    [SerializeField] private float maxMoveChargeTime = 2.0f;

    // ---------- Lifecycle ----------
    private void OnEnable()
    {
        KnockbackSignal.OnPlayerKnockedBack += HandleKnockback;
    }

    private void OnDisable()
    {
        KnockbackSignal.OnPlayerKnockedBack -= HandleKnockback;
    }

    private void HandleKnockback()
    {
        Debug.Log("[playerCtrl] Player was knocked back!");
        knockbackTimer = knockbackDuration;
    }

    private void Start()
    {
        playerSprite = GetComponent<SpriteRenderer>();

        // 의도: 지정한 충전 시간(jumpForceChargeTime)에 맞춰 풀차지 되도록 증가량 계산.
        // 60Hz 고정틱 기준 Time.fixedDeltaTime 사용 → FixedUpdate 누적과 일치.
        jumpForceIncrease = (jumpForceLimit - 10f) * Time.fixedDeltaTime / jumpForceChargeTime;

        // 애니메이션 클립 재생 속도를 충전 시간에 동기화.
        speedMultiplier = clipLength / jumpForceChargeTime;
        animator.SetFloat("jumpChargeSpeed", speedMultiplier);
    }

    private void Update()
    {
        // 입력 수집 및 상태 전이(물리 적용은 FixedUpdate에서 수행)
        moveInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetMouseButtonDown(1))
        {
            startSceneManager.Instance.LoadGameOver();
        }

        // 바라보는 방향/스프라이트 뒤집기
        if (moveInput == 1) { facing = 1;  playerSprite.flipX = false; }
        if (moveInput == -1){ facing = -1; playerSprite.flipX = true; }

        // 점프 차지/해제 입력(지면, 비행동, 쿨타임 종료 조건)
        if (landingCooldown <= 0f && !isActing && isGrounded)
        {
            if (Input.GetButton("Jump"))
            {
                isPressingAct = true;
                isActJump = true;
            }
            else if (Input.GetButtonUp("Jump"))
            {
                isPressingAct = false;
                isActing = true;
                isActOnce = true;
                landingCooldown = coolDownSec; // 연속 점프 방지
            }
        }

        // 지면 접촉 판정(콜라이더-레이어 접촉)
        isGrounded = feetCollider.IsTouchingLayers(groundLayer);

        // 착지 프레임: 쿨타임 갱신 및 행동 종료
        if (!wasGrounded && isGrounded)
        {
            landingCooldown = coolDownSec;
            isActing = false;
        }

        // 지면 상태에서 이동 차지(수평 힘 가중용)
        if (isGrounded)
        {
            isActing = false;
            moveChargeTime = (moveInput != 0)
                ? Mathf.Min(moveChargeTime + Time.deltaTime, maxMoveChargeTime)
                : 0f;
        }

        // 타이머 감소
        if (landingCooldown > 0f) landingCooldown -= Time.deltaTime;
        if (walkSFXTimer > 0f)    walkSFXTimer    -= Time.deltaTime;

        wasGrounded = isGrounded;
    }

    private void FixedUpdate()
    {
        // 물리/애니메이션 플래그 초기화
        animator.SetBool("isWalking", false);
        animator.SetBool("isJumping", false);

        // 넉백 중에는 조작 잠시 차단
        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            return;
        }

        // 점프 차지 중: 수평 정지 + 차지 애니메이션 1회 트리거
        if (isPressingAct && isActJump)
        {
            if (jumpForce < jumpForceLimit)
            {
                if (jumpForce == 0) jumpForce += 10; // 최소 임펄스 보장(바닥 이탈)
                jumpForce += jumpForceIncrease;
            }

            if (!isJumpChargingOnce)
            {
                isJumpChargingOnce = true;
                animator.SetBool("isJumpCharging", true);
                animator.SetBool("isWalking", false);
                animator.SetBool("isJumping", false);
            }

            // 차지 중 수평 정지(미끄러짐 방지)
            playerBody.linearVelocity = new Vector2(0f, playerBody.linearVelocity.y);
        }
        // 점프 발사 프레임
        else if (isActOnce && isActJump)
        {
            isActOnce = false;

            // 수평 가중치: 누적 이동 차지를 jumpForce에 비례하여 가중
            playerBody.AddForce(
                Vector2.up * jumpForce +
                new Vector2(moveInput * (moveChargeTime / maxMoveChargeTime) * jumpForce, 0),
                ForceMode2D.Impulse);

            jumpForce = 0;
            isActJump = false;

            animator.SetBool("isWalking", false);
            animator.SetBool("isJumpCharging", false);
            animator.SetBool("isJumping", true);
            isJumpChargingOnce = false;

            SoundManager.Instance.PlaySFX(3);
        }

        // 지면에서의 수평 이동(차지 아님)
        if (!isPressingAct && isGrounded)
        {
            playerBody.linearVelocity = new Vector2(moveInput * moveSpeed, playerBody.linearVelocity.y);

            animator.SetBool("isWalking", moveInput != 0);
            animator.SetBool("isJumping", false);

            // 보행 SFX: 교대 재생(좌/우 발 구분 느낌)
            if (moveInput != 0 && walkSFXTimer <= 0f)
            {
                SoundManager.Instance.PlaySFX(6 + (walkSFXCnt % 2));
                walkSFXCnt += 1;
                walkSFXTimer = walkSFXCooldown;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 벽 충돌 시 반사 + 약간 상승 보정 → 넉백
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            Vector2 reflected = new Vector2(-playerBody.linearVelocity.x, playerBody.linearVelocity.y + 0.5f).normalized;
            playerBody.linearVelocity = Vector2.zero;
            playerBody.AddForce(reflected * 10f, ForceMode2D.Impulse);
            knockbackTimer = knockbackDuration;
        }
    }
}
