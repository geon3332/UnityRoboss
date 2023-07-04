using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IObjectInfo
{
    // 기본 능력치
    public static readonly int BASIC_MOVE_SPEED = 10; // 초기 이동 속도
    public static readonly int BASIC_HEALTH = 3; // 초기 체력
    public static readonly float BASIC_ATTACK_SPEED = 1f; // 초기 공격 속도

    // 데미지
    private const float DMG_ATTACK = 1f; // 평타 데미지
    private const float DMG_SKILL1 = 4f; // 차지샷 데미지
    private const float DMG_SKILL2 = 4f; // Q스킬 데미지

    // 받는 데미지
    private const int RECEIVE_DMG_FALL = 2; // 추락 시 데미지
    
    // 쿨다운
    private const float COOLDOWN_SKILL2 = 10f; // Q스킬 쿨다운
    private const float COOLDOWN_DEFANSE = 15f; // 쉴드 스킬 쿨다운

    // 가중치
    private const float WEIGHT_DAMAGE = 0.05f; // 공격력 업그레이드 가중치
    private const float WEIGHT_ATTACK_SPEED = 0.02f; // 공격 속도 업그레이드 가중치
    private const float WEIGHT_MOVE_SPEED = 0.2f; // 이동 속도 업그레이드 가중치

    // 속도 감속치
    private const float VELECITY_DECREASE = 0.1f; // 기본 속도 감속치
    private const float INERTIA_VELECITY_DECREASE = 0.8f; // 외부 속도 감속치

    // 피격 시 무적 관련
    private const float DAMAGE_INVINCIBLE_TIME = 1f; // 피격 시 무적 시간
    private const float DAMAGE_BLINK_TIME = 0.05f; // 피격 시 색이 깜빡이는 간격

    // 추락 후 원상 복구 시간
    private const float FALL_END_TIME = 1f;

    // 바퀴 회전 속도
    private const float WHEEL_SPIN_SPEED = 0.1f;

    // 물리
    private Rigidbody rigid;
    private Vector3 velocity; // 플레이어 조작 기본 속도
    private Vector3 inertiaVelocity; // 외부 속도

    private int isMaxHealth; // 최대 체력
    private int isHealth; // 현재 체력
    private float isMoveSpeed = BASIC_MOVE_SPEED; // 현재 이동 속도
    private float isAttackSpeed = BASIC_ATTACK_SPEED; // 현재 공격 속도

    private bool isActionDelay = false; // 행동 상태
    private bool isFall = false; // 추락 상태
    private bool isDamaged = false; // 데미지 입은 상태
    private bool isInvincible = false; // 무적 상태
    public bool isDead { get; set; } // 죽음 상태

    private Animator anim;
    private Renderer shieldColor; // 쉴드 렌더러
    private RaycastHit mouseHit; // 마우스 위치
    private GameObject isRespawnPoint; // 추락 시 이동되는 지점
    private GameObject mouseObject; // 커서 위치 잡기를 위한 투명 오브젝트

    private Coroutine actionCoroutine;

    [SerializeField] private Renderer playerColor;
    [SerializeField] private Transform upperTransform;
    [SerializeField] private Transform underTransform;
    [SerializeField] private GameObject leftShootEffect;
    [SerializeField] private GameObject rightShootEffect;
    [SerializeField] private GameObject energyBallEffect;
    [SerializeField] private GameObject shieldEffect;

    IObjectInfo iDamageConponent { get; set; } // 데미지 인터페이스

    #region propertys
    public int Health
    {
        get
        {
            return isHealth;
        }
        set
        {
            isHealth = Mathf.Clamp(value, 0, isMaxHealth * 4);
            UI.PlayerHealth.SetHealthUI(isHealth);
        }

    }
    public int MaxHealth
    {
        get
        {
            return isMaxHealth;
        }
        set
        {
            isMaxHealth = Mathf.Clamp(value, 1, 12);
            UI.PlayerHealth.SetMaxHealthUI(value);
        }

    }
    public float MoveSpeed
    {
        get
        {
            return isMoveSpeed;
        }
        set
        {
            isMoveSpeed = Mathf.Max(value, 10);
        }

    }
    public float AttackSpeed
    {
        get
        {
            return isAttackSpeed;
        }
        set
        {
            isAttackSpeed = Mathf.Max(value, 0);
        }

    }

    private GameObject RespawnPoint
    {
        get
        {
            if (isRespawnPoint == null)
                isRespawnPoint = GameObject.FindGameObjectWithTag("RespawnPoint");
            return isRespawnPoint;
        }
    }
    #endregion

    void Start()
    {
        velocity = Vector3.zero;
        inertiaVelocity = Vector3.zero;
        rigid = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        shieldColor = shieldEffect.GetComponent<Renderer>();
        StartCoroutine(SetPlayerData());
    }

    // UI 초기화 완료 후 조정
    IEnumerator SetPlayerData()
    {
        yield return null;
        MaxHealth = GameManager.Instance.isHealthUpgrade + BASIC_HEALTH;
        Health = MaxHealth * 4;
        MoveSpeed = BASIC_MOVE_SPEED + (GameManager.Instance.isMoveSpeedUpgrade * WEIGHT_MOVE_SPEED);
        AttackSpeed = BASIC_ATTACK_SPEED - (GameManager.Instance.isAtkSpeedUpgrade * WEIGHT_ATTACK_SPEED);
    }

    // 주는 데미지
    private float GetAttackDamage(float _damage)
    {
        return _damage * (1 + (GameManager.Instance.isDamageUpgrade * WEIGHT_DAMAGE));
    }

    //모든 행동 중지
    private void ActionEnd()
    {
        // 현재 진행중인 행동 코루틴 정지
        if (actionCoroutine != null)
            StopCoroutine(actionCoroutine);
        actionCoroutine = null;

        // 행동 비활성화
        isActionDelay = false;

        // 차지 사운드 정지
        SoundManager.Instance.SFXStop("EnergyChargeSE");

        // 모션 정지
        anim.SetBool("Right Rapid Attack", false);
        anim.SetBool("Both Aim", false);
        anim.SetBool("Left Aim", false);

        // 오브젝트 비활성화
        energyBallEffect.SetActive(false);
        leftShootEffect.SetActive(false);
        rightShootEffect.SetActive(false);
    }

    // 데미지 입음
    public void GetReceiveDamage(float damage, Vector3 angle)
    {
        if (!isInvincible && !isDamaged && !isDead && !isFall && !GameManager.Instance.isCinematic)
        {
            // 넉백
            inertiaVelocity = angle;

            // 피격 사운드 재생
            SoundManager.Instance.SFXPlay("PlayerHitSE");

            // 로비에선 피해를 받지 않음
            if (!GameManager.Instance.isLobby)
                Health -= (int)damage;

            if (isHealth > 0)
            {
                isDamaged = true;
                StartCoroutine(DamageBlink());
            }
            else
                StartCoroutine(DeadCoroutine());
        }
    }

    // 죽음 연출
    IEnumerator DeadCoroutine()
    {
        isDead = true;

        // 모든 행동 정지
        ActionEnd();

        // 데스 모션 재생
        anim.SetTrigger("Die");

        // 피격 이펙트
        EffectManager.Instance.CreateEffect("PlayerHit", transform.position + Vector3.up, Quaternion.identity, 1.0f);

        // 사운드 설정
        SoundManager.Instance.SFXPlay("PlayerBreakdownSE");
        SoundManager.Instance.BgmStop(false);

        // 플레이어 색 점차 빨갛게 변경
        float time = 0f;
        while (time < 1f)
        {
            playerColor.material.color = Color.Lerp(Color.white, Color.red, time);
            time += Time.deltaTime;
            yield return null;
        }
        playerColor.material.color = Color.red;
        yield return new WaitForSeconds(0.5f);

        // 플레이어 폭발
        PlayerExplosion();
    }

    private void PlayerExplosion()
    {
        // 폭발 사운드
        SoundManager.Instance.SFXPlay("ExplosionSE");

        // 폭발 이펙트
        EffectManager.Instance.CreateEffect("Explode", transform.position + Vector3.up, Quaternion.identity, 3.0f);

        // 필터 설정
        UI.instance.SetFilter(new Color(1, 1, 1, 0.5f), new Color(1, 1, 1, 0), 0.5f);

        // 카메라 흔들림 설정
        GameManager.CameraInstance.CameraNoise(0.2f, 0.1f, 0.5f);

        // 게임오버 UI 보이기
        UI.GameOver.ShowGameOverUI();
        gameObject.SetActive(false);
    }

    // 피격 시 일시적 무적
    IEnumerator DamageBlink()
    {
        float time = DAMAGE_INVINCIBLE_TIME;
        bool isBlink = false;
        while (time > 0)
        {
            isBlink = !isBlink;
            if (isBlink)
                playerColor.material.color = Color.red;
            else
                playerColor.material.color = Color.white;
            time -= 0.05f;
            yield return new WaitForSeconds(DAMAGE_BLINK_TIME);
        }
        playerColor.material.color = Color.white;
        isDamaged = false;
    }

    // 커서 위치 갱신
    void SetCursorPoint()
    {
        int layerMask = 1 << LayerMask.NameToLayer("MousePositionObject");

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out mouseHit, Mathf.Infinity, layerMask);
    }

    private void Move()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        if (GameManager.Instance.isCinematic)
        {
            x = 0; z = 0;
        }
        Vector3 speed = new Vector3(x, 0, z).normalized * isMoveSpeed * 2f;

        // 키보드 이동
        if (speed != Vector3.zero)
        {
            velocity += speed * VELECITY_DECREASE;
            if (velocity.magnitude > isMoveSpeed)
                velocity = velocity.normalized * isMoveSpeed;
        }
        else
        {
            float decrease = isMoveSpeed * VELECITY_DECREASE;
            velocity = velocity.normalized * (velocity.magnitude - decrease);
            if (velocity.magnitude < decrease)
                velocity = Vector3.zero;
        }

    }

    // 이동 모션 재생
    private void WalkAnimation()
    {
        // 현재 속도 비례
        if (velocity.magnitude > 0f)
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(velocity), WHEEL_SPIN_SPEED);

        if (isMoveSpeed > 0)
            anim.SetFloat("Forward", velocity.magnitude / isMoveSpeed);
        else
            anim.SetFloat("Forward", 0f);
    }

    // 외부 적용 속도
    private void InertiaMove()
    {
        if (inertiaVelocity != Vector3.zero)
        {
            float decrease = INERTIA_VELECITY_DECREASE;
            inertiaVelocity = inertiaVelocity.normalized * (inertiaVelocity.magnitude - decrease);
            if (inertiaVelocity.magnitude < decrease)
                inertiaVelocity = Vector3.zero;
        }
        rigid.velocity = new Vector3(velocity.x + inertiaVelocity.x, rigid.velocity.y, velocity.z + inertiaVelocity.z);
    }

    private void BodySpin()
    {
        // 회전
        if (!GameManager.Instance.isCinematic)
        {
            Vector3 dir = new Vector3(mouseHit.point.x, upperTransform.position.y, mouseHit.point.z);
            upperTransform.LookAt(dir);
            upperTransform.rotation = upperTransform.rotation * Quaternion.Euler(new Vector3(0, -90, -90));
        }
    }

    private void Attack()
    {
        if (Input.GetButton("Fire1") && !isActionDelay && !GameManager.Instance.isCinematic)
        {
            isActionDelay = true;
            rightShootEffect.SetActive(true);

            SoundManager.Instance.SFXPlay("BasicShootSE");
            anim.SetBool("Right Rapid Attack", true);

            Vector3 startPoint = rightShootEffect.transform.position;
            Vector3 dir = new Vector3(mouseHit.point.x, 0f, mouseHit.point.z) - rightShootEffect.transform.position;
            startPoint.y = transform.position.y + 0.8f;
            dir.y = 0f;
            ProjectileManager.Instance.CreateProjectile("Blue", startPoint, Quaternion.LookRotation(dir), 50f, GetAttackDamage(DMG_ATTACK));

            actionCoroutine = StartCoroutine(AttackEnd());
        }
    }

    IEnumerator AttackEnd()
    {
        yield return new WaitForSeconds(0.2f * isAttackSpeed);
        isActionDelay = false;
        rightShootEffect.SetActive(false);
        anim.SetBool("Right Rapid Attack", Input.GetButton("Fire1"));
    }

    private void Fall()
    {
        if (!isFall && transform.position.y <= -5f)
        {
            isFall = true;

            // 모든 행동 중지
            ActionEnd();

            // 모션 재생
            anim.SetTrigger("Take Damage");

            // 사운드 재생
            SoundManager.Instance.SFXPlay("FallSE");

            // 속도 초기화
            rigid.velocity = Vector3.zero;
            StartCoroutine(FallCoroutine());
        }
    }

    IEnumerator FallCoroutine()
    {
        // 추락 연출
        float time = 0f;
        while(time < 1f)
        {
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, time);
            time += Time.deltaTime * 2f;
            yield return null;
        }
        transform.localScale = Vector3.zero;
        yield return new WaitForSeconds(FALL_END_TIME);

        // 크기 복구
        transform.localScale = Vector3.one;

        // 리스폰 지점으로 이동
        transform.position = RespawnPoint.transform.position;

        // 추락 데미지
        GetReceiveDamage(RECEIVE_DMG_FALL, Vector3.zero);

        // 추락 종료
        isFall = false;
    }

    private void Skill1()
    {
        if (Input.GetButton("Fire2") && !isActionDelay && !GameManager.Instance.isCinematic)
        {
            SoundManager.Instance.SFXPlay("EnergyChargeSE");
            isActionDelay = true;
            energyBallEffect.SetActive(true);
            energyBallEffect.transform.localScale = Vector3.zero;
            actionCoroutine = StartCoroutine(Skill1Charge());
            anim.SetBool("Both Aim", true);
        }
    }
    IEnumerator Skill1Charge()
    {
        float chargeDamage = 0f;
        while (Input.GetButton("Fire2"))
        {
            chargeDamage += Time.deltaTime;
            chargeDamage = Mathf.Min(chargeDamage, 1f);
            energyBallEffect.transform.localScale = Vector3.one * (chargeDamage * 2f);
            yield return null;
        }
        energyBallEffect.SetActive(false);
        anim.SetBool("Both Aim", false);

        SoundManager.Instance.SFXStop("EnergyChargeSE");
        if (chargeDamage >= 0.5f)
        {
            anim.SetTrigger("Both Blast Attack");

            //손 이펙트
            leftShootEffect.SetActive(true);
            rightShootEffect.SetActive(true);

            //효과음
            SoundManager.Instance.SFXPlay("ChargeShotSE");

            //발사 이펙트
            EffectManager.Instance.CreateEffect("BlueExplode", energyBallEffect.transform.position, Quaternion.identity, 1.5f);

            float dmg = GetAttackDamage(DMG_SKILL1 * chargeDamage);

            //탄막 생성 후 크기 조절
            Vector3 dir = new Vector3(mouseHit.point.x, 0f, mouseHit.point.z) - transform.position;
            dir.y = 0f;
            GameObject p =  ProjectileManager.Instance.CreateProjectile("EnergyBall", energyBallEffect.transform.position, Quaternion.LookRotation(dir), 50f, dmg);
            p.transform.localScale = energyBallEffect.transform.localScale;

            yield return new WaitForSeconds(0.5f);
        }


        isActionDelay = false;
        leftShootEffect.SetActive(false);
        rightShootEffect.SetActive(false);
    }

    private void Skill2()
    {
        if (Input.GetButton("Fire3") && !SkillUI.cooldownState[2] && !GameManager.Instance.isCinematic)
        {
            if (isActionDelay)
                ActionEnd();

            SoundManager.Instance.SFXPlay("GunLoadSE");
            UI.Skill.GetCooldown(SkillUI.Type.Q_Skill, COOLDOWN_SKILL2);
            isActionDelay = true;
            actionCoroutine = StartCoroutine(Skill2Coroutine());
            anim.SetBool("Left Aim", true);
        }
    }

    IEnumerator Skill2Coroutine()
    {
        yield return new WaitForSeconds(0.5f);

        anim.SetBool("Left Aim", false);
        anim.SetTrigger("Left Blast Attack");
        leftShootEffect.SetActive(true);

        SoundManager.Instance.SFXPlay("ChargeShotSE");

        Vector3 dir = new Vector3(mouseHit.point.x, 0f, mouseHit.point.z) - leftShootEffect.transform.position;
        dir.y = 0f;
        ProjectileManager.Instance.CreateProjectile("Green", leftShootEffect.transform.position, Quaternion.LookRotation(dir), 40f, GetAttackDamage(DMG_SKILL2));
        for (int angle = 15; angle < 45; angle += 15)
        {
            ProjectileManager.Instance.CreateProjectile("Green", leftShootEffect.transform.position, Quaternion.LookRotation(dir) * Quaternion.Euler(Vector3.up * angle), 40f, GetAttackDamage(DMG_SKILL2));
            ProjectileManager.Instance.CreateProjectile("Green", leftShootEffect.transform.position, Quaternion.LookRotation(dir) * Quaternion.Euler(Vector3.up * -angle), 40f, GetAttackDamage(DMG_SKILL2));
        }

        yield return new WaitForSeconds(0.5f);
        isActionDelay = false;
        leftShootEffect.SetActive(false);
    }

    void DefenseSkill()
    {
        if (Input.GetButton("Fire4") && !SkillUI.cooldownState[3] && !isInvincible)
        {
            isInvincible = true;
            SoundManager.Instance.SFXPlay("BarrierSE");
            shieldEffect.gameObject.SetActive(true);
            UI.Skill.GetCooldown(SkillUI.Type.Defanse, COOLDOWN_DEFANSE);
            shieldColor.material.SetFloat("_Opacity", 0f);
            EffectManager.Instance.CreateEffect("CircleWave", transform.position + Vector3.up, Quaternion.identity, 1.0f);
            StartCoroutine(ShieldEnd(2.0f));
        }
    }

    IEnumerator ShieldEnd(float time)
    {
        float alpha = 0f;
        while (time > 0)
        {
            if (alpha < 0.3f)
            {
                alpha += 0.01f;
                shieldColor.material.SetFloat("_Opacity", alpha);
            }
            time -= Time.deltaTime;
            yield return null;
        }

        //종료
        isInvincible = false;
        alpha = 0.3f;
        while (alpha > 0)
        {
            shieldColor.material.SetFloat("_Opacity", alpha);
            alpha -= 0.01f;
            yield return null;
        }
        shieldEffect.gameObject.SetActive(false);
    }

    private bool ControlState()
    {
        return !isDead && !isFall && !GameManager.Instance.isGamePause && !GameManager.Instance.isShop && !GameManager.Instance.isCinematic;
    }

    void LateUpdate()
    {
        if (ControlState())
        {
            SetCursorPoint();
            BodySpin();
            Attack();
            Skill1();
            Skill2();
            DefenseSkill();
        }
    }

    private void FixedUpdate()
    {
        if (ControlState())
        {
            Move();
            WalkAnimation();
            InertiaMove();
            Fall();
        }
        else
        {
            if (isActionDelay)
                ActionEnd();
            velocity = Vector3.zero;
            inertiaVelocity = Vector3.zero;
            rigid.velocity = Vector3.zero;
            anim.SetFloat("Forward", 0f);
        }
    }
}
