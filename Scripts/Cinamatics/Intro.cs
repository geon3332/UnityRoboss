using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intro : MonoBehaviour
{
    private bool isExecution = false;
    [SerializeField] private Transform bossTransform;
    [SerializeField] private string bossIntroMusic = null;
    [SerializeField] private string bossMusic;

    private void Start()
    {
        isExecution = true;
        GameManager.CameraInstance.CamLock = false;
        UI.instance.SetFilter(new Color(0, 0, 0, 1), new Color(0, 0, 0, 0), 2f);
        UI.Cinematic.GetShowCinematicPanel(true);
        StartCoroutine(SkipCoroutine());
    }

    //ÄÆ¾À Á¾·á
    public void EndCinematic()
    {
        if (isExecution)
        {
            StopAllCoroutines();
            isExecution = false;
            if (bossIntroMusic == "")
                SoundManager.Instance.BgmPlay(bossMusic);
            else
            {
                SoundManager.Instance.BgmPlay(bossIntroMusic);
                SoundManager.Instance.SetNextBgm(bossMusic);
            }
            StartCoroutine(EndCinematicCoroutine());
            UI.Cinematic.GetShowCinematicPanel(false);
        }
    }
    IEnumerator EndCinematicCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        UI.instance.ShowGameInterfaces(true);
        UI.instance.SetBossHealthUIEnable(true);
        GameManager.Instance.isCinematic = false;
        GameManager.CameraInstance.CamLock = true;
        gameObject.SetActive(false);
        bossTransform.position = Vector3.zero;
    }

    IEnumerator SkipCoroutine()
    {
        while (GameManager.Instance.isCinematic)
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                EndCinematic();
                break;
            }
            yield return null;
        }
    }
}
