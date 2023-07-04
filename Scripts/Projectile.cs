using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnemyMain;

public class Projectile : MonoBehaviour
{
    public float isDamage = 0;
    public ProjectileManager.OwnerType isOwner;
    private bool isTerrainPass; // 지형 충돌 무시
    private bool isHitPass; // 피격 시 파괴 설정

    #region propertys
    public bool TerrainPass
    {
        set
        {
            isTerrainPass = value;
        }
    }
    public bool HitPass
    {
        set
        {
            isHitPass = value;
        }
    }
    #endregion

    public void AddSpeed(Vector3 _addSpeed)
    {
        StartCoroutine(AddSpeedCoroutine(_addSpeed));
    }

    IEnumerator AddSpeedCoroutine(Vector3 _addSpeed)
    {
        Rigidbody rigid = GetComponent<Rigidbody>();
        while (true)
        {
            rigid.AddForce(_addSpeed);
            yield return new WaitForFixedUpdate();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Terrain") && !isTerrainPass)
        {
            SoundManager.Instance.SFXPlay("BulletDestroySE");
            ProjectileManager.Instance.PollingDestroy(gameObject);
        }
        else if ((isOwner == ProjectileManager.OwnerType.Player && other.gameObject.CompareTag("Enemy")) ||
            (isOwner == ProjectileManager.OwnerType.Enemy && other.gameObject.CompareTag("Player")))
        {
            IObjectInfo objectConponent = other.GetComponent<IObjectInfo>();
            if (!objectConponent.isDead)
            {
                SoundManager.Instance.SFXPlay("BulletDestroySE");
                objectConponent.GetReceiveDamage(isDamage, transform.forward);
                if (!isHitPass)
                    ProjectileManager.Instance.PollingDestroy(gameObject);
            }
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        isTerrainPass = false;
        isHitPass = false;
    }
}
