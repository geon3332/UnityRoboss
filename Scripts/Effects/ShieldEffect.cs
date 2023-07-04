using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldEffect : MonoBehaviour
{
    private const float COLOR_CHANGE_SPEED = 300f;
    private const float SIZE_CHANGE_SPEED = 300f;

    private Vector3 defaultSize;
    private float isSize;
    private float isColor;
    private Renderer shieldColor;

    private void Start()
    {
        shieldColor = GetComponent<Renderer>();
        defaultSize = transform.localScale;
    }

    private void Update()
    {
        transform.rotation = Quaternion.identity;
        ChangeColor();
        ChangeSize();
    }

    private void ChangeColor()
    {
        isColor = (isColor + Time.deltaTime * COLOR_CHANGE_SPEED) % 360;
        Color color = Color.white;
        color.g = 0.5f + (Mathf.Cos(isColor * Mathf.Deg2Rad) * 0.5f);
        shieldColor.material.SetColor("_Color", color);
    }

    private void ChangeSize()
    {
        isSize = (isSize + Time.deltaTime * SIZE_CHANGE_SPEED) % 360;
        transform.localScale = defaultSize + (Vector3.one * (Mathf.Cos(isSize * Mathf.Deg2Rad) * 0.25f));
    }
}
