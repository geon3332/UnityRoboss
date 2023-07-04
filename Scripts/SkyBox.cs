using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyBox : MonoBehaviour
{
    private const float BASIC_ROTATION = 0f;
    private const float BASIC_EXPOSURE = 1f;

    [SerializeField] private float spinSpeed = 1f; // 회전 속도
    [SerializeField] private float isExposureSpeed = 10f; // 밝기 변경 속도
    [SerializeField] private float isExposureRange = 0.3f; // 밝기 범위

    void Start()
    {
        StartCoroutine(SpinSkyBox());
    }

    IEnumerator SpinSkyBox()
    {
        float degree = 0;
        float exposure = 0;

        while (true)
        {
            degree = (degree - Time.deltaTime * spinSpeed) % 360;
            RenderSettings.skybox.SetFloat("_Rotation", degree);

            exposure = (exposure + Time.deltaTime * isExposureSpeed) % 360;
            RenderSettings.skybox.SetFloat("_Exposure", 1f + Mathf.Cos(exposure * Mathf.Deg2Rad) * isExposureRange);

            yield return null;
        }
    }

    void OnApplicationQuit()
    {
        RenderSettings.skybox.SetFloat("_Rotation", BASIC_ROTATION);
        RenderSettings.skybox.SetFloat("_Exposure", BASIC_EXPOSURE);
    }
}
