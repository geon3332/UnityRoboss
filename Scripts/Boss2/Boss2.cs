using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnemyMain;
using System;
using Random = UnityEngine.Random;

namespace Boss2Namespace
{
    public class SkyProduction : MonoBehaviour
    {
        private SpriteRenderer skyRenderer;
        private Coroutine skyProductionCoroutine;

        public void SetSkyRenderer(SpriteRenderer _renderer)
        {
            skyRenderer = _renderer;
        }

        public void SetSkyColor(Color startColor, Color endColor, float maxTime)
        {
            if (skyProductionCoroutine != null)
                StopCoroutine(skyProductionCoroutine);

            if (maxTime > 0f)
                skyProductionCoroutine = StartCoroutine(FadeSkyProduction(startColor, endColor, maxTime));
            else
                skyRenderer.color = endColor;
        }

        private IEnumerator FadeSkyProduction(Color startColor, Color endColor, float maxTime)
        {
            float time = 0f;
            while (time < maxTime)
            {
                skyRenderer.color = Color.Lerp(startColor, endColor, time / maxTime);
                time += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }
            skyRenderer.color = endColor;
            skyProductionCoroutine = null;
        }
    }

    public class Boss2Grounds : MonoBehaviour
    {
        private List<Vector3> groundList;

        public List<Vector3> PushData(ref List<Vector3> targetList)
        {
            foreach (var data in groundList)
                targetList.Add(data);
            return targetList;
        }

        public Boss2Grounds()
        {
            LayerMask layerMask = LayerMask.GetMask("SpecialTerrain");
            Collider[] colliders = Physics.OverlapBox(Vector3.zero, Vector3.one * 40f, Quaternion.identity, layerMask);
            groundList = new List<Vector3>();
            foreach (Collider col in colliders)
            {
                Vector3 point = col.transform.position;
                point.y = 0;
                groundList.Add(point);
            }
        }
    }


    public class Boss2 : Boss
    {
        private const float MAX_HEALTH = 400f; // 최대 체력
        private const float MAX_MANA = 600f; // 최대 마나
        private const int MAX_PATTERN = 5;

        private GameObject followObject;
        private Coroutine followCoroutine;
        private SkyProduction skyCube;
        private Boss2Grounds grounds;

        [SerializeField] LineRenderer lineRenderer;
        [SerializeField] SpriteRenderer skyProduction;
        [SerializeField] GameObject handRedSpark;
        [SerializeField] GameObject handBlueSpark;
        [SerializeField] GameObject groundSpark;
        [SerializeField] GameObject[] holePoints;

        protected override void Start()
        {
            base.Start();
            lineRenderer.GetComponent<LineRenderer>();
            grounds = new Boss2Grounds();
            InitializePattern();
            InitializeHealth();
            InitializeSkyCube();
            nav.enabled = false;
            BossState = true;
        }

        protected override void InitializePattern()
        {
            maxPattern = MAX_PATTERN;
            patternCount = new int[maxPattern];
            ultiPattern = UltimatePattern();
        }

        protected override void InitializeHealth()
        {
            maxHealth = MAX_HEALTH;
            health = maxHealth;
        }
        private void InitializeSkyCube()
        {
            skyCube = gameObject.AddComponent<SkyProduction>();
            skyCube.SetSkyRenderer(skyProduction);
        }

        private void FollowObject(GameObject obj, float _speed)
        {
            followObject = obj;
            nav.speed = _speed;
            if (followCoroutine != null)
                StopCoroutine(followCoroutine);

            nav.enabled = true;
            followCoroutine = StartCoroutine(FollowObjectCoroutine());
        }

        IEnumerator FollowObjectCoroutine()
        {
            while (true)
            {
                nav.SetDestination(GameManager.PlayerInstance.transform.position);
                yield return new WaitForFixedUpdate();
            }
        }

        private void StopFollowObject()
        {
            if (followCoroutine != null)
            {
                StopCoroutine(followCoroutine);
                followCoroutine = null;
                nav.ResetPath();
                nav.enabled = false;
            }
        }

        IEnumerator ThunderAttackCoroutine(Vector3 point, float time)
        {
            GameObject rangeObj = EffectManager.Instance.CreateEffect("SquareRange", point, Quaternion.identity, 0.6f);
            Color startColor = new Color(1, 0, 0, 1);
            Color endColor = new Color(1, 0, 0, 0f);
            EffectManager.Instance.GetFadeObject(rangeObj, startColor, endColor, 0.5f);
            yield return new WaitForSeconds(time);
            EffectManager.Instance.CreateEffect("ThunderBlue", point, Quaternion.identity, 1f);

            Vector3 targetPoint = GameManager.PlayerInstance.transform.position;
            if (Mathf.Abs(targetPoint.x - point.x) <= 4f && Mathf.Abs(targetPoint.z - point.z) <= 4f)
            {
                GameManager.PlayerInstance.GetReceiveDamage(2, (point - targetPoint).normalized * 32f);
            }
        }
        IEnumerator ThunderAttackProduction(float time)
        {
            yield return new WaitForSeconds(time);
            EffectManager.Instance.CreateEffect("ElectroHit", transform.position + Vector3.up, Quaternion.identity, 1f);
            SoundManager.Instance.SFXPlay("LightningSE");
            skyCube.SetSkyColor(Color.white, new Color(1, 1, 1, 0), 0.5f);
            GameManager.CameraInstance.CameraNoise(0.2f, 0.1f, 0.5f);
        }

        IEnumerator Pattern1()
        {
            GameManager.CameraInstance.SetCameraRange(CameraScript.BASIC_RANGE + 2.5f, 0.25f);
            SoundManager.Instance.SFXPlay("BossDashSE");
            EffectManager.Instance.CreateEffect("ElectroHit", transform.position + Vector3.up, Quaternion.identity, 1.5f);
            GameObject lightning = EffectManager.Instance.CreateEffect("LightningEffect", transform.position + Vector3.up, Quaternion.identity);
            skyCube.SetSkyColor(new Color(0.5f, 0.5f, 1, 0), new Color(0.5f, 0.5f, 1f, 1f), 0.5f);
            anim.SetBool("isCombat", true);
            yield return new WaitForSeconds(1.0f);

            float time = 2f;
            for (int i = 0; i < 3; i++)
            {
                SoundManager.Instance.SFXPlay("Signal01");

                List<Vector3> randVectors = new List<Vector3>();
                grounds.PushData(ref randVectors);

                for (int j = 0; j < 12; j++)
                {
                    int kindNum = Random.Range(0, randVectors.Count - 1);
                    Vector3 targetPoint = randVectors[kindNum];
                    randVectors.RemoveAt(kindNum);
                    StartCoroutine(ThunderAttackCoroutine(targetPoint, time));
                    StartCoroutine(ThunderAttackProduction(time));
                }
                time += 0.2f;
                yield return new WaitForSeconds(0.3f);
            }
            yield return new WaitForSeconds(2.5f);
            GameManager.CameraInstance.SetCameraRange(CameraScript.BASIC_RANGE, 0.25f);
            EffectManager.Instance.PollingDestroy(lightning);
            anim.SetBool("isCombat", false);
        }

        IEnumerator FadeMove(Vector3 targetPoint)
        {
            anim.SetBool("isWalk", true);
            float time = 0f;
            Quaternion targetRotate = Quaternion.Euler(targetPoint - transform.position);
            while(time < 1f)
            {
                transform.position = Vector3.Lerp(transform.position, targetPoint, 0.1f);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotate, 0.1f);
                time += 0.02f;
                yield return new WaitForFixedUpdate();
            }
            transform.position = targetPoint;
            transform.rotation = targetRotate;
            anim.SetBool("isWalk", false);
        }

        IEnumerator Pattern2()
        {
            RaycastHit hit;
            Vector3 point = transform.position;
            Vector3 targetPoint = Vector3.zero;
            Vector3 dic = Vector3.zero;
            point.y = 0.5f;

            SoundManager.Instance.SFXPlay("WeaponSparkSE");
            handBlueSpark.SetActive(true);
            GetTargetAngle(true);
            anim.SetBool("isCombat", true);
            UI.instance.SetFilter(new Color(0, 0, 0, 0f), new Color(0, 0, 0, 0.3f), 0.25f);

            lineRenderer.SetPosition(0, point);
            float time = 0f;
            while (time < 0.5f)
            {
                targetPoint = GameManager.PlayerInstance.transform.position;
                targetPoint.y = 0.5f;
                dic = (targetPoint - point).normalized;

                if (Physics.Raycast(point, (targetPoint - point).normalized, out hit, 100f, LayerMask.GetMask("Terrain")))
                    lineRenderer.SetPosition(1, hit.point);

                time += Time.deltaTime;
                yield return null;
            }

            GetTargetAngle(false);
            anim.SetBool("isCombat", false);
            anim.SetTrigger("isAttack");
            yield return new WaitForSeconds(0.5f);

            lineRenderer.SetPosition(0, Vector3.zero);
            lineRenderer.SetPosition(1, Vector3.zero);
            EffectManager.Instance.CreateEffect("ElectroHit", transform.position + Vector3.up, Quaternion.identity, 1f);
            SoundManager.Instance.SFXPlay("LightningSE");
            skyCube.SetSkyColor(Color.white, new Color(1, 1, 1, 0), 0.5f);
            GameManager.CameraInstance.CameraNoise(0.2f, 0.1f, 0.5f);

            UI.instance.SetFilter(new Color(1, 1, 1, 0.5f), new Color(1, 1, 1, 0f), 0.25f);
            handBlueSpark.SetActive(false);
            float shootArrow = 90f;
            while (!Physics.Raycast(point, dic, out hit, 2f, LayerMask.GetMask("Terrain")))
            {
                point += dic * 2f;
                CreateWaitBullet(point, Quaternion.Euler(Vector3.up * shootArrow) * dic);
                shootArrow *= -1;
                yield return new WaitForSeconds(0.08f);
            }
            yield return new WaitForSeconds(0.5f);
        }
        private void CreateWaitBullet(Vector3 point, Vector3 dic)
        {
            SoundManager.Instance.SFXPlay("DrumGunSE");
            EffectManager.Instance.CreateEffect("MiniElectroHit", point, Quaternion.identity, 1f);
            for (int i = 10; i <= 30; i += 10)
            {
                GameObject obj = ProjectileManager.Instance.CreateProjectile("Red", point, Quaternion.Euler(dic), 0f, 1, ProjectileManager.OwnerType.Enemy);
                Projectile bullet = obj.GetComponent<Projectile>();
                Vector3 bulletSpeed = dic * i;
                bullet.AddSpeed(bulletSpeed);
            }
        }


        IEnumerator Pattern3()
        {
            anim.SetTrigger("isAttack");
            yield return new WaitForSeconds(0.6f);
            BossHide(true);
            SoundManager.Instance.SFXPlay("LightningSE");
            EffectManager.Instance.CreateEffect("Teleport", transform.position + Vector3.up, Quaternion.identity, 1.5f);
            UI.instance.SetFilter(new Color(1, 1, 1, 0.5f), new Color(1, 1, 1, 0f), 0.5f);
            GameManager.CameraInstance.SetCameraRange(CameraScript.BASIC_RANGE + 10f, 0.5f);
            yield return new WaitForSeconds(0.5f);

            UI.instance.SetFilter(new Color(0, 0, 0, 0f), new Color(0, 0, 0, 0.25f), 0.5f);
            Vector3 targetPoint = new Vector3(Random.Range(-12f, 12f), 0, Random.Range(-12f, 12f));
            EffectManager.Instance.CreateEffect("BlueMagicCircle", targetPoint + Vector3.up, Quaternion.identity, 2.0f);
            yield return new WaitForSeconds(0.5f);
            for (int i = 0; i < 360; i += 10)
            {
                if (i % 40 == 0)
                    SoundManager.Instance.SFXPlay("DrumGunSE");
                Vector3 bulletPoint = (Quaternion.Euler(Vector3.up * i) * Vector3.forward) * 12f;
                EffectManager.Instance.CreateEffect("StarExplode", targetPoint + bulletPoint + Vector3.up, Quaternion.identity, 1f);
                yield return new WaitForSeconds(0.02f);
            }
            yield return new WaitForSeconds(0.5f);
            UI.instance.SetFilter(new Color(1, 1, 1, 0.5f), new Color(1, 1, 1, 0f), 0.5f);
            BossHide(false);
            transform.position = targetPoint;
            SoundManager.Instance.SFXPlay("LightningSE");
            EffectManager.Instance.CreateEffect("Teleport", targetPoint + Vector3.up, Quaternion.identity, 1.5f);
            for (int i = 0; i < 360; i += 10)
            {
                Vector3 dic = Quaternion.Euler(Vector3.up * i) * Vector3.forward;
                Vector3 bulletPoint = targetPoint + Vector3.up * 0.5f + (dic * 12f);
                EffectManager.Instance.CreateEffect("MiniElectroHit", bulletPoint, Quaternion.identity, 1f);
                GameObject obj = ProjectileManager.Instance.CreateProjectile("Red", bulletPoint, Quaternion.Euler(Vector3.up * i), 0f, 1, ProjectileManager.OwnerType.Enemy);
                Projectile bullet = obj.GetComponent<Projectile>();
                bullet.AddSpeed(dic * 10f);
            }
            yield return new WaitForSeconds(0.5f);
            GameManager.CameraInstance.SetCameraRange(CameraScript.BASIC_RANGE, 0.5f);
        }

        // 특정 위치에서 가장 가까운 구멍 찾기
        GameObject SearchNearHolePoint(Vector3 startPoint)
        {
            float distance = 0f;
            GameObject targetObj = null;

            for (int i = 0; i < holePoints.Length; i++)
            {
                if (i == 0 || Vector3.Distance(startPoint, holePoints[i].transform.position) < distance)
                {
                    targetObj = holePoints[i];
                    distance = Vector3.Distance(startPoint, holePoints[i].transform.position);
                }
            }
            return targetObj;
        }

        #region Pattern4Methods
        IEnumerator Pattern4()
        {
            SoundManager.Instance.SFXPlay("WeaponSparkSE");
            EffectManager.Instance.CreateEffect("RedElectroHit", handRedSpark.transform.position, Quaternion.identity, 1f);
            handRedSpark.SetActive(true);

            // 플레이어 바라보며 이동
            GetTargetAngle(true, GameManager.PlayerInstance.gameObject);
            FollowObject(GameManager.PlayerInstance.gameObject, 4f);
            yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));

            anim.SetTrigger("isAttack");
            yield return new WaitForSeconds(0.5f);

            // 큰 탄막 발사
            EffectManager.Instance.CreateEffect("RedElectroHit", transform.position + Vector3.up, Quaternion.identity, 1f);
            EffectManager.Instance.CreateEffect("CircleWave", transform.position + Vector3.up * 2.5f, Quaternion.identity, 1.0f);
            StartCoroutine(ShootBigExplodeBullet());
            handRedSpark.SetActive(false);
            yield return new WaitForSeconds(Random.Range(1.0f, 2.0f));

            GetTargetAngle(false);
            StopFollowObject();
        }
        IEnumerator ShootBigExplodeBullet()
        {
            SoundManager.Instance.SFXPlay("CannonSE");
            Vector3 dic = GameManager.PlayerInstance.transform.position - transform.position;

            GameObject obj = ProjectileManager.Instance.CreateProjectile("Red", transform.position + Vector3.up, Quaternion.LookRotation(dic), 10f, 1, ProjectileManager.OwnerType.Enemy);
            Projectile pData = obj.GetComponent<Projectile>();
            obj.transform.localScale = Vector3.one * 6f;
            pData.TerrainPass = true;
            pData.HitPass = true;
            yield return new WaitForSeconds(0.8f);

            Vector3 point = obj.transform.position;
            ProjectileManager.Instance.PollingDestroy(obj);

            float angle = Random.Range(0f, 45f);
            for (int i = 0; i < 8; i++)
                StartCoroutine(ShootExplodeBullet(point, Vector3.up * (i * 45 + angle)));
        }
        IEnumerator ShootExplodeBullet(Vector3 point, Vector3 dic)
        {
            EffectManager.Instance.CreateEffect("CircleHit", point, Quaternion.identity, 1f);
            SoundManager.Instance.SFXPlay("HandGun3");
            GameObject obj = ProjectileManager.Instance.CreateProjectile("Red", point, Quaternion.Euler(dic), 10f, 1, ProjectileManager.OwnerType.Enemy);
            obj.transform.localScale = Vector3.one * 3f;
            Projectile pData = obj.GetComponent<Projectile>();
            pData.TerrainPass = true;
            pData.HitPass = true;
            yield return new WaitForSeconds(0.8f);

            point = obj.transform.position;
            ProjectileManager.Instance.PollingDestroy(obj);
            OctagonShoot(point);
        }

        private void OctagonShoot(Vector3 point)
        {
            EffectManager.Instance.CreateEffect("CircleHit", point, Quaternion.identity, 1f);
            SoundManager.Instance.SFXPlay("HandGun6");
            float angle = Random.Range(0f, 45f);
            for (int i = 0; i < 8; i++)
                ProjectileManager.Instance.CreateProjectile("Red", point, Quaternion.Euler(Vector3.up * (i * 45 + angle)), 20f, 1, ProjectileManager.OwnerType.Enemy);
        }
        #endregion

        #region Pattern5Methods
        IEnumerator Pattern5()
        {
            anim.SetBool("isHit", true);
            SoundManager.Instance.SFXPlay("WeaponSparkSE");
            groundSpark.gameObject.SetActive(true);
            EffectManager.Instance.CreateEffect("CircleWave", transform.position + Vector3.up * 2.5f, Quaternion.identity, 1.0f);
            yield return new WaitForSeconds(0.5f);
            anim.SetBool("isHit", false);
            anim.SetBool("isWalk", true);
            GetTargetAngle(true, GameManager.PlayerInstance.gameObject);
            FollowObject(GameManager.PlayerInstance.gameObject, 5f);
            yield return new WaitForSeconds(0.5f);
            
            Coroutine shootCoroutine = StartCoroutine(RandomShoot());
            yield return new WaitForSeconds(Random.Range(3f, 4f));

            groundSpark.gameObject.SetActive(false);
            StopCoroutine(shootCoroutine);
            anim.SetBool("isWalk", false);
            GetTargetAngle(false);
            StopFollowObject();
        }

        IEnumerator RandomShoot()
        {
            float _addAngle = 25f;
            float _angle = Random.Range(0f, 360f);
            float _damage = 1;
            float _speed = 15f;

            while (true)
            {
                _angle = (_angle + _addAngle) % 360;

                SoundManager.Instance.SFXPlay("pa pew");
                Vector3 point = transform.position + Vector3.up;

                ProjectileManager.Instance.CreateProjectile("Red", point, Quaternion.Euler(Vector3.up * _angle), _speed, _damage, ProjectileManager.OwnerType.Enemy);
                ProjectileManager.Instance.CreateProjectile("Red", point, Quaternion.Euler(Vector3.up * ((_angle + 180) % 360)), _speed, _damage, ProjectileManager.OwnerType.Enemy);
                yield return new WaitForSeconds(0.05f);
            }
        }

        #endregion

        protected override IEnumerator UltimatePattern()
        {
            Mana = 0f;
            isUltimate = true;
            isInvincible = true;
            EnemyList.RemoveEnemys();
            UI.instance.SetFilter(new Color(1, 1, 1, 0.5f), new Color(1, 1, 1, 0), 0.5f);
            SoundManager.Instance.SFXPlay("FullManaSE");
            EffectManager.Instance.CreateEffect("Ultimate", transform.position + Vector3.up, Quaternion.identity, 1.5f);
            EffectManager.Instance.CreateEffect("YellowLight", transform.position + Vector3.up, Quaternion.identity, 1.5f);

            yield return new WaitForSeconds(0.5f);

            anim.SetTrigger("isAttack");
            yield return new WaitForSeconds(0.5f);

            StartTeleport(true);
            transform.position = UltimatePoint.transform.position;
            EffectManager.Instance.CreateEffect("BlueMagicCircle", UltimatePoint.transform.position + Vector3.up, Quaternion.identity, 1.5f);
            yield return new WaitForSeconds(1.5f);

            StartTeleport(false);
            yield return new WaitForSeconds(1.0f);
        }

        private void StartTeleport(bool _start)
        {
            BossHide(_start);
            SoundManager.Instance.SFXPlay("LightningSE");
            EffectManager.Instance.CreateEffect("Teleport", transform.position + Vector3.up, Quaternion.identity, 1.5f);
            UI.instance.SetFilter(new Color(1, 1, 1, 0.5f), new Color(1, 1, 1, 0f), 0.5f);
            GameManager.CameraInstance.SetCameraRange(CameraScript.BASIC_RANGE + 10f, 0.5f);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                StartCoroutine(Pattern1());
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                StartCoroutine(Pattern2());
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                StartCoroutine(Pattern3());
            else if (Input.GetKeyDown(KeyCode.Alpha4))
                StartCoroutine(Pattern4());
            else if (Input.GetKeyDown(KeyCode.Alpha5))
                StartCoroutine(Pattern5());
            else if (Input.GetKeyDown(KeyCode.Alpha6))
                StartCoroutine(UltimatePattern());

        }
    }

}
