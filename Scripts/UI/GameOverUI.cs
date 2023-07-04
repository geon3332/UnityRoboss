using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    private Canvas canvas;
    private bool selectState = false; // 메뉴 선택이 가능한 상태

    [SerializeField] private Image blackBackdrop; // 검은 배경
    [SerializeField] private Text gameoverText; // 게임 오버 텍스트
    [SerializeField] private GameObject menu;

    private void Start()
    {
        canvas = GetComponent<Canvas>();
    }

    public void ShowGameOverUI()
    {
        StartCoroutine(GameOverCoroutine());
    }

    public void HideGameOverUI()
    {
        if (selectState)
        {
            selectState = false;
            StartCoroutine(GameOverHideCoroutine());
        }
    }

    IEnumerator GameOverHideCoroutine()
    {
        float time = 0f;

        SoundManager.Instance.SFXPlay("Denied5");
        while (time < 1f)
        {
            menu.transform.localScale = new Vector3(1, Mathf.Lerp(1, 0, time), 1);
            time += 0.3f;
            yield return new WaitForSecondsRealtime(0.02f);
        }
        menu.transform.localScale = new Vector3(1, 1, 1);
        menu.SetActive(false);
        canvas.enabled = false;
        UI.instance.SetFilter(Color.black, Color.black, 0);
        yield return new WaitForSecondsRealtime(2f);
        GameManager.Instance.InitializeVariables();
        Time.timeScale = 1;
        GameManager.Instance.LoadLobbyScene();
    }

    IEnumerator ShowBlackBackdrop()
    {
        float time = 0f;
        Color firstColor = new Color(0, 0, 0, 0);
        Color lastColor = new Color(0, 0, 0, 1);

        menu.SetActive(false);
        canvas.enabled = true;
        blackBackdrop.color = firstColor;
        gameoverText.color = firstColor;
        while (time < 1f)
        {
            blackBackdrop.color = Color.Lerp(firstColor, lastColor, time);
            Time.timeScale = 1 - time;
            time += 0.01f;
            yield return new WaitForSecondsRealtime(0.02f);
        }
        Time.timeScale = 0;
        blackBackdrop.color = lastColor;
    }

    IEnumerator ShowGameOverText(bool _show)
    {
        Color firstColor = new Color(1, 1, 1, 0);
        Color lastColor = Color.white;
        if (!_show)
        {
            lastColor = firstColor;
            firstColor = Color.white;
        }

        float time = 0f;
        while (time < 1f)
        {
            gameoverText.color = Color.Lerp(firstColor, lastColor, time);
            time += 0.01f;
            yield return new WaitForSecondsRealtime(0.02f);
        }
    }
    IEnumerator ShowReplayMenu()
    {
        SoundManager.Instance.SFXPlay("Denied5");
        menu.SetActive(true);
        menu.transform.localScale = new Vector3(1, 0, 1);
        float time = 0f;
        while (time < 1f)
        {
            menu.transform.localScale = new Vector3(1, Mathf.Lerp(0, 1, time), 1);
            time += 0.3f;
            yield return new WaitForSecondsRealtime(0.02f);
        }
        menu.transform.localScale = new Vector3(1, 1, 1);
        selectState = true;
    }


    IEnumerator GameOverCoroutine()
    {
        // 인터페이스 숨김
        UI.instance.ShowGameInterfaces(false);
        yield return new WaitForSecondsRealtime(1.5f);
        
        // 화면 가리기
        StartCoroutine(ShowBlackBackdrop());
        yield return new WaitForSecondsRealtime(2.5f);

        // 게임 오버 텍스트 보이기
        SoundManager.Instance.SFXPlay("GameOverSE");
        StartCoroutine(ShowGameOverText(true));
        yield return new WaitForSecondsRealtime(5f);

        // 게임 오버 텍스트 숨김
        StartCoroutine(ShowGameOverText(false));
        yield return new WaitForSecondsRealtime(3f);

        // 재시작 메뉴 보이기
        StartCoroutine(ShowReplayMenu());
    }
}
