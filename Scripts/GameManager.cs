using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ItemNameSpace;

public class GameManager : MonoBehaviour
{
    public static readonly bool DEBUG_MODE = true;
    private const int MAX_OWNER_ITEMS = 6;

    private List<ItemData> ownerItems;

    private static GameManager instance;

    private static Player instancePlayer;
    private static CameraScript instanceCamera;

    [SerializeField] private int isStage = 1;
    [SerializeField] private int isScrew = 10; // 돈

    [HideInInspector] public bool isGamePause = false;

    [HideInInspector] public bool isLobby = true; // 대기실인지 체크
    [HideInInspector] public bool isShop = false; // 상점 이용 중인지 체크
    [HideInInspector] public bool isCinematic = false; // 플레이어 컨트롤 가능 여부
    [HideInInspector] public bool isMusicEnable = true; // 배경 음악 설정
    [HideInInspector] public bool isSoundEnable = true; // 효과음 설정

    [HideInInspector] public float isCamNoisePower = 1f; // 카메라 진동 세기

    [HideInInspector] public int isDamageUpgrade = 0; // 데미지 업그레이드 횟수
    [HideInInspector] public int isAtkSpeedUpgrade = 0; // 공격 속도 업그레이드 횟수
    [HideInInspector] public int isHealthUpgrade = 0; // 체력 업그레이드 횟수
    [HideInInspector] public int isMoveSpeedUpgrade = 0; // 이동속도 업그레이드 횟수

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

    private void Start()
    {
        InitializeVariables();
    }

    // 초기화
    public void InitializeVariables()
    {
        isGamePause = false;

        isLobby = true;
        isShop = false;
        isCinematic = false;
        isMusicEnable = true;
        isSoundEnable = true;

        isCamNoisePower = 1f;

        isDamageUpgrade = 0;
        isAtkSpeedUpgrade = 0;
        isHealthUpgrade = 0;
        isMoveSpeedUpgrade = 0;

        if (ownerItems == null)
            ownerItems = new List<ItemData>();
        else
            ownerItems.Clear();

        Stage = 1;
        Screw = 10;
    }

    #region property
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
                instance = new GameManager();
            return instance;
        }

    }
    public int Stage
    {
        get
        {
            return isStage;
        }

        set
        {
            isStage = Mathf.Clamp(value, 1, 9999);
            UI.instance.SetStageText(isStage);
        }

    }
    public int Screw
    {
        get
        {
            return isScrew;
        }

        set
        {
            isScrew = Mathf.Clamp(value, 0, 9999);
            UI.instance.SetScrewText(isScrew);
        }

    }

    public static Player PlayerInstance
    {
        get
        {
            if (instancePlayer == null)
                instancePlayer = FindObjectOfType<Player>();
            return instancePlayer;
        }
    }
    public static CameraScript CameraInstance
    {
        get
        {
            if (instanceCamera == null)
                instanceCamera = FindObjectOfType<CameraScript>();
            return instanceCamera;
        }
    }
    #endregion

    public void PlayerGetItem(ItemData _item)
    {
        if (ownerItems.Count < MAX_OWNER_ITEMS)
            ownerItems.Add(_item);
    }

    public void LoadLobbyScene()
    {
        SceneManager.LoadScene("LobbyScene");
        isLobby = true;
        UI.instance.SetFilter(new Color(0, 0, 0, 1), new Color(0, 0, 0, 0), 1.5f);
        UI.instance.ShowGameInterfaces(true);
        UI.instance.SetBossHealthUIEnable(false);
        SoundManager.Instance.BgmPlay("LobbyMusic");
    }


    private void ESCMenu()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!EscMenu.isEnable)
                UI.EscapeMenu.ShowESCMenu();
            else
                UI.EscapeMenu.OptionMenu(false);
        }
    }

    public void GameQuit()
    {
        Application.Quit();
    }

    private void Update()
    {
        ESCMenu();
        DebugCommand();
    }

    // 테스트 기능
    private void DebugCommand()
    {
        if (DEBUG_MODE)
        {
            if (Input.GetKeyDown(KeyCode.F1))
                Stage--;
            else if (Input.GetKeyDown(KeyCode.F2))
                Stage++;
            else if (Input.GetKeyDown(KeyCode.F3))
                Screw = 1000;
        }
    }
}