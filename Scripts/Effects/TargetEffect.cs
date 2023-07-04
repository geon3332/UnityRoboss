using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetEffect : MonoBehaviour
{
    private const float INITIAL_SIZE = 180f;
    private const float EXPOSURE_RANGE = 0.15f;
    private const float SIZE_PINGPONG_SPEED = 300f;

    private float isSize;

    [SerializeField] private SpriteRenderer spriteRenderer;

    void OnEnable()
    {
        isSize = INITIAL_SIZE;

        SetCoroutine();
    }

    private void SetCoroutine()
    {
        StopAllCoroutines();
        StartCoroutine(SizeUpdate());
        StartCoroutine(FadeColor());
    }

    IEnumerator SizeUpdate()
    {
        while (true)
        {
            isSize = (isSize + (Time.deltaTime * SIZE_PINGPONG_SPEED)) % 360;
            transform.localScale = Vector3.one * (Mathf.Cos(isSize * Mathf.Deg2Rad) * EXPOSURE_RANGE);
            transform.localScale += Vector3.one * 0.5f;
            yield return null;
        }
    }
    IEnumerator FadeColor()
    {
        Color startColor = Color.red;
        startColor.a = 0f;
        float time = 0f;
        while (time < 1f)
        {
            spriteRenderer.color = Color.Lerp(startColor, Color.red, time);
            time += Time.deltaTime * 2f;
            yield return null;
        }
        spriteRenderer.color = Color.red;
    }
}
