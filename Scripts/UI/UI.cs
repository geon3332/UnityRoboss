using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class UI : MonoBehaviour
{
    public static UI instance;
    static private SkillUI instanceSkillUI;
    static private PlayerHealthUI instancePlayerHealthUI;
    static private EscMenu instanceEscMenu;
    static private CinematicUI instanceCinematic;
    static private GameOverUI instanceGameOverUI;

    #region singleton
    private void Awake()
    {
        if (instance == null)
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

    private Coroutine filterCo; // ȭ�� ���� �ڷ�ƾ
    private Coroutine polishCoroutine;

    private Image bossHealthCase; // ���� ü�¹� ���
    private Image bossHealthBar; // ���� ü�¹�
    private Image bossManaCase; // ���� ü�¹� ���
    private Image bossManaBar; // ���� ü�¹�

    [SerializeField] private GameObject gameInterfaces; // ���͸� ������ ��� UI
    [SerializeField] private GameObject bossUIPanel; // ���� UI
    [SerializeField] private Image escFilter;
    [SerializeField] private Text screwValueText;
    [SerializeField] private Text stageText;

    [SerializeField] private Image filter;
    [SerializeField] private RectTransform healthPolish;
    [SerializeField] private RectTransform manaPolish;

    #region Property
    public static SkillUI Skill
    {
        get
        {
            if (instanceSkillUI == null)
                instanceSkillUI = FindObjectOfType<SkillUI>();
            return instanceSkillUI;
        }
    }
    public static PlayerHealthUI PlayerHealth
    {
        get
        {
            if (instancePlayerHealthUI == null)
                instancePlayerHealthUI = FindObjectOfType<PlayerHealthUI>();
            return instancePlayerHealthUI;
        }
    }
    public static EscMenu EscapeMenu
    {
        get
        {
            if (instanceEscMenu == null)
                instanceEscMenu = FindObjectOfType<EscMenu>();
            return instanceEscMenu;
        }
    }
    public static CinematicUI Cinematic
    {
        get
        {
            if (instanceCinematic == null)
                instanceCinematic = FindObjectOfType<CinematicUI>();
            return instanceCinematic;
        }
    }
    public static GameOverUI GameOver
    {
        get
        {
            if (instanceGameOverUI == null)
                instanceGameOverUI = FindObjectOfType<GameOverUI>();
            return instanceGameOverUI;
        }
    }
    #endregion

    public void SetStageText(int _stage)
    {
        stageText.text = "Stage: " + _stage;
    }

    void Start()
    {
        bossHealthCase = bossUIPanel.transform.GetChild(0).GetComponentInChildren<Image>();
        bossHealthBar = bossHealthCase.transform.GetChild(0).GetComponent<Image>();
        bossManaCase = bossUIPanel.transform.GetChild(1).GetComponentInChildren<Image>();
        bossManaBar = bossManaCase.transform.GetChild(0).GetComponent<Image>();
        //bossHealthBarText = bossHealthBar.GetComponentInChildren<Text>();

        //�÷��̾� �� ǥ��
        SetScrewText(GameManager.Instance.Screw);

        LobbyFadeFilter();
    }

    private void LobbyFadeFilter()
    {
        if (SceneManager.GetActiveScene().name == "LobbyScene")
            SetFilter(Color.black, new Color(0, 0, 0, 0), 2.0f);
    }

    public void ShowGameInterfaces(bool _show)
    {
        gameInterfaces.gameObject.SetActive(_show);
    }

    //���� ü�� UI ����
    public void SetBossHealthUIEnable(bool state)
    {
        bossUIPanel.gameObject.SetActive(state);
        if (state)
            polishCoroutine = StartCoroutine(HealthPolish());
        else
        {
            if (polishCoroutine != null)
            {
                StopCoroutine(polishCoroutine);
                polishCoroutine = null;
            }
        }
    }

    IEnumerator HealthPolish()
    {
        float posY = -300f;
        while (true)
        {
            healthPolish.anchoredPosition = new Vector2(0, posY);
            manaPolish.anchoredPosition = new Vector2(0, posY);
            posY += Time.deltaTime * 300;
            if (posY > 300)
                posY = -300;
            yield return null;
        }
    }

    // ���� ü�¹� ������Ʈ
    public void SetBossHealthUI(float applyHp, float maxHp)
    {
        bossHealthBar.fillAmount = applyHp / maxHp;
        //bossHealthBarText.text = (int)applyHp + " / " + (int)maxHp;
    }
    public void SetBossManaUI(float applyMp, float maxMp)
    {
        bossManaBar.fillAmount = applyMp / maxMp;
    }

    public void SetFilter(Color firstColor, Color endColor, float time)
    {
        //�̹� �������̸� ����
        if (filterCo != null)
            StopCoroutine(filterCo);

        if (time > 0)
        {
            filter.color = firstColor;
            filterCo = StartCoroutine(FilterCoroutine(firstColor, endColor, time));
        }
        else
            filter.color = endColor;
    }

    IEnumerator FilterCoroutine(Color firstColor, Color endColor, float time)
    {
        float maxTime = time;
        while (time > 0)
        {
            filter.color = Color.Lerp(endColor, firstColor, time / maxTime);
            time -= Time.deltaTime;
            yield return null;
        }

        filterCo = null;
    }


    public void SetEscFilterEnable(bool enable)
    {
        escFilter.gameObject.SetActive(enable);
    }

    public void SetScrewText(int _value)
    {
        screwValueText.text = _value.ToString();
    }
}
