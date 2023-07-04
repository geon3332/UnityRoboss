using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EscMenu : MonoBehaviour
{
    private const float FADE_SPEED = 0.1f;
    private const float DECREASE_MUSIC = 0.5f; // 메뉴 설정창 배경음악 볼륨 감소율

    public static bool isEnable; // 설정 메뉴 켜진 상태

    private bool isDelay = false;

    private float tempBgmValue = 0f;
    private float tempSfxValue = 0f;
    private float tempcamNoiseValue = 0f;

    [SerializeField] private Image escMenuBackdrop;
    [SerializeField] private Image optionBackdrop;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider camNoiseSlider;
    [SerializeField] private Text bgmValueText;
    [SerializeField] private Text sfxValueText;
    [SerializeField] private Text camNoiseValueText;

    public void ShowESCMenu()
    {
        if (!GameManager.PlayerInstance.isDead && !isDelay && !GameManager.Instance.isCinematic && !GameManager.Instance.isShop)
        {
            SoundManager.Instance.SFXPlay("Denied5");
            GameManager.Instance.isGamePause = !GameManager.Instance.isGamePause;
            if (GameManager.Instance.isGamePause)
            {
                StartCoroutine(ShowEscMenuCoroutine(true));
                SoundManager.Instance.SetBgmVolume(DECREASE_MUSIC);
            }
            else
            {
                StartCoroutine(ShowEscMenuCoroutine(false));
                SoundManager.Instance.SetBgmVolume(1f);
            }
        }
    }

    IEnumerator ShowEscMenuCoroutine(bool _show)
    {
        float firstSize = 0, lastSize = 0;
        Color _color = escMenuBackdrop.color;

        isDelay = true;
        if (_show)
        {
            Time.timeScale = 0;
            lastSize = 1f;
            UI.instance.SetEscFilterEnable(true);
        }
        else
            firstSize = 1f;

        float time = 0;
        while (time < 1f)
        {
            _color.a = Mathf.Lerp(firstSize, lastSize, time);
            escMenuBackdrop.color = _color;
            escMenuBackdrop.transform.localScale = new Vector3(1, Mathf.Lerp(firstSize, lastSize, time), 1);
            time += FADE_SPEED;
            yield return null;
        }
        escMenuBackdrop.transform.localScale = Vector3.one;

        if (_show)
        {
            _color.a = 1;
            escMenuBackdrop.color = _color;
        }
        else
        {
            UI.instance.SetEscFilterEnable(false);
            Time.timeScale = 1;
        }

        isDelay = false;
    }


    public void DataSave()
    {
        tempBgmValue = SoundManager.bgmVolume;
        tempSfxValue = SoundManager.sfxVolume;
        tempcamNoiseValue = CameraScript.isNoiseOption;
    }

    public void OptionMenu(bool _show)
    {
        if (!isDelay)
        {
            isEnable = _show;
            SoundManager.Instance.SFXPlay("Denied5");
            if (_show)
            {
                //이전 데이터 백업
                DataSave();
                bgmSlider.value = tempBgmValue;
                sfxSlider.value = tempSfxValue;
                camNoiseSlider.value = tempcamNoiseValue;
                StartCoroutine(OptionCoroutine());
            }
            else
            {
                SoundManager.bgmVolume = tempBgmValue;
                SoundManager.sfxVolume = tempSfxValue;
                CameraScript.isNoiseOption = tempcamNoiseValue;
                SoundManager.Instance.SetBgmVolume(DECREASE_MUSIC);
            }
            StartCoroutine(ShowOptionMenuCoroutine(_show));
        }
    }

    IEnumerator ShowOptionMenuCoroutine(bool _show)
    {
        isDelay = true;
        float firstSize = 1, lastSize = 0;
        Image targetImage;

        targetImage = _show ? escMenuBackdrop : optionBackdrop;
        targetImage.transform.localScale = Vector3.one;

        Color _color = targetImage.color;
        float time = 0;
        while (time < 1f)
        {
            _color.a = Mathf.Lerp(firstSize, lastSize, time);
            targetImage.color = _color;
            targetImage.transform.localScale = new Vector3(1, Mathf.Lerp(firstSize, lastSize, time), 1);
            time += FADE_SPEED;
            yield return null;
        }
        escMenuBackdrop.gameObject.SetActive(!_show);
        optionBackdrop.gameObject.SetActive(_show);

        targetImage = _show ? optionBackdrop : escMenuBackdrop;
        targetImage.transform.localScale = Vector3.zero;

        time = 1;
        while (time > 0f)
        {
            _color.a = Mathf.Lerp(firstSize, lastSize, time);
            targetImage.color = _color;
            targetImage.transform.localScale = new Vector3(1, Mathf.Lerp(firstSize, lastSize, time), 1);
            time -= FADE_SPEED;
            yield return null;
        }
        targetImage.transform.localScale = Vector3.one;
        _color.a = 1;
        targetImage.color = _color;

        isDelay = false;
    }


    IEnumerator OptionCoroutine()
    {
        while (isEnable)
        {
            bgmValueText.text = ((int)(bgmSlider.value * 100f)).ToString() + "%";
            sfxValueText.text = ((int)(sfxSlider.value * 100f)).ToString() + "%";
            camNoiseValueText.text = ((int)(camNoiseSlider.value * 100f)).ToString() + "%";

            SoundManager.bgmVolume = bgmSlider.value;
            SoundManager.sfxVolume = sfxSlider.value;
            CameraScript.isNoiseOption = camNoiseSlider.value;
            SoundManager.Instance.SetBgmVolume(DECREASE_MUSIC);
            yield return null;
        }
    }
}
