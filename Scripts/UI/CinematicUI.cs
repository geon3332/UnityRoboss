using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CinematicUI : MonoBehaviour
{
    private const float SKIP_TEXT_BLINK_TIME = 0.5f; // SKIP ÅØ½ºÆ® ±ôºýÀÌ´Â °£°Ý

    private Canvas canvas;
    [SerializeField] private Image topPanel;
    [SerializeField] private Image bottomPanel;
    [SerializeField] private Text skipText;

    private Coroutine blinkCoroutine;

    private void Start()
    {
        canvas = GetComponent<Canvas>();
    }

    public void GetShowCinematicPanel(bool state)
    {
        if (state)
        {
            canvas.enabled = state;
            blinkCoroutine = StartCoroutine(BlinkSkipText());
        }
        else if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            skipText.gameObject.SetActive(false);
            blinkCoroutine = null;
        }

        StartCoroutine(FadeCinematicPanel(state));
    }

    IEnumerator BlinkSkipText()
    {
        bool _show = false;
        while (true)
        {
            skipText.gameObject.SetActive(_show);
            _show = !_show;
            yield return new WaitForSeconds(SKIP_TEXT_BLINK_TIME);
        }
    }

    IEnumerator FadeCinematicPanel(bool state)
    {
        float fa = state ? 0f : 1f;
        float add = state ? 2 : -2;

        topPanel.fillAmount = fa;
        bottomPanel.fillAmount = fa;

        do
        {
            yield return null;
            topPanel.fillAmount = fa;
            bottomPanel.fillAmount = fa;
            fa += Time.deltaTime * add;
        } while (fa >= 0f && fa <= 1f);

        if (!state)
        {
            topPanel.fillAmount = 0f;
            bottomPanel.fillAmount = 0f;
            canvas.enabled = false;
        }
        else
        {
            topPanel.fillAmount = 1f;
            bottomPanel.fillAmount = 1f;
        }
    }
}
