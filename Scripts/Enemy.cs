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
        private const float HIT_CHANGE_COLOR_TIME = 0.05f; // �� �ǰ� �� ���� ���� �� ���� ������ ���ư��� �ð�
        protected const float BASIC_HIT_DAMAGE = 1;
        protected const float BASIC_KNOCKBACK_SPEED = 15f;

        private GameObject ultimatePoint; // �ñر� ��� �� �̵��� ��ġ

        private bool isBoss = false; // ���� ����

        private float hitDamage = BASIC_HIT_DAMAGE; // �浹 ���� ������
        private float knockbackSpeed = 15f; // �浹 �� �˹� �ӵ�
        private bool isCollision = true; // �浹 ���� ����
        protected bool isInvincible; // ���� ����
        protected Renderer[] enemyColors;
        protected Rigidbody rigid;
        protected Animator anim;
        protected CapsuleCollider enemyCol;
        protected NavMeshAgent nav;

        [SerializeField] protected float maxHealth = 10; // �ִ� ü��
        protected float health; // ���� ü��

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

        // �浹 ����
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

        protected int maxPattern; // �⺻ ������ �ִ� ��
        protected int applyPattern; // ���� �������� ����
        protected int[] patternCount; // �� ��� ���� ī��Ʈ

        protected float maxMana = 600f; // �ִ� ����
        protected float isMana = 0f; // ���� ����
        protected float isSpinSpeed = BASIC_SPINSPEED; // ȸ�� �ӵ�
        protected bool isUltimate = false; // �ʻ�� ��� ����
        private bool targetAngle = false; // ��ǥ ������Ʈ �ٶ󺸱�

        private GameObject targetObj; // ��ǥ ������Ʈ

        protected IEnumerator ultiPattern;
        protected Func<IEnumerator>[] PatternAction; // ���� �Լ� ����
        protected Action[] PatternStop; // ���� ���� �Լ� ����

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


        //������Ʈ�� �ֽ��ϰ� ����
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

        // ���� ����
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