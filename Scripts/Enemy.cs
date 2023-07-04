using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;
using Random = UnityEngine.Random;

namespace EnemyMain
{
    static class EnemyList
    {
        static List<Enemy> enemys = new List<Enemy>();

        static public void AddList(Enemy enm)
        {
            enemys.Add(enm);
        }

        static public void RemoveList(Enemy enm)
        {
            enemys.Remove(enm);
        }

        static public void RemoveEnemys()
        {
            for (int i = enemys.Count - 1; i >= 0; i--)
            {
                if (!enemys[i].BossState)
                {
                    var enm = enemys[i];
                    RemoveList(enm);
                    enm.RemoveEnemy();
                }
            }
        }
    }

    public class Enemy : MonoBehaviour, IObjectInfo
    {
        private const float HIT_CHANGE_COLOR_TIME = 0.05f; // 적 피격 시 색상 변경 후 원래 색으로 돌아가는 시간
        protected const float BASIC_HIT_DAMAGE = 1;
        protected const float BASIC_KNOCKBACK_SPEED = 15f;

        private GameObject ultimatePoint; // 궁극기 사용 시 이동할 위치

        private bool isBoss = false; // 보스 여부

        private float hitDamage = BASIC_HIT_DAMAGE; // 충돌 피해 데미지
        private float knockbackSpeed = 15f; // 충돌 시 넉백 속도
        private bool isCollision = true; // 충돌 피해 여부
        protected bool isInvincible; // 무적 상태
        protected Renderer[] enemyColors;
        protected Rigidbody rigid;
        protected Animator anim;
        protected CapsuleCollider enemyCol;
        protected NavMeshAgent nav;

        [SerializeField] protected float maxHealth = 10; // 최대 체력
        protected float health; // 현재 체력

        private Color[] originalColors;
        public bool isDead { get; set; }

        #region propertys
        public bool BossState { get; set; }

        protected GameObject UltimatePoint
        {
            get
            {
                if (ultimatePoint == null)
                    ultimatePoint = GameObject.FindGameObjectWithTag("UltimatePoint");
                return ultimatePoint;
            }
        }
        protected float HitDamage
        {
            set
            {
                hitDamage = value;
            }
        }
        protected float KnockbackSpeed
        {
            set
            {
                knockbackSpeed = value;
            }
        }
        protected bool Collision
        {
            set
            {
                isCollision = value;
            }
        }
        #endregion


        protected virtual void Start()
        {
            EnemyList.AddList(this);
            enemyColors = GetComponentsInChildren<Renderer>();
            rigid = GetComponent<Rigidbody>();
            anim = GetComponent<Animator>();
            enemyCol = GetComponent<CapsuleCollider>();
            nav = GetComponent<NavMeshAgent>();
            health = maxHealth;

            originalColors = new Color[enemyColors.Length];
            for (int i = 0; i < enemyColors.Length; i++)
                originalColors[i] = enemyColors[i].material.color;
        }

        // 충돌 피해
        void OnCollisionStay(Collision col)
        {
            if (!isDead && isCollision && col.transform.CompareTag("Player"))
            {
                Vector3 velocity = (col.transform.position - transform.position).normalized * knockbackSpeed;
                IObjectInfo idmg = col.gameObject.GetComponent<IObjectInfo>();
                idmg.GetReceiveDamage(hitDamage, velocity);
            }
        }

        public virtual void GetReceiveDamage(float damage, Vector3 angle)
        {
            if (!isDead && !isInvincible)
            {
                health -= damage;
                if (health <= 0)
                {
                    health = 0;
                    Dead(angle);
                }
                else
                {
                    StartCoroutine(HitChangeColor());
                }
                
            }
        }
        protected virtual void Dead(Vector3 angle)
        {
            nav.enabled = false;
            isDead = true;
            rigid.useGravity = false;
            enemyCol.enabled = false;
        }

        public void RemoveEnemy()
        {
            SoundManager.Instance.SFXPlay("EnemyExplosionSE");
            EffectManager.Instance.CreateEffect("EnemyDeath", transform.position, Quaternion.identity, 1.5f);
            Destroy(gameObject);
        }


        IEnumerator HitChangeColor()
        {
            enemyColors[0].material.color = Color.red;
            yield return new WaitForSeconds(HIT_CHANGE_COLOR_TIME);
            enemyColors[0].material.color = originalColors[0];
        }

        private void OnDestroy()
        {
            EnemyList.RemoveList(this);
        }
    }

    public abstract class Boss : Enemy
    {
        protected const float BASIC_SPINSPEED = 5;

        protected int maxPattern; // 기본 패턴의 최대 수
        protected int applyPattern; // 현재 진행중인 패턴
        protected int[] patternCount; // 미 사용 패턴 카운트

        protected float maxMana = 600f; // 최대 마나
        protected float isMana = 0f; // 현재 마나
        protected float isSpinSpeed = BASIC_SPINSPEED; // 회전 속도
        protected bool isUltimate = false; // 필살기 사용 여부
        private bool targetAngle = false; // 목표 오브젝트 바라보기

        private GameObject targetObj; // 목표 오브젝트

        protected IEnumerator ultiPattern;
        protected Func<IEnumerator>[] PatternAction; // 패턴 함수 모음
        protected Action[] PatternStop; // 패턴 정지 함수 모음

        protected SkinnedMeshRenderer meshRenderer;

        #region Property
        protected float Mana
        {
            set
            {
                isMana = value;
                UI.instance.SetBossManaUI(isMana, maxMana);
                if (isMana >= maxMana)
                {
                    StopAllCoroutines();
                    PatternStop[applyPattern]();
                    StartCoroutine(ultiPattern);
                }
            }
            get
            {
                return isMana;
            }
        }
        #endregion
        protected abstract IEnumerator UltimatePattern();
        protected abstract void InitializePattern();
        protected abstract void InitializeHealth();
        

        protected override void Start()
        {
            base.Start();
            meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            BossState = true;
            StartCoroutine(ManaRegen());
        }
        public override void GetReceiveDamage(float damage, Vector3 angle)
        {
            base.GetReceiveDamage(damage, angle);
            UI.instance.SetBossHealthUI(health, maxHealth);
        }

        protected void BossHide(bool _hide)
        {
            foreach (var color in enemyColors)
                color.enabled = !_hide;
            Collision = !_hide;
            enemyCol.enabled = !_hide;
            isInvincible = _hide;
        }

        protected IEnumerator ManaRegen()
        {
            while (true)
            {
                if (!isUltimate)
                {
                    Mana += Time.deltaTime * 10f;
                }
                yield return null;
            }
        }


        //오브젝트를 주시하게 설정
        protected void GetTargetAngle(bool state, GameObject obj = null)
        {
            targetAngle = state;

            if (state)
            {
                if (obj != null)
                    targetObj = obj;
                else
                    targetObj = GameManager.PlayerInstance.gameObject;
                StartCoroutine(TargetAngleCoroutine());
            }
        }
        IEnumerator TargetAngleCoroutine()
        {
            while (targetAngle)
            {
                Vector3 dic = targetObj.transform.position - transform.position;
                dic.y = 0f;
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dic), isSpinSpeed * Time.deltaTime);
                yield return null;
            }
        }

        // 패턴 선택
        protected int SelectPattern(int except)
        {
            int count = 0;
            for (int i = 0; i < maxPattern; i++)
            {
                if (i != except)
                {
                    patternCount[i]++;
                    count += patternCount[i];
                }
            }

            int rand = Random.Range(0, count + 1);
            int rangeCount = 0;
            for (int i = maxPattern - 1; i >= 0; i--)
            {
                if (i != except)
                {
                    rangeCount += patternCount[i];
                    if (rand <= rangeCount)
                    {
                        patternCount[i] = 0;
                        return i;
                    }
                }
            }
            return -1;
        }
    }
}