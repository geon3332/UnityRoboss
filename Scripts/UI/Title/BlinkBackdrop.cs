using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlinkBackdrop : MonoBehaviour
{
    [SerializeField] Image secondBackdrop;
    [SerializeField] Sprite[] blinkImages;

    private float blinkAlpha = 0;
    private float isAngle = 0;

    private void Start()
    {
        transform.localScale = Vector3.one * 2.1f;
        ChangeScreen();
    }

    private void Update()
    {
        SpinScreen();
        BlinkScreen();
    }
    private void SpinScreen()
    {
        isAngle = (isAngle + Time.deltaTime * 2f) % 360;
        transform.rotation = Quaternion.Euler(Vector3.forward * isAngle);
    }

    private void BlinkScreen()
    {
        blinkAlpha += Time.deltaTime * 20f;
        if (blinkAlpha >= 180)
        {
            blinkAlpha -= 180;
            ChangeScreen();
        }
        secondBackdrop.color = new Color(1, 1, 1, Mathf.Sin(blinkAlpha * Mathf.Deg2Rad));
    }

    // 배경 변경
    private void ChangeScreen()
    {
        secondBackdrop.sprite = blinkImages[Random.Range(0, blinkImages.Length - 1)];
    }
}
