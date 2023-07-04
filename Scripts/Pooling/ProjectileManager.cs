using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : Polling
{
    private static ProjectileManager instance;
    private const float DEFAULT_LIFESPAN = 3f;
    public enum OwnerType
    {
        Player = 0,
        Enemy = 1
    }

    #region singleton
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance);
        }
        else
        {
            PollingDestroy(gameObject);
        }
    }
    #endregion singleton

    [SerializeField] private string[] isSerializeNames;
    [SerializeField] private string[] DestroyEffects;
    [SerializeField] private GameObject[] isSerializeObjects;

    public static ProjectileManager Instance
    {
        get
        {
            if (instance == null)
                instance = new ProjectileManager();
            return instance;
        }

    }

    protected override void Start()
    {
        isNames = isSerializeNames;
        isObjects = isSerializeObjects;
        base.Start();
    }

    public GameObject CreateProjectile(string typeName, Vector3 point, Quaternion angle, float speed, float damage, OwnerType owner = OwnerType.Player)
    {
        GameObject _obj = base.Create(typeName, point, angle);
        Rigidbody bulletRigid = _obj.GetComponent<Rigidbody>();
        Projectile p_comp = _obj.GetComponent<Projectile>();

        _obj.transform.position = point;
        _obj.transform.rotation = angle;
        bulletRigid.velocity = _obj.transform.forward * speed;
        p_comp.isDamage = damage;
        p_comp.isOwner = owner;

        PollingDestroy(_obj, DEFAULT_LIFESPAN);
        return _obj;
    }
    public override void PollingDestroy(GameObject _obj, float lifespan = 0)
    {
        if (lifespan <= 0)
            EffectManager.Instance.CreateEffect(DestroyEffects[dictionary[_obj.name]], _obj.transform.position, Quaternion.identity, 1.5f);

        base.PollingDestroy(_obj, lifespan);
    }
}
