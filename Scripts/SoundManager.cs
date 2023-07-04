using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    static public float bgmVolume = 1f; // 배경음악 볼륨 설정
    static public float sfxVolume = 1f; // 효과음 볼륨 설정

    private string nextBgm;
    private AudioSource[] audioSources;
    private AudioSource[] bgmSources;
    private Dictionary<string, int> sfxDict;
    private Dictionary<string, int> bgmDict;

    private AudioSource applyMusic; // 재생중인 배경음악

    private static SoundManager instance; // 싱글톤

    public AudioClip[] audioClips;
    public AudioClip[] bgmClips;

    private Coroutine nextBgmCoroutine;

    #region singleton
    private void Awake()
    {
        if (Instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion singleton

    public static SoundManager Instance
    {
        get
        {
            if (instance == null)
            {
                new SoundManager();
            }
            return instance;
        }

    }

    public void SFXPlay(string sfxName, bool isLoop = false)
    {
        int kind = sfxDict[sfxName];
        audioSources[kind].Play();
        audioSources[kind].loop = isLoop;
        audioSources[kind].volume = sfxVolume;
    }

    public void SFXStop(string sfxName, bool fade = false)
    {
        int kind = sfxDict[sfxName];
        if (!fade)
            audioSources[kind].Stop();
        else
            StartCoroutine(SFXStopFade(kind));
    }

    IEnumerator SFXStopFade(int kind)
    {
        float time = 1f;

        while(time > 0f)
        {
            audioSources[kind].volume = sfxVolume * time;
            time -= Time.deltaTime;
            yield return null;
        }
        audioSources[kind].volume = sfxVolume;
        audioSources[kind].Stop();
    }

    public void BgmStop(bool isFade)
    {
        if (nextBgmCoroutine != null)
            StopCoroutine(nextBgmCoroutine);

        if (isFade)
            StartCoroutine(BgmFadeStop());
        else
            if (applyMusic != null)
                applyMusic.Stop();
    }

    public void BgmPause(bool state)
    {
        if (state)
            applyMusic.Pause();
        else
            applyMusic.Play();
    }
    public void SetBgmVolume(float _volume)
    {
        applyMusic.volume = _volume * bgmVolume;
    }

    IEnumerator BgmFadeStop()
    {
        float time = 0f;
        float isVolume = applyMusic.volume;
        while (time < 1f)
        {
            applyMusic.volume = Mathf.Lerp(isVolume, 0f, time);
            time += Time.deltaTime;
            yield return null;
        }
        //종료 후 볼륨 원래대로 초기화
        applyMusic.Stop();
        applyMusic.volume = isVolume;
    }

    public void BgmPlay(string bgmName)
    {
        int kind = bgmDict[bgmName];
        bgmSources[kind].Play();
        applyMusic = bgmSources[kind];
        applyMusic.loop = true;
        applyMusic.volume = bgmVolume;
    }

    public void SetNextBgm(string bgmName)
    {
        if (applyMusic.isPlaying)
        {
            applyMusic.loop = false;
            nextBgm = bgmName;
            if (nextBgmCoroutine != null)
                StopCoroutine(nextBgmCoroutine);
            nextBgmCoroutine = StartCoroutine(AutoPlay());
        }
        else
            BgmPlay(nextBgm);
    }

    IEnumerator AutoPlay()
    {
        while (applyMusic.isPlaying)
            yield return null;

        BgmPlay(nextBgm);
    }

    //사운드 초기화
    private void Start()
    {
        audioSources = new AudioSource[audioClips.Length];
        bgmSources = new AudioSource[bgmClips.Length];
        sfxDict = new Dictionary<string, int>();
        bgmDict = new Dictionary<string, int>();

        for (int i = 0; i < audioClips.Length; i++)
        {
            audioSources[i] = gameObject.AddComponent<AudioSource>();
            audioSources[i].clip = audioClips[i];
            audioSources[i].playOnAwake = false;
            sfxDict.Add(audioClips[i].name, i);
        }

        for (int i = 0; i < bgmClips.Length; i++)
        {
            bgmSources[i] = gameObject.AddComponent<AudioSource>();
            bgmSources[i].clip = bgmClips[i];
            bgmSources[i].playOnAwake = false;
            bgmSources[i].loop = true;
            bgmDict.Add(bgmClips[i].name, i);
        }

        BgmAutoPlay();

    }

    private void BgmAutoPlay()
    {
        switch (SceneManager.GetActiveScene().name)
        {
            case "LobbyScene":
                BgmPlay("LobbyMusic");
                break;
        }
    }
}
