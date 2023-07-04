using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using EnemyMain;
using System;
using Random = UnityEngine.Random;

public class Boss1 : Boss
{
    // 능력치 관련
    private const float MAX_HEALTH = 300f; // 최대 체력
    private const float MAX_MANA = 600f; // 최대 마나

    // 패턴 관련
    private const int MAX_PATTERN = 5; // 패턴 최대 수
    private const float FIRST_PATTERN_START_TIME = 1.5f; // 첫 패턴 시작 딜레이

    // 기타
    private const float AFTER_IMAGE_FADE_SPEED = 2f; // 잔상 사라지는 속도
    private const float DECREASE_SPAWN_HEALTH = 0.3f; // 몬스터 소환 체력 감소량
    
    private float spawnHealth = 0.6f; // 해당 체력(%) 도달 시 몬스터 소환
    private GameObject rangeObj;
    private Renderer afterImageRend; // 중심 잔상
    private LavaGround[] lavaGrounds; // 용암 바닥

    [SerializeField] private GameObject miniGolem;
    [SerializeField] private GameObject afterImage;
    [SerializeField] private GameObject bossPoint;
    [SerializeField] private ParticleSystem fireBreath;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private GameObject[] gatePoints;
    [SerializeField] private GateScript[] bossGates;

    protected override void Start()
    {
        base.Start();
        
        BossState = true;

        InitializePattern();
        InitializeHealth();
        InitializeComponents();

        StartCoroutine(PatternStart());
    }

    protected override void InitializePattern()
    {
        maxPattern = MAX_PATTERN;
        patternCount = new int[maxPattern];
        ultiPattern = UltimatePattern();

        Boss1 instance = this;
        PatternAction = new Func<IEnumerator>[]
        {
            () => instance.Pattern1(),
            () => instance.Pattern2(),
            () => instance.Pattern3(),
            () => instance.Pattern4(),
            () => instance.Pattern5(),
            () => instance.PatternLoadEnemy()
        };
        PatternStop = new Action[]
        {
            () => instance.Pattern1End(),
            () => instance.Pattern2End(),
            () => instance.Pattern3End(),
            () => instance.Pattern4End(),
            () => instance.Pattern5End(),
            () => instance.PatternLoadEnemyEnd()
        };
    }
    protected override void InitializeHealth()
    {
        maxHealth = MAX_HEALTH;
        health = maxHealth;
    }

    private void InitializeComponents()
    {
        afterImageRend = afterImage.GetComponentInChildren<Renderer>();
        lavaGrounds = FindObjectsOfType<LavaGround>();
    }


    IEnumerator PatternStart()
    {
        while (GameManager.Instance.isCinematic)
            yield return null;

        yield return new WaitForSeconds(FIRST_PATTERN_START_TIME);
        StartBossPattern();
    }

    protected override void Dead(Vector3 angle)
    {
        base.Dead(angle);
        StopAllCoroutines();
        PatternStop[applyPattern]();
        EnemyList.RemoveEnemys();
        StartCoroutine(DeadCoroutine());
    }

    IEnumerator DeadCoroutine()
    {
        anim.SetTrigger("isDie");
        GameManager.Instance.isCinematic = true;
        SoundManager.Instance.SFXPlay("Boss1DieSE");
        SoundManager.Instance.BgmStop(true);
        GameManager.CameraInstance.CamLock = false;
        GameManager.CameraInstance.FadeMove(transform.position);
        yield return new WaitForSeconds(1.0f);
        GameManager.CameraInstance.CameraNoise(0.1f, 0.1f, 0.5f);
        SoundManager.Instance.SFXPlay("GiantLandingSE");
        EffectManager.Instance.CreateEffect("ShockWave", transform.position - transform.forward * 2f, Quaternion.identity, 1.0f);
        yield return new WaitForSeconds(0.5f);

        EffectManager.Instance.CreateEffect("BossDeath", transform.position - transform.forward * 2f, Quaternion.identity, 4.0f);
        SoundManager.Instance.SFXPlay("BossDeathChargeSE");
        UI.instance.SetFilter(new Color(1, 1, 1, 0f), new Color(1, 1, 1, 0.9f), 2.0f);
        yield return new WaitForSeconds(2.0f);
        
        EffectManager.Instance.CreateEffect("BossExplosion", transform.position, Quaternion.identity, 1.0f);
        SoundManager.Instance.SFXStop("BossDeathChargeSE");
        SoundManager.Instance.SFXPlay("ExplosionSE");
        UI.instance.SetFilter(new Color(1, 1, 1, 1f), new Color(1, 1, 1, 0), 0.5f);
        GameManager.CameraInstance.CameraNoise(1.0f, 0.1f, 1.0f);
        meshRenderer.enabled = false;
        enemyCol.enabled = false;
        yield return new WaitForSeconds(2f);

        GameManager.CameraInstance.FadeMove(GameManager.PlayerInstance.transform.position);
        yield return new WaitForSeconds(1f);

        GameManager.Instance.isCinematic = false;
        GameManager.CameraInstance.CamLock = true;
        yield return new WaitForSeconds(2f);

        UI.instance.SetFilter(new Color(0, 0, 0, 0f), new Color(0, 0, 0, 1), 1.0f);
        yield return new WaitForSeconds(2f);

        GameManager.Instance.Stage++;
        GameManager.Instance.LoadLobbyScene();
    }

    private Vector3 Vector3RandomRange(Vector3 target, float distance)
    {
        Vector3 point = target;
        float randAngle = Random.Range(0f, 360f);

        int layerMask = 1 << LayerMask.NameToLayer("Terrain");
        Vector3 dic = Quaternion.Euler(Vector3.up * randAngle) * Vector3.forward;
        dic = dic.normalized;
        RaycastHit hit;
        if (Physics.Raycast(point, dic, out hit, distance, layerMask))
        {
            point = hit.point - (dic * 2f);
        }
        else
            point += dic * distance;

        return point;
    }

    private void ShowAfterImage()
    {
        afterImage.gameObject.SetActive(true);
        afterImage.transform.localScale = Vector3.one;
        afterImageRend.material.color = Color.white;
        StartCoroutine(AfterImageCoroutine());
    }

    IEnumerator AfterImageCoroutine()
    {
        float time = 0f;
        Color c_alpha = Color.white;
        c_alpha.a = 0;
        while (time < 1)
        {
            afterImage.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 2, time);
            afterImageRend.material.color = Color.Lerp(Color.white, c_alpha, time);
            time += Time.deltaTime * AFTER_IMAGE_FADE_SPEED;
            yield return null;
        }
        afterImage.gameObject.SetActive(false);
    }


    #region Pattern1Methods
    private void Pattern1End()
    {
        anim.SetBool("isHit", false);
        rigid.useGravity = true;
    }

    IEnumerator Pattern1()
    {
        Vector3 s_Point = Vector3RandomRange(transform.position, Random.Range(0f, 3.5f));
        Vector3 e_Point = s_Point + Vector3.up * 2f;

        StartCoroutine(LowJump(s_Point, e_Point));
        yield return new WaitForSeconds(0.8f);

        StartCoroutine(LowLanding(s_Point, e_Point));
        yield return new WaitForSeconds(2.0f);

        StartBossPattern();
    }

    // 이동과 동시에 낮은 점프
    IEnumerator LowJump(Vector3 s_Point, Vector3 e_Point)
    {
        EffectManager.Instance.CreateEffect("Dust", transform.position + Vector3.up * 0.5f, Quaternion.identity, 1.0f);
        SoundManager.Instance.SFXPlay("mob7atk");
        anim.SetBool("isHit", true);
        rigid.useGravity = false;

        float time = 0f;

        while (time < 1f)
        {
            transform.position = Vector3.Lerp(transform.position, e_Point, 0.1f);
            time += Time.fixedDeltaTime * 2f;
            yield return new WaitForFixedUpdate();
        }
        transform.position = e_Point;
    }

    // 착지 후 탄막 발사
    IEnumerator LowLanding(Vector3 s_Point, Vector3 e_Point)
    {
        float time = 1;
        while (time > 0f)
        {
            transform.position = Vector3.Lerp(s_Point, e_Point, time);
            time -= Time.fixedDeltaTime * 10f;
            yield return new WaitForFixedUpdate();
        }
        transform.position = s_Point;

        rigid.useGravity = true; // 중력 초기화
        anim.SetBool("isHit", false); // 모션 재생
        SoundManager.Instance.SFXPlay("HandGun3"); // 효과음 재생
        GameManager.CameraInstance.CameraNoise(0.2f, 0.1f, 0.5f); // 카메라 흔들림
        EffectManager.Instance.CreateEffect("ShockWave", s_Point, Quaternion.identity, 1.5f); // 기류 이펙트

        ShootCircleBullet(s_Point);
    }

    // 원형 탄막 발사
    private void ShootCircleBullet(Vector3 point)
    {
        float randAngle = Random.Range(0f, 10f);
        for (int i = 0; i < 40; i++)
            ProjectileManager.Instance.CreateProjectile("Red", point + Vector3.up, Quaternion.Euler(Vector3.up * (i * 9 + randAngle)), 10f, 1, ProjectileManager.OwnerType.Enemy);
    }
    #endregion

    #region Pattern2Methods
    private void Pattern2End()
    {
        if (rangeObj != null)
            EffectManager.Instance.PollingDestroy(rangeObj);
        anim.SetBool("isHit", false);
        GetTargetAngle(false);
        rigid.useGravity = true;
        Collision = true;
        anim.speed = 1.0f;
        UI.instance.SetFilter(new Color(0, 0, 0, 0f), new Color(0, 0, 0, 0f), 0f);
    }
    IEnumerator Pattern2()
    {
        Vector3 point = Vector3RandomRange(GameManager.PlayerInstance.transform.position, Random.Range(0f, 3f));
        rangeObj = EffectManager.Instance.CreateEffect("CircleRange", point, Quaternion.identity);
        UI.instance.SetFilter(new Color(0, 0, 0, 0f), new Color(0, 0, 0, 0.25f), 0.5f);
        SoundManager.Instance.SFXPlay("MobSE");
        anim.speed = 0.8f;
        Collision = false;
        rangeObj.transform.localScale = Vector3.one * 3f;
        EffectManager.Instance.CreateEffect("Dust", transform.position + Vector3.up * 0.5f, Quaternion.identity, 1.0f);
        GameManager.CameraInstance.SetCameraRange(20f, 0.5f);
        anim.SetBool("isHit", true);
        rigid.useGravity = false;
        GetTargetAngle(true, rangeObj);

        float isHeight = 0.5f;
        while (isHeight > 0f)
        {
            isHeight -= 0.02f;
            transform.position += Vector3.up * isHeight * 0.4f;
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitForSeconds(0.5f);
        
        Vector3 startVec = transform.position;
        Vector3 endVec = rangeObj.transform.position;
        float time = 0f;
        float objTime = 0f;
        while (time < 1f)
        {
            if (objTime <= 0f)
            {
                GameObject moveObj = EffectManager.Instance.CreateEffect("Boss1AfterImage", transform.position, transform.rotation, 1.1f);
                EffectManager.Instance.GetFadeObject(moveObj, Color.red, new Color(1, 0, 0, 0));
                objTime += 0.03f;
            }
            objTime -= Time.deltaTime;
            transform.position = Vector3.Lerp(startVec, endVec, time);
            time += 0.2f;
            yield return new WaitForFixedUpdate();
        }
        transform.position = endVec;
        SoundManager.Instance.SFXPlay("StompExplosionSE");
        EffectManager.Instance.CreateEffect("FireExplode", endVec, Quaternion.identity, 1.5f);
        GetTargetAngle(false);
        EffectManager.Instance.PollingDestroy(rangeObj);
        rigid.useGravity = true;
        GameManager.CameraInstance.SetCameraRange(CameraScript.BASIC_RANGE, 0.5f);
        GameManager.CameraInstance.CameraNoise(0.5f, 0.1f, 0.5f);
        if (Vector3.Distance(GameManager.PlayerInstance.transform.position, endVec) <= 4f)
        {
            Vector3 _velecity = (GameManager.PlayerInstance.transform.position - endVec).normalized * 32f;
            GameManager.PlayerInstance.GetReceiveDamage(2, _velecity);
        }
        anim.SetBool("isHit", false);
        Collision = true;
        anim.speed = 1.0f;
        UI.instance.SetFilter(new Color(1, 0, 0, 0.5f), new Color(1, 0, 0, 0f), 0.25f);
        yield return new WaitForSeconds(1.0f);
        StartBossPattern(1);
    }

    IEnumerator HighJump()
    {
        SoundManager.Instance.SFXPlay("MobSE");
        EffectManager.Instance.CreateEffect("Dust", transform.position + Vector3.up * 0.5f, Quaternion.identity, 1.0f);
        GameManager.CameraInstance.SetCameraRange(20f, 0.5f);
        anim.SetBool("isHit", true);
        rigid.useGravity = false;

        float isHeight = 0.5f;
        while (isHeight > 0f)
        {
            isHeight -= 0.02f;
            transform.position += Vector3.up * isHeight * 0.4f;
            yield return new WaitForFixedUpdate();
        }
    }
    #endregion

    #region Pattern3Methods
    private void Pattern3End()
    {
        if (rangeObj != null)
            EffectManager.Instance.PollingDestroy(rangeObj);
        anim.SetBool("isWalk", false);
        GetTargetAngle(false);
        anim.speed = 1.0f;
        UI.instance.SetFilter(new Color(0, 0, 0, 0f), new Color(0, 0, 0, 0f), 0f);
        lineRenderer.SetPosition(0, Vector3.zero);
        lineRenderer.SetPosition(1, Vector3.zero);
        HitDamage = BASIC_HIT_DAMAGE;
        KnockbackSpeed = BASIC_KNOCKBACK_SPEED;
    }
    IEnumerator Pattern3()
    {
        int layerMask = 1 << LayerMask.NameToLayer("Terrain");
        float time = 0f;

        GameManager.CameraInstance.CameraNoise(0.1f, 0.1f, 0.25f);
        SoundManager.Instance.SFXPlay("BossDashReadySE");
        GetTargetAngle(true);
        lineRenderer.SetPosition(0, transform.position + Vector3.up);
        lineRenderer.SetPosition(1, GameManager.PlayerInstance.transform.position + Vector3.up);
        EffectManager.Instance.CreateEffect("YellowLight", transform.position + Vector3.up, Quaternion.identity, 1.5f);
        rangeObj = EffectManager.Instance.CreateEffect("Target", transform.position, Quaternion.identity);

        UI.instance.SetFilter(new Color(0, 0, 0, 0f), new Color(0, 0, 0, 0.2f), 0.25f);
        while (time < 1f)
        {
            Vector3 dic = (GameManager.PlayerInstance.transform.position - transform.position);
            dic.y = 0f;
            dic = dic.normalized;
            RaycastHit hit;
            Vector3 point1 = transform.position + (Vector3.up * (enemyCol.height * 0.5f));
            Vector3 point2 = transform.position - (Vector3.up * (enemyCol.height * 0.5f));
            if (Physics.CapsuleCast(point1, point2, enemyCol.radius, dic, out hit, Mathf.Infinity, layerMask))
            {
                float targetDistance = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(hit.point.x, 0, hit.point.z));
                Vector3 targetPoint = transform.position + (dic * targetDistance);
                targetPoint.y = 0.1f;
                rangeObj.transform.position = targetPoint;
                lineRenderer.SetPosition(1, targetPoint);
            }
            time += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        UI.instance.SetFilter(new Color(1, 0, 0, 0.5f), new Color(0, 0, 0, 0.2f), 0.25f);
        SoundManager.Instance.SFXPlay("WinkSE");
        GetTargetAngle(true, rangeObj);
        yield return new WaitForSeconds(0.5f);
        EffectManager.Instance.PollingDestroy(rangeObj);
        lineRenderer.SetPosition(0, Vector3.zero);
        lineRenderer.SetPosition(1, Vector3.zero);
        GetTargetAngle(false);
        anim.SetBool("isWalk", true);
        anim.speed = 5f;
        EffectManager.Instance.CreateEffect("ShockWave", transform.position + Vector3.up * 0.5f, Quaternion.identity, 1.0f);
        SoundManager.Instance.SFXPlay("BossDashSE");
        UI.instance.SetFilter(new Color(0, 0, 0, 0.2f), new Color(0, 0, 0, 0f), 0.25f);

        HitDamage = 2;
        KnockbackSpeed = 40f;
        float distance = 0f;
        while (true)
        {
            Vector3 dic = transform.forward;
            RaycastHit hit;
            Vector3 point1 = transform.position + (Vector3.up * (enemyCol.height * 0.5f));
            Vector3 point2 = transform.position - (Vector3.up * (enemyCol.height * 0.5f));
            if (Physics.CapsuleCast(point1, point2, enemyCol.radius, dic, out hit, 1f, layerMask))
            {
                float targetDistance = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(hit.point.x, 0, hit.point.z));
                transform.position += dic * hit.distance;
                break;
            }
            else
                transform.position += dic;

            // 잔상 생성
            distance -= 1f;
            if (distance <= 0)
            {
                GameObject moveObj = EffectManager.Instance.CreateEffect("Boss1AfterImage", transform.position, transform.rotation, 1.1f);
                EffectManager.Instance.GetFadeObject(moveObj, new Color(1, 0, 0, 0.5f), new Color(1, 0, 0, 0));
                distance += 3f;
            }
            yield return new WaitForFixedUpdate();
        }
        HitDamage = BASIC_HIT_DAMAGE;
        KnockbackSpeed = BASIC_KNOCKBACK_SPEED;
        anim.SetBool("isWalk", false);
        anim.speed = 1f;
        SoundManager.Instance.SFXPlay("StompExplosionSE");
        EffectManager.Instance.CreateEffect("FireExplode", transform.position, Quaternion.identity, 1.5f);
        GameManager.CameraInstance.CameraNoise(0.5f, 0.1f, 0.5f);
        for (int i = 0; i < 20; i++)
            ProjectileManager.Instance.CreateProjectile("Red", transform.position + Vector3.up, Quaternion.Euler(Vector3.up * (i * 18)), 10f, 1, ProjectileManager.OwnerType.Enemy);

        time = 1f;
        while(time > 0f)
        {
            Vector3 point = transform.position;
            Vector3 dic = transform.forward.normalized * time * 0.1f;
            point -= dic;

            Vector3 startPoint = point + Vector3.up * (enemyCol.height * 0.5f);
            Vector3 endPoint = point - Vector3.up * (enemyCol.height * 0.5f);
            transform.position = point;

            time -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitForSeconds(1.0f);
        StartBossPattern(2);
    }
    #endregion

    #region Pattern4Methods
    private void Pattern4End()
    {
        anim.SetLayerWeight(1, 0);
        anim.SetBool("isWalk", false);
        nav.ResetPath();
        nav.enabled = false;
    }
    IEnumerator Pattern4()
    {
        anim.SetLayerWeight(1, 1);
        anim.SetBool("isWalk", true);
        GetTargetAngle(true);

        float attackTime = 1.5f;
        float time = Random.Range(5f, 8f);
        bool animHand = true;


        nav.enabled = true;
        while (0f < time)
        {
            // 이동 공격
            if (attackTime <= 0f)
            {
                Vector3 point = transform.position + Vector3.up;
                Vector3 t_point = GameManager.PlayerInstance.transform.position;
                t_point.y = point.y;

                EffectManager.Instance.CreateEffect("CircleWave", transform.position + Vector3.up * 2.5f, Quaternion.identity, 1.0f);
                SoundManager.Instance.SFXPlay("HandGun6");
                if (animHand)
                    anim.SetTrigger("isUpperAttack01");
                else
                    anim.SetTrigger("isUpperAttack02");
                animHand = !animHand;
                attackTime = Random.Range(0.5f, 1f);

                GameManager.CameraInstance.CameraNoise(0.05f, 0.1f, 0.25f);
                Quaternion angle = Quaternion.LookRotation(t_point - point) * Quaternion.Euler(Vector3.up * Random.Range(-25f, 25f));
                ProjectileManager.Instance.CreateProjectile("Red", point, angle, 10f, 1, ProjectileManager.OwnerType.Enemy);
                for (int i = 1; i < 3; i++)
                {
                    ProjectileManager.Instance.CreateProjectile("Red", point, angle * Quaternion.Euler(Vector3.up * (-i * 15f)), 10f, 1, ProjectileManager.OwnerType.Enemy);
                    ProjectileManager.Instance.CreateProjectile("Red", point, angle * Quaternion.Euler(Vector3.up * (i * 15f)), 10f, 1, ProjectileManager.OwnerType.Enemy);
                }
            }
            else
                attackTime -= Time.deltaTime;


            nav.SetDestination(GameManager.PlayerInstance.transform.position);
            time -= Time.deltaTime;
            yield return null;
        }
        nav.ResetPath();
        nav.enabled = false;
        anim.SetBool("isWalk", false);
        GetTargetAngle(false);

        time = 1f;
        while (time <= 0)
        {
            anim.SetLayerWeight(1, time);
            time -= Time.deltaTime * 2f;
        }
        anim.SetLayerWeight(1, 0);
        yield return new WaitForSeconds(1.0f);
        StartBossPattern(3);
    }
    #endregion

    #region Pattern5Methods
    private void Pattern5End()
    {
        var module = fireBreath.emission;

        UI.instance.SetFilter(new Color(0, 0, 0, 0f), new Color(0, 0, 0, 0f), 0f);
        GetTargetAngle(false);
        SoundManager.Instance.SFXStop("FireBreathSE");
        anim.SetBool("isHit", false);
        module.rateOverTime = 0;
        anim.speed = 1f;
        isSpinSpeed = BASIC_SPINSPEED;
    }
    IEnumerator Pattern5()
    {
        UI.instance.SetFilter(new Color(0, 0, 0, 0f), new Color(0, 0, 0, 0.25f), 0.5f);
        var module = fireBreath.emission;
        SoundManager.Instance.SFXPlay("BreathChargeSE");
        anim.SetBool("isHit", true);
        GetTargetAngle(true);
        yield return new WaitForSeconds(0.5f);
        isSpinSpeed = 3f;
        yield return new WaitForSeconds(0.5f);
        UI.instance.SetFilter(new Color(1, 0, 0, 0.5f), new Color(0, 0, 0, 0), 0.5f);
        module.rateOverTime = 30;
        SoundManager.Instance.SFXPlay("FireBreathSE");
        EffectManager.Instance.CreateEffect("FireExplode", transform.position + Vector3.up, Quaternion.identity, 1.5f);
        anim.speed = 0f;
        yield return new WaitForSeconds(2.5f);
        SoundManager.Instance.SFXStop("FireBreathSE", true);
        anim.speed = 1f;
        isSpinSpeed = BASIC_SPINSPEED;
        GetTargetAngle(false);
        module.rateOverTime = 0;
        yield return new WaitForSeconds(0.25f);
        anim.SetBool("isHit", false);
        yield return new WaitForSeconds(0.5f);
        StartBossPattern(4);
    }
    #endregion

    #region UltimateMethods
    protected override IEnumerator UltimatePattern()
    {
        Mana = 0f;
        isUltimate = true;
        isInvincible = true;
        EnemyList.RemoveEnemys();
        ShowAfterImage();
        UI.instance.SetFilter(new Color(1, 1, 1, 0.5f), new Color(1, 1, 1, 0), 0.5f);
        SoundManager.Instance.SFXPlay("FullManaSE");
        EffectManager.Instance.CreateEffect("Ultimate", transform.position + Vector3.up, Quaternion.identity, 1.5f);
        EffectManager.Instance.CreateEffect("YellowLight", transform.position + Vector3.up, Quaternion.identity, 1.5f);

        yield return new WaitForSeconds(1f);
        // 시야 확장
        GameManager.CameraInstance.SetCameraRange(25f, 1.0f);

        yield return new WaitForSeconds(1f);

        // 보스 점프
        SoundManager.Instance.SFXPlay("MobSE");
        Vector3 startPoint = transform.position;
        Vector3 endPoint = UltimatePoint.transform.position;
        Vector3 centerPoint = (startPoint + endPoint) * 0.5f;
        centerPoint.y += 10f;

        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.Euler(Vector3.up * 180);

        EffectManager.Instance.CreateEffect("Dust", startPoint + Vector3.up * 0.5f, Quaternion.identity, 1.0f);
        anim.SetBool("isHit", true);
        float time = 0f;
        while (time < 1f)
        {
            transform.position =  Mathf.Pow(1 - time, 2) * startPoint + 2 * (1 - time) * time * centerPoint + Mathf.Pow(time, 2) * endPoint;
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, time);
            time += Time.deltaTime * 1f;
            yield return null;
        }
        anim.SetBool("isHit", false);

        transform.position = endPoint;
        EffectManager.Instance.CreateEffect("ShockWave", endPoint, Quaternion.identity, 1.5f);
        SoundManager.Instance.SFXPlay("GiantLandingSE");
        GameManager.CameraInstance.CameraNoise(0.2f, 0.1f, 0.5f);

        yield return new WaitForSeconds(0.5f);

        // 불 지형 활성화
        UI.instance.SetFilter(new Color(1, 0, 0, 0f), new Color(1, 0, 0, 0.2f), 0.5f);
        SoundManager.Instance.SFXPlay("FireGroundOnSE");
        foreach (var lava in lavaGrounds)
            lava.SetLight(true);
        yield return new WaitForSeconds(0.5f);

        anim.SetTrigger("attack02");
        yield return new WaitForSeconds(0.5f);

        // 불덩이 생성
        SoundManager.Instance.SFXPlay("StompExplosionSE");
        foreach (var lava in lavaGrounds)
        {
            EffectManager.Instance.CreateEffect("UpperFire", lava.transform.position, Quaternion.identity, 2f);
            if (Vector3.Distance(GameManager.PlayerInstance.transform.position, lava.transform.position) <= 3f)
            {
                Vector3 _velocity = (GameManager.PlayerInstance.transform.position - lava.transform.position).normalized * 32f;
                GameManager.PlayerInstance.GetReceiveDamage(2, _velocity);
            }
        }

        UI.instance.SetFilter(new Color(1, 0.5f, 0, 0.4f), new Color(1, 0, 0, 0.2f), 0.5f);
        GameManager.CameraInstance.CameraNoise(1f, 0.1f, 0.5f);
        yield return new WaitForSeconds(1.0f);

        time = 1f;
        // 불 떨어뜨리기
        while(time > -0.4f)
        {
            Vector3 point = Vector3RandomRange(GameManager.PlayerInstance.transform.position, Random.Range(0f, 8f));
            StartCoroutine(FailFireCoroutine(point, time >= 0.2f));
            yield return new WaitForSeconds(Mathf.Max(0.1f, time));
            time -= 0.05f;
        }
        yield return new WaitForSeconds(1.5f);
        UI.instance.SetFilter(new Color(1, 0, 0, 0.2f), new Color(1, 0, 0, 0f), 0.5f);
        foreach (var lava in lavaGrounds)
            lava.SetLight(false);
        yield return new WaitForSeconds(1f);

        // 필드로 점프
        GameManager.CameraInstance.SetCameraRange(CameraScript.BASIC_RANGE, 1.0f);
        SoundManager.Instance.SFXPlay("MobSE");
        startPoint = transform.position;
        endPoint = bossPoint.transform.position;
        centerPoint = (startPoint + endPoint) * 0.5f;
        centerPoint.y += 10f;
        time = 0f;
        anim.SetBool("isHit", true);
        EffectManager.Instance.CreateEffect("Dust", startPoint + Vector3.up * 0.5f, Quaternion.identity, 1.0f);
        while (time < 1f)
        {
            transform.position = Mathf.Pow(1 - time, 2) * startPoint + 2 * (1 - time) * time * centerPoint + Mathf.Pow(time, 2) * endPoint;

            time += Time.deltaTime * 1f;
            yield return null;
        }
        anim.SetBool("isHit", false);
        EffectManager.Instance.CreateEffect("ShockWave", endPoint, Quaternion.identity, 1.5f);
        SoundManager.Instance.SFXPlay("GiantLandingSE");
        GameManager.CameraInstance.CameraNoise(0.2f, 0.1f, 0.5f);

        // 종료
        isInvincible = false;
        isUltimate = false;
        StartCoroutine(ManaRegen());
        yield return new WaitForSeconds(1.5f);
        StartBossPattern(0);
    }
    IEnumerator FailFireCoroutine(Vector3 point, bool animState)
    {
        GameObject rangeObj = EffectManager.Instance.CreateEffect("CircleRange", point, Quaternion.identity);
        rangeObj.transform.localScale = Vector3.one * 2f;
        yield return new WaitForSeconds(0.5f);

        if (animState)
            anim.SetTrigger("attack02");
        EffectManager.Instance.CreateEffect("FailFire", point, Quaternion.identity, 1f);
        yield return new WaitForSeconds(0.5f);

        EffectManager.Instance.CreateEffect("FireExplode", point + Vector3.up, Quaternion.identity, 1.5f);
        EffectManager.Instance.PollingDestroy(rangeObj);
        SoundManager.Instance.SFXPlay("StompExplosionSE");
        //UI.instance.SetFilter(new Color(1, 0.5f, 0, 0.4f), new Color(1, 0, 0, 0.2f), 0.25f);
        GameManager.CameraInstance.CameraNoise(0.2f, 0.1f, 0.25f);

        if (Vector3.Distance(GameManager.PlayerInstance.transform.position, point) <= 3f)
        {
            Vector3 _velocity = (GameManager.PlayerInstance.transform.position - point).normalized * 16f;
            GameManager.PlayerInstance.GetReceiveDamage(2, _velocity);
        }
    }
    #endregion

    #region LoadEnemyMethods
    private void PatternLoadEnemyEnd()
    {
        anim.SetBool("isVictory", false);
        for (int i = 0; i < bossGates.Length; i++)
            bossGates[i].ControlGate(false);
    }

    IEnumerator PatternLoadEnemy()
    {
        anim.SetBool("isVictory", true);
        for (int i = 0; i < bossGates.Length; i++)
            bossGates[i].ControlGate(true);

        yield return new WaitForSeconds(1f);
        SoundManager.Instance.SFXPlay("Boss1Roar");
        GameManager.CameraInstance.CameraNoise(0.3f, 0.1f, 1.2f);
        yield return new WaitForSeconds(0.5f);


        for (int i = 0; i < 5; i++)
        {
            Vector3 spawnPoint;
            if (Random.Range(0, 2) == 0)
                spawnPoint = gatePoints[0].transform.position;
            else
                spawnPoint = gatePoints[1].transform.position;

            Instantiate(miniGolem, spawnPoint, Quaternion.identity);
            yield return new WaitForSeconds(Random.Range(0.2f, 0.5f));
        }

        for (int i = 0; i < bossGates.Length; i++)
            bossGates[i].ControlGate(false);

        anim.SetBool("isVictory", false);
        yield return new WaitForSeconds(1.0f);
        StartBossPattern(6);
    }
    #endregion

    //랜덤한 패턴 실행
    private void StartBossPattern(int except = -1)
    {
        if (isMana < MAX_MANA)
        {
            if (health / maxHealth * 1 > spawnHealth)
            {
                int target = SelectPattern(except);
                applyPattern = target;
                StartCoroutine(PatternAction[applyPattern]());
            }
            else
            {
                spawnHealth -= DECREASE_SPAWN_HEALTH;
                applyPattern = 5;
                StartCoroutine(PatternAction[applyPattern]());
            }
        }
        else
            StartCoroutine(UltimatePattern());
    }
}
