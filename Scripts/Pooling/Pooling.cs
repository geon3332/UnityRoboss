using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Polling : MonoBehaviour
{
    protected string[] isNames;
    protected GameObject[] isObjects;

    protected Dictionary<string, int> dictionary;
    protected Dictionary<GameObject, Coroutine> tempCoroutine;
    protected Queue<GameObject>[] poolings; // 오브젝트 풀링
    protected Vector3[] isSizes; // 원본 사이즈 저장
    protected List<GameObject> outsideObjects; // 활성화 중인 오브젝트

    virtual protected void Start()
    {
        SceneManager.sceneLoaded += StorySceneLoaded;

        outsideObjects = new List<GameObject>();
        poolings = new Queue<GameObject>[isObjects.Length];
        dictionary = new Dictionary<string, int>();
        isSizes = new Vector3[isObjects.Length];
        tempCoroutine = new Dictionary<GameObject, Coroutine>();
        for (int i = 0; i < isObjects.Length; i++)
        {
            dictionary.Add(isNames[i], i);
            poolings[i] = new Queue<GameObject>();
            isSizes[i] = isObjects[i].transform.localScale;
        }
    }

    virtual protected GameObject Create(string typeName, Vector3 point, Quaternion angle)
    {
        GameObject _obj;
        int kindNum = dictionary[typeName];

        if (poolings[kindNum].Count == 0)
        {
            GameObject mdl = isObjects[kindNum];
            _obj = Instantiate(mdl, point, angle, transform);
            _obj.name = typeName;
        }
        else
        {
            _obj = poolings[kindNum].Dequeue();
            _obj.transform.position = point;
            _obj.transform.rotation = angle;
            _obj.transform.localScale = isSizes[kindNum];
            _obj.gameObject.SetActive(true);
        }
        outsideObjects.Add(_obj);

        return _obj;
    }

    virtual public void PollingDestroy(GameObject _obj, float lifespan = 0f)
    {
        if (lifespan > 0)
        {
            if (tempCoroutine.ContainsKey(_obj))
                StopCoroutine(tempCoroutine[_obj]);
            tempCoroutine[_obj] = StartCoroutine(DestroyDelay(_obj, lifespan));
        }
        else
        {
            if (tempCoroutine.ContainsKey(_obj))
            {
                StopCoroutine(tempCoroutine[_obj]);
                tempCoroutine.Remove(_obj);
            }
            poolings[dictionary[_obj.name]].Enqueue(_obj);
            _obj.gameObject.SetActive(false);
            outsideObjects.Remove(_obj);
        }
    }

    protected IEnumerator DestroyDelay(GameObject _obj, float lifespan)
    {
        yield return new WaitForSeconds(lifespan);
        PollingDestroy(_obj);
    }

    private void RemoveAllObject()
    {
        while (outsideObjects.Count > 0)
        {
            PollingDestroy(outsideObjects[outsideObjects.Count - 1]);
        }
    }

    // 씬 로드 시 모든 오브젝트 숨김
    private void StorySceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RemoveAllObject();
    }
}