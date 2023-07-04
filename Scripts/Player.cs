using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IObjectInfo
{
    // �⺻ �ɷ�ġ
    public static readonly int BASIC_MOVE_SPEED = 10; // �ʱ� �̵� �ӵ�
    public static readonly int BASIC_HEALTH = 3; // �ʱ� ü��
    public static readonly float BASIC_ATTACK_SPEED = 1f; // �ʱ� ���� �ӵ�

    // ������
    private const float DMG_ATTACK = 1f; // ��Ÿ ������
    private const float DMG_SKILL1 = 4f; // ������ ������
    private const float DMG_SKILL2 = 4f; // Q��ų ������

    // �޴� ������
    private const int RECEIVE_DMG_FALL = 2; // �߶� �� ������
    
    // ��ٿ�
    private const float COOLDOWN_SKILL2 = 10f; // Q��ų ��ٿ�
    private const float COOLDOWN_DEFANSE = 15f; // ���� ��ų ��ٿ�

    // ����ġ
    private const float WEIGHT_DAMAGE = 0.05f; // ���ݷ� ���׷��̵� ����ġ
    private const float WEIGHT_ATTACK_SPEED = 0.02f; // ���� �ӵ� ���׷��̵� ����ġ
    private const float WEIGHT_MOVE_SPEED = 0.2f; // �̵� �ӵ� ���׷��̵� ����ġ

    // �ӵ� ����ġ
    private const float VELECITY_DECREASE = 0.1f; // �⺻ �ӵ� ����ġ
    private const float INERTIA_VELECITY_DECREASE = 0.8f; // �ܺ� �ӵ� ����ġ

    // �ǰ� �� ���� ����
    private const float DAMAGE_INVINCIBLE_TIME = 1f; // �ǰ� �� ���� �ð�
    private const float DAMAGE_BLINK_TIME = 0.05f; // �ǰ� �� ���� �����̴� ����

    // �߶� �� ���� ���� �ð�
    private const float FALL_END_TIME = 1f;

    // ���� ȸ�� �ӵ�
    private const float WHEEL_SPIN_SPEED = 0.1f;

    // ����
    private Rigidbody rigid;
    private Vector3 velocity; // �÷��̾� ���� �⺻ �ӵ�
    private Vector3 inertiaVelocity; // �ܺ� �ӵ�

    private int isMaxHealth; // �ִ� ü��
    private int isHealth; // ���� ü��
    private float isMoveSpeed = BASIC_MOVE_SPEED; // ���� �̵� �ӵ�
    private float isAttackSpeed = BASIC_ATTACK_SPEED; // ���� ���� �ӵ�

    private bool isActionDelay = false; // �ൿ ����
    private bool isFall = false; // �߶� ����
    private bool isDamaged = false; // ������ ���� ����
    private bool isInvincible = false; // ���� ����
    public bool isDead { get; set; } // ���� ����

    private Animator anim;
    private Renderer shieldColor; // ���� ������
    private RaycastHit mouseHit; // ���콺 ��ġ
    private GameObject isRespawnPoint; // �߶� �� �̵��Ǵ� ����
    private GameObject mouseObject; // Ŀ�� ��ġ ��⸦ ���� ���� ������Ʈ

    private Coroutine actionCoroutine;

    [SerializeField] private Renderer playerColor;
    [SerializeField] private Transform upperTransform;
    [SerializeField] private Transform underTransform;
    [SerializeField] private GameObject leftShootEffect;
    [SerializeField] private GameObject rightShootEffect;
    [SerializeField] private GameObject energyBallEffect;
    [SerializeField] private GameObject shieldEffect;

    IObjectInfo iDamageConponent { get; set; } // ������ �������̽�

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

    // UI �ʱ�ȭ �Ϸ� �� ����
    IEnumerator SetPlayerData()
    {
        yield return null;
        MaxHealth = GameManager.Instance.isHealthUpgrade + BASIC_HEALTH;
        Health = MaxHealth * 4;
        MoveSpeed = BASIC_MOVE_SPEED + (GameManager.Instance.isMoveSpeedUpgrade * WEIGHT_MOVE_SPEED);
        AttackSpeed = BASIC_ATTACK_SPEED - (GameManager.Instance.isAtkSpeedUpgrade * WEIGHT_ATTACK_SPEED);
    }

    // �ִ� ������
    private float GetAttackDamage(float _damage)
    {
        return _damage * (1 + (GameManager.Instance.isDamageUpgrade * WEIGHT_DAMAGE));
    }

    //��� �ൿ ����
    private void ActionEnd()
    {
        // ���� �������� �ൿ �ڷ�ƾ ����
        if (actionCoroutine != null)
            StopCoroutine(actionCoroutine);
        actionCoroutine = null;

        // �ൿ ��Ȱ��ȭ
        isActionDelay = false;

        // ���� ���� ����
        SoundManager.Instance.SFXStop("EnergyChargeSE");

        // ��� ����
        anim.SetBool("Right Rapid Attack", false);
        anim.SetBool("Both Aim", false);
        anim.SetBool("Left Aim", false);

        // ������Ʈ ��Ȱ��ȭ
        energyBallEffect.SetActive(false);
        leftShootEffect.SetActive(false);
        rightShootEffect.SetActive(false);
    }

    // ������ ����
    public void GetReceiveDamage(float damage, Vector3 angle)
    {
        if (!isInvincible && !isDamaged && !isDead && !isFall && !GameManager.Instance.isCinematic)
        {
            // �˹�
            inertiaVelocity = angle;

            // �ǰ� ���� ���
            SoundManager.Instance.SFXPlay("PlayerHitSE");

            // �κ񿡼� ���ظ� ���� ����
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

    // ���� ����
    IEnumerator DeadCoroutine()
    {
        isDead = true;

        // ��� �ൿ ����
        ActionEnd();

        // ���� ��� ���
        anim.SetTrigger("Die");

        // �ǰ� ����Ʈ
        EffectManager.Instance.CreateEffect("PlayerHit", transform.position + Vector3.up, Quaternion.identity, 1.0f);

        // ���� ����
        SoundManager.Instance.SFXPlay("PlayerBreakdownSE");
        SoundManager.Instance.BgmStop(false);

        // �÷��̾� �� ���� ������ ����
        float time = 0f;
        while (time < 1f)
        {
            playerColor.material.color = Color.Lerp(Color.white, Color.red, time);
            time += Time.deltaTime;
            yield return null;
        }
        playerColor.material.color = Color.red;
        yield return new WaitForSeconds(0.5f);

        // �÷��̾� ����
        PlayerExplosion();
    }

    private void PlayerExplosion()
    {
        // ���� ����
        SoundManager.Instance.SFXPlay("ExplosionSE");

        // ���� ����Ʈ
        EffectManager.Instance.CreateEffect("Explode", transform.position + Vector3.up, Quaternion.identity, 3.0f);

        // ���� ����
        UI.instance.SetFilter(new Color(1, 1, 1, 0.5f), new Color(1, 1, 1, 0), 0.5f);

        // ī�޶� ��鸲 ����
        GameManager.CameraInstance.CameraNoise(0.2f, 0.1f, 0.5f);

        // ���ӿ��� UI ���̱�
        UI.GameOver.ShowGameOverUI();
        gameObject.SetActive(false);
    }

    // �ǰ� �� �Ͻ��� ����
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

    // Ŀ�� ��ġ ����
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

        // Ű���� �̵�
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

    // �̵� ��� ���
    private void WalkAnimation()
    {
        // ���� �ӵ� ���
        if (velocity.magnitude > 0f)
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(velocity), WHEEL_SPIN_SPEED);

        if (isMoveSpeed > 0)
            anim.SetFloat("Forward", velocity.magnitude / isMoveSpeed);
        else
            anim.SetFloat("Forward", 0f);
    }

    // �ܺ� ���� �ӵ�
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
        // ȸ��
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

            // ��� �ൿ ����
            ActionEnd();

            // ��� ���
            anim.SetTrigger("Take Damage");

            // ���� ���
            SoundManager.Instance.SFXPlay("FallSE");

            // �ӵ� �ʱ�ȭ
            rigid.velocity = Vector3.zero;
            StartCoroutine(FallCoroutine());
        }
    }

    IEnumerator FallCoroutine()
    {
        // �߶� ����
        float time = 0f;
        while(time < 1f)
        {
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, time);
            time += Time.deltaTime * 2f;
            yield return null;
        }
        transform.localScale = Vector3.zero;
        yield return new WaitForSeconds(FALL_END_TIME);

        // ũ�� ����
        transform.localScale = Vector3.one;

        // ������ �������� �̵�
        transform.position = RespawnPoint.transform.position;

        // �߶� ������
        GetReceiveDamage(RECEIVE_DMG_FALL, Vector3.zero);

        // �߶� ����
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

            //�� ����Ʈ
            leftShootEffect.SetActive(true);
            rightShootEffect.SetActive(true);

            //ȿ����
            SoundManager.Instance.SFXPlay("ChargeShotSE");

            //�߻� ����Ʈ
            EffectManager.Instance.CreateEffect("BlueExplode", energyBallEffect.transform.position, Quaternion.identity, 1.5f);

            float dmg = GetAttackDamage(DMG_SKILL1 * chargeDamage);

            //ź�� ���� �� ũ�� ����
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

        //����
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
