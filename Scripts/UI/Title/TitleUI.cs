using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleUI : MonoBehaviour
{
    private const string TITLE_TEXT = "Roboss";
    private const string TUTORIAL_TEXT = "How to play";

    private const float MENU_SHOW_SPEED = 10f; // 메뉴 갱신 속도

    private bool isButtonEnable = false; // 버튼 작동 여부

    [SerializeField] Image titleBackdrop;
    [SerializeField] Image menuBackdrop;
    [SerializeField] Image fadeBackdrop;
    [SerializeField] Image tutorialBackdrop;
    [SerializeField] Text titleText;
    [SerializeField] RectTransform titlePolish;

    void Start()
    {
        StartCoroutine(HideBlackScreen());
        StartCoroutine(MoveTitlePolish());
    }

    IEnumerator MoveTitlePolish()
    {
        float posY = 400f;
        while (true)
        {
            titlePolish.anchoredPosition = new Vector2(0, posY);
            posY -= Time.deltaTime * 200;
            if (posY < -400)
                posY = 400;
            yield return null;
        }
    }

    IEnumerator ShowBlackScreen()
    {
        fadeBackdrop.gameObject.SetActive(true);
        fadeBackdrop.enabled = true;

        Color startColor = new Color(0, 0, 0, 0);
        float time = 0f;
        while (time < 1f)
        {
            fadeBackdrop.color = Color.Lerp(startColor, Color.black, time);
            time += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(0.5f);
        SoundManager.Instance.BgmPlay("LobbyMusic");
        SceneManager.LoadScene("LobbyScene");
    }

    IEnumerator HideBlackScreen()
    {
        titleBackdrop.gameObject.SetActive(false);
        menuBackdrop.gameObject.SetActive(false);
        fadeBackdrop.enabled = true;

        Color endColor = new Color(0, 0, 0, 0);
        float time = 0f;
        while (time < 1f)
        {
            fadeBackdrop.color = Color.Lerp(Color.black, endColor, time);
            time += Time.deltaTime;
            yield return null;
        }
        fadeBackdrop.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        SoundManager.Instance.SFXPlay("Denied5");
        SoundManager.Instance.BgmPlay("TitleMusic");
        StartCoroutine(ShowUI(titleBackdrop));
        StartCoroutine(ShowUI(menuBackdrop));
    }

    IEnumerator ShowUI(Image targetUI, IEnumerator nextCoroutine = null)
    {
        targetUI.gameObject.SetActive(true);
        Vector3 startScale = new Vector3(0, 1, 1);
        Color startColor = targetUI.color;
        Color endColor = targetUI.color;
        startColor.a = 0f;
        targetUI.transform.localScale = startScale;

        float time = 0f;
        while (time < 1f)
        {
            targetUI.color = Color.Lerp(startColor, endColor, time);
            targetUI.transform.localScale = Vector3.Lerp(startScale, Vector3.one, time);
            time += Time.deltaTime * MENU_SHOW_SPEED;
            yield return null;
        }
        targetUI.transform.localScale = Vector3.one;
        targetUI.color = endColor;

        if (nextCoroutine != null)
            StartCoroutine(nextCoroutine);
        else
            isButtonEnable = true;
    }

    IEnumerator HideUI(Image targetUI, IEnumerator nextCoroutine = null)
    {
        Vector3 endScale = new Vector3(0, 1, 1);
        Color startColor = targetUI.color;
        Color endColor = targetUI.color;
        endColor.a = 0f;

        float time = 0f;
        while (time < 1f)
        {
            targetUI.color = Color.Lerp(startColor, endColor, time);
            targetUI.transform.localScale = Vector3.Lerp(Vector3.one, endScale, time);
            time += Time.deltaTime * MENU_SHOW_SPEED;
            yield return null;
        }
        targetUI.gameObject.SetActive(false);
        targetUI.transform.localScale = Vector3.one;
        targetUI.color = startColor;

        if (nextCoroutine != null)
            StartCoroutine(nextCoroutine);
        else
            isButtonEnable = true;
    }

    IEnumerator ChangeTitleText(string _text)
    {
        float time = 0f;
        while (time < 1f)
        {
            time += Time.deltaTime * MENU_SHOW_SPEED;
            yield return null;
        }
        titleText.text = _text;
    }

    public void StartGame()
    {
        isButtonEnable = false;
        SoundManager.Instance.BgmStop(true);
        SoundManager.Instance.SFXPlay("Denied5");
        StartCoroutine(HideUI(titleBackdrop));
        StartCoroutine(HideUI(menuBackdrop, ShowBlackScreen()));
    }

    public void ShowTutorialMenu(bool _show)
    {
        isButtonEnable = false;
        SoundManager.Instance.SFXPlay("Denied5");
        if (_show)
        {
            StartCoroutine(HideUI(menuBackdrop, ShowUI(tutorialBackdrop)));
            StartCoroutine(HideUI(titleBackdrop, ShowUI(titleBackdrop)));
            StartCoroutine(ChangeTitleText(TUTORIAL_TEXT));
        }
        else
        {
            StartCoroutine(HideUI(tutorialBackdrop, ShowUI(menuBackdrop)));
            StartCoroutine(HideUI(titleBackdrop, ShowUI(titleBackdrop)));
            StartCoroutine(ChangeTitleText(TITLE_TEXT));
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
