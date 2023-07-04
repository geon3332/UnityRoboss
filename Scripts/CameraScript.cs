using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraScript : MonoBehaviour
{
    static public readonly float BASIC_RANGE = 15f; // ī�޶� �⺻ ����
    static public float isNoiseOption = 1f; // ī�޶� ���� ���� ����
    private const float BASIC_ANGLE = 60f;

    private bool isCamLock = true; // ī�޶� �÷��̾� ����
    private float isRange = BASIC_RANGE; // ī�޶� ����
    private float noisePower; // ī�޶� ��鸲 ����
    private float noiseInterval; // ī�޶� ��鸲 ����
    private float noiseIntervalCount;

    private float noisePowerX;
    private float noisePowerY;
    private float noisePowerZ;

    private Vector3 camPos; // ī�޶� ��ġ
    private Quaternion camRot; // ī�޶� ����

    private Coroutine noiseCoroutine;
    private Coroutine rangeCoroutine;
    private Coroutine moveCoroutine;

    private CinemachineVirtualCamera cinevirtual;

    private bool isCamNoise; // ī�޶� ��鸮�� ����

    private void Update()
    {
        PosCamera();
        UpdateCamera();
    }

    public bool CamLock {
        get
        {
            return isCamLock;
        }
        set
        {
            isCamLock = value;
        }
    }

    private void PosCamera()
    {
        if (isCamLock)
        {
            float x = GameManager.PlayerInstance.transform.position.x + noisePowerX;
            float y = Mathf.Max(GameManager.PlayerInstance.transform.position.y + isRange, isRange);
            float z = GameManager.PlayerInstance.transform.position.z - (isRange * 0.6f) + noisePowerZ;

            camPos = new Vector3(x, y, z);
            camRot = Quaternion.Euler(Vector3.right * BASIC_ANGLE);
            Camera.main.fieldOfView = 60f;
        }

    }

    private void UpdateCamera()
    {
        float x = camPos.x + noisePowerX;
        float z = camPos.z + noisePowerZ;

        if (isCamNoise && Time.timeScale != 0)
        {
            noiseIntervalCount += Time.deltaTime;
            if (noiseIntervalCount <= noiseInterval)
            {
                float _power = noisePower * isNoiseOption;
                noiseIntervalCount = 0;
                noisePowerX = Random.Range(-_power, _power);
                noisePowerY = Random.Range(-_power, _power);
                noisePowerZ = Random.Range(-_power, _power);
            }
        }
        Camera.main.transform.position = new Vector3(x, camPos.y, z);
        Camera.main.transform.rotation = camRot;
    }

    public void CameraNoise(float power, float interval, float time = 0f)
    {
        if (noiseCoroutine != null)
        {
            StopCoroutine(noiseCoroutine);
            noiseCoroutine = null;
        }

        if (time > 0f)
        {
            if (!isCamNoise)
                isCamNoise = true;

            noiseCoroutine = StartCoroutine(CameraNoiseEnd(time));
        }
        noiseIntervalCount = 0;
        noisePower = power;
        noiseInterval = interval;
    }

    IEnumerator CameraNoiseEnd(float time)
    {
        yield return new WaitForSeconds(time);
        noisePower = 0;
        noiseInterval = 0;
        noisePowerX = 0;
        noisePowerY = 0;
        noisePowerZ = 0;
        isCamNoise = false;
    }

    public void SetCameraRange(float range, float time = 0f)
    {
        if(rangeCoroutine != null)
        {
            StopCoroutine(rangeCoroutine);
            rangeCoroutine = null;
        }

        if (time <= 0f)
        {
            isRange = range;
        }
        else
            rangeCoroutine = StartCoroutine(CameraRangeEnd(range, time));
    }

    IEnumerator CameraRangeEnd(float range, float time)
    {
        float firstRange = isRange;
        float targetRange = range;
        float maxTime = time;

        while (time > 0f)
        {
            isRange = Mathf.Lerp(targetRange, firstRange, time / maxTime);
            time -= Time.deltaTime;
            yield return null;
        }
        isRange = targetRange;
    }

    public void FadeMove(Vector3 target)
    {
        if (!isCamLock)
        {
            if (moveCoroutine != null)
                StopCoroutine(moveCoroutine);

            target.y = Mathf.Max(target.y + isRange, isRange);
            target.z -= isRange * 0.6f;
            moveCoroutine = StartCoroutine(FadeMoveCoroutine(target));
        }
    }

    IEnumerator FadeMoveCoroutine(Vector3 target)
    {
        float time = 0f;
        while(time < 1f)
        {
            camPos = Vector3.Lerp(camPos, target, 0.05f);

            time += Time.deltaTime;
            yield return null;
        }
        camPos = target;

        moveCoroutine = null;
    }
}
