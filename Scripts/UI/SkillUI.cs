using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SkillUI : MonoBehaviour
{
    public static bool[] cooldownState;

    [SerializeField] private Image[] caseIcon;
    private Image[] skillIcon;
    private Image[] cooldownIcon;
    private Text[] hotkeyText;
    private Text[] cooldownText;
    public enum Type
    {
        L_Mouse = 0,
        R_Mouse = 1,
        Q_Skill = 2,
        Defanse = 3
    }

    private void Start()
    {
        SceneManager.sceneLoaded += StorySceneLoaded;
        cooldownState = new bool[caseIcon.Length];

        SetComponent();
    }

    private void SetComponent()
    {
        skillIcon = new Image[caseIcon.Length];
        cooldownIcon = new Image[caseIcon.Length];
        hotkeyText = new Text[caseIcon.Length];
        cooldownText = new Text[caseIcon.Length];
        for (int i = 0; i < caseIcon.Length; i++)
        {
            skillIcon[i] = caseIcon[i].transform.GetChild(0).GetComponent<Image>();
            cooldownIcon[i] = caseIcon[i].transform.GetChild(1).GetComponent<Image>();
            hotkeyText[i] = caseIcon[i].transform.GetChild(2).GetComponent<Text>();
            cooldownText[i] = caseIcon[i].transform.GetChild(3).GetComponent<Text>();
        }
    }

    public void GetCooldown(Type skillType, float time)
    {
        int kind = (int)skillType;

        if (!cooldownState[kind])
        {
            cooldownState[kind] = true;

            cooldownIcon[kind].gameObject.SetActive(true);
            cooldownText[kind].gameObject.SetActive(true);

            cooldownIcon[kind].fillAmount = 1;
            cooldownText[kind].text = time > 0.5f ? time.ToString("F0") : time.ToString("F1");

            StartCoroutine(CooldownEnd(kind, time));
        }
    }

    private IEnumerator CooldownEnd(int kind, float time)
    {
        float maxTime = time;

        while (time > 0)
        {
            time -= Time.deltaTime;
            cooldownIcon[kind].fillAmount = time / maxTime;
            cooldownText[kind].text = time > 0.5f ? time.ToString("F0") : time.ToString("F1");
            yield return null;
        }
        ResetCooldown(kind);
    }

    private void ResetCooldown(int kind)
    {
        cooldownIcon[kind].gameObject.SetActive(false);
        cooldownText[kind].gameObject.SetActive(false);
        cooldownState[kind] = false;
    }

    private void StorySceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StopAllCoroutines();
        for (int i = 0; i < caseIcon.Length; i++)
            ResetCooldown(i);
    }
}
