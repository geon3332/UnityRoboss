using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : Polling
{
    private static EffectManager instance;

    #region singleton
    private void Awake()
    {
        if (Instance == null)
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
    [SerializeField] private GameObject[] isSerializeObjects;

    public static EffectManager Instance
    {
        get
        {
            if (instance == null)
                instance = new EffectManager();
            return instance;
        }

    }

    protected override void Start()
    {
        isNames = isSerializeNames;
        isObjects = isSerializeObjects;
        base.Start();
    }

    public GameObject CreateEffect(string typeName, Vector3 point, Quaternion angle, float lifespan = 0)
    {
        GameObject _obj = base.Create(typeName, point, angle);
        
        if (lifespan > 0)
            PollingDestroy(_obj, lifespan);
        return _obj;
    }

    public void GetFadeObject(GameObject obj, Color firstColor, Color lastColor, float time = 1f)
    {
        Renderer renderer = obj.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = firstColor;
            StartCoroutine(FadeObjectCoroutine(obj, renderer, firstColor, lastColor, time));
        }
    }

    protected IEnumerator FadeObjectCoroutine(GameObject obj, Renderer renderer, Color firstColor, Color lastColor, float time)
    {
        float maxTime = time;
        while (time > 0 && obj.activeSelf)
        {
            renderer.material.color = Color.Lerp(lastColor, firstColor, time / maxTime);
            time -= Time.deltaTime;
            yield return null;
        }
        renderer.material.color = lastColor;
    }
}
