using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaGround : MonoBehaviour
{
    private const float INTENSITY_OFF = 0f;
    private const float INTENSITY_ON = 7f;

    [SerializeField] private Light lavaLight;

    public void SetLight(bool _on)
    {
        StopAllCoroutines();
        StartCoroutine(LightCoroutine(_on));
    }

    IEnumerator LightCoroutine(bool _on)
    {
        float s_Inten;
        float e_Inten;
        if (_on)
        {
            s_Inten = INTENSITY_OFF;
            e_Inten = INTENSITY_ON;
        }
        else
        {
            e_Inten = INTENSITY_OFF;
            s_Inten = INTENSITY_ON;
        }

        float time = 0f;
        while (time < 1f)
        {
            lavaLight.intensity = Mathf.Lerp(s_Inten, e_Inten, time);
            time += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        lavaLight.intensity = e_Inten;
    }
}
