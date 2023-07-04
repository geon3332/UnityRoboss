using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using EnemyMain;

public class MiniGolem : Enemy
{
    private const float ATTACK_DELAY = 0.7f;
    private const float DEAD_KNOCKBACK_SPEED = 10f;
    private const float DEAD_DELAY = 1.5f;
    
    private int actionCount = 0;
    private float isMoveSpeed = 3f; // 이동 속도
    private bool isAction = false; // 행동 상태

    public bool Action { get; set; }

    protected override void Start()
    {
        base.Start();

        JumpReady();
    }

    #region SpawnMethod
    void JumpReady()
    {
        anim.SetBool("isHit", true);
        isAction = true;
        rigid.useGravity = false;
        enemyCol.enabled = false;
        SoundManager.Instance.SFXPlay("DrumGunSE");

        StartCoroutine(JumpAction());
    }

    IEnumerator JumpAction()
    {
        Vector3 startPoint = transform.position;
        Vector3 endPoint = new Vector3(Random.Range(-10f, 10f), 0f, Random.Range(-5f, 5f));
        Vector3 centerPoint = (startPoint + endPoint) * 0.5f;
        GameObject rangeObj = EffectManager.Instance.CreateEffect("CircleRange", endPoint, Quaternion.identity, 1.5f);
        rangeObj.transform.localScale = Vector3.one * 1.5f;
        centerPoint.y += 5f;

        transform.rotation = Quaternion.LookRotation(endPoint - startPoint);

        float time = 0f;
        while (time < 1)
        {
            transform.position = Mathf.Pow(1 - time, 2) * startPoint + 2 * (1 - time) * time * centerPoint + Mathf.Pow(time, 2) * endPoint;
            time += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        transform.position = endPoint;
        EffectManager.Instance.PollingDestroy(rangeObj);

        StartCoroutine(Landing());
    }

    IEnumerator Landing()
    {
        SoundManager.Instance.SFXPlay("BulletDestroySE");
        EffectManager.Instance.CreateEffect("ShockWave", transform.position, Quaternion.identity, 1.0f);
        enemyCol.enabled = true;
        rigid.useGravity = true;
        nav.enabled = true;
        yield return new WaitForSeconds(0.5f);

        isAction = false;
        anim.SetBool("isHit", false);
    }
    #endregion

    private void FixedUpdate()
    {
        if (!isDead)
        {
            actionCount++;
            Move();

            StartAction();
        }
    }

    //1초마다 행동 명령
    private void StartAction()
    {
        if (actionCount >= 50)
        {
            if (!isAction)
            {
                Attack();
            }
            actionCount = 0;
        }
    }

    private void Move()
    {
        anim.SetBool("isWalk", !isAction);
        if (!isAction)
            nav.SetDestination(GameManager.PlayerInstance.transform.position);
        else if (nav.enabled)
            nav.ResetPath();
    }

    private void Attack()
    {
        if (Random.Range(0, 2) == 0)
        {
            isAction = true;
            StartCoroutine(AttackCoroutine());
        }
    }

    IEnumerator AttackCoroutine()
    {
        Quaternion angle = transform.rotation;
        Vector3 target = GameManager.PlayerInstance.transform.position;
        float time = 0f;
        target.y = transform.position.y;
        anim.SetTrigger("attack02");
        while(time < 1)
        {
            transform.rotation = Quaternion.Lerp(angle, Quaternion.LookRotation(target - transform.position), time);
            time += Time.deltaTime * 2f;
            yield return null;
        }
        transform.rotation = Quaternion.LookRotation(target - transform.position);

        SoundManager.Instance.SFXPlay("RockBulletSE");
        EffectManager.Instance.CreateEffect("Guard", transform.position + Vector3.up + transform.forward, transform.rotation, 1f);
        ProjectileManager.Instance.CreateProjectile("Red", transform.position + Vector3.up, transform.rotation, 20f, 2, ProjectileManager.OwnerType.Enemy);

        yield return new WaitForSeconds(ATTACK_DELAY);
        isAction = false;
    }

    protected override void Dead(Vector3 angle)
    {
        base.Dead(angle);
        SoundManager.Instance.SFXPlay("GolemDeathSE");
        isDead = true;
        StopAllCoroutines();
        nav.enabled = false;
        rigid.drag = 2;
        rigid.constraints = RigidbodyConstraints.FreezeRotation;
        rigid.velocity = angle * DEAD_KNOCKBACK_SPEED;
        anim.SetTrigger("isDie");
        transform.rotation = Quaternion.LookRotation(-angle);
        StartCoroutine(DeadCoroutine());
    }

    IEnumerator DeadCoroutine()
    {
        yield return new WaitForSeconds(DEAD_DELAY);
        SoundManager.Instance.SFXPlay("EnemyExplosionSE");
        EffectManager.Instance.CreateEffect("EnemyDeath", transform.position, Quaternion.identity, 1.5f);
        Destroy(gameObject);
    }
}
