using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

abstract public class ShopUIMain : MonoBehaviour
{
    protected const string MSG_SCREW_LACK = "나사가 부족합니다!";

    private const float ROBOT_ARM_REPAIR_TIME = 1.0f; // 로봇 팔 수리 모션 시간
    private const float SHOW_MENU_TIME = 0.1f; // 메뉴 등장 시간
    private const float FADE_MOVE_TIME = 0.25f; // 플레이어가 상점 중앙에 도달하는 시간

    private bool isShopDelay = false; // 상점 버튼 딜레이
    private float robotArmFixTime = 0; // 로봇 팔 수리 시간 카운트
    
    // UI
    private Canvas canvas;
    private Text messageText;
    private Image backdrop;

    // 코루틴
    private Coroutine robotArmCoroutine;
    private Coroutine messageCoroutine;

    // 로봇 팔
    private GameObject robotArm;
    private GameObject robotArmParticle;

    #region propertys
    protected Canvas Canvas { set { canvas = value; } }
    protected Text MessageText { set { messageText = value; } }
    protected Image Backdrop { set { backdrop = value; } }
    protected GameObject RobotArm { set { robotArm = value; } }
    protected GameObject RobotArmParticle { set { robotArmParticle = value; } }
    #endregion

    protected abstract void PassValues();
    public abstract void ExitMenuButton();

    #region MenuUI
    protected void ShowUI(bool _show)
    {
        if (!isShopDelay)
        {
            SoundManager.Instance.SFXPlay("Denied3");
            isShopDelay = true;
            GameManager.Instance.isShop = _show;
            if (_show)
            {
                canvas.gameObject.SetActive(_show);
                StartCoroutine(ShopUISizeFade(0f, 1f));
                StartCoroutine(ShopFadeMove());
                StartCoroutine(ShopUIHotkey());
            }
            else
            {
                if (robotArmCoroutine != null)
                {
                    StopCoroutine(robotArmCoroutine);
                    robotArmCoroutine = null;
                    StartCoroutine(RobotArmWorkEnd());
                }

                StartCoroutine(ShopUISizeFade(1f, 0f));
            }
        }
    }

    IEnumerator ShopUISizeFade(float firstSize, float lastSize)
    {
        float time = 0;
        Color _color = backdrop.color;

        while (time < SHOW_MENU_TIME)
        {
            _color.a = Mathf.Lerp(firstSize, lastSize, time / SHOW_MENU_TIME);
            backdrop.color = _color;
            backdrop.transform.localScale = new Vector3(1, Mathf.Lerp(firstSize, lastSize, time / SHOW_MENU_TIME), 1);
            time += Time.deltaTime;
            yield return null;
        }
        backdrop.transform.localScale = Vector3.one;
        isShopDelay = false;

        if (lastSize == 0)
            canvas.gameObject.SetActive(false);
        else
        {
            _color.a = 1;
            backdrop.color = _color;
        }
    }

    IEnumerator ShopFadeMove()
    {
        float time = 0;
        Vector3 p_vec = GameManager.PlayerInstance.transform.position;
        Vector3 t_vec = transform.position;

        t_vec.y = p_vec.y;
        while (time < FADE_MOVE_TIME)
        {
            GameManager.PlayerInstance.transform.position = Vector3.Lerp(p_vec, t_vec, time / FADE_MOVE_TIME);
            time += Time.deltaTime;
            yield return null;
        }
        GameManager.PlayerInstance.transform.position = t_vec;
    }

    // ESC로 상점 메뉴 닫기
    IEnumerator ShopUIHotkey()
    {
        while (GameManager.Instance.isShop)
        {
            if (Input.GetKey(KeyCode.Escape))
                ShowUI(false);

            yield return null;
        }
    }
    #endregion

    #region RobotArm
    protected void GetRobotArmWork()
    {
        if (robotArmCoroutine != null)
        {
            robotArmFixTime = ROBOT_ARM_REPAIR_TIME;
        }
        else
            robotArmCoroutine = StartCoroutine(RobotArmWork());
    }

    IEnumerator RobotArmWork()
    {
        float angle = 0f;

        while (angle < 20f)
        {
            robotArm.transform.rotation = Quaternion.Euler(new Vector3(angle, 90, 0));
            angle += Time.deltaTime * 100f;
            yield return null;
        }
        robotArm.transform.rotation = Quaternion.Euler(new Vector3(20f, 90, 0));

        robotArmParticle.gameObject.SetActive(true);
        SoundManager.Instance.SFXPlay("WeldingSE", true);
        robotArmFixTime = ROBOT_ARM_REPAIR_TIME;
        while (robotArmFixTime > 0f)
        {
            robotArmFixTime -= Time.deltaTime;
            yield return null;
        }
        robotArmFixTime = 0f;
        robotArmCoroutine = null;
        StartCoroutine(RobotArmWorkEnd());
    }

    // 로봇 팔 작동 종료
    IEnumerator RobotArmWorkEnd()
    {
        robotArmParticle.gameObject.SetActive(false);
        SoundManager.Instance.SFXStop("WeldingSE");

        float angle = 20f;
        while (angle > 0)
        {
            robotArm.transform.rotation = Quaternion.Euler(new Vector3(angle, 90, 0));
            angle -= Time.deltaTime * 100f;
            yield return null;
        }
        robotArm.transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
    }
    #endregion

    #region MessageText
    protected void ShopMessage(string msg, float time)
    {
        if (messageCoroutine != null)
            StopCoroutine(messageCoroutine);
        SoundManager.Instance.SFXPlay("Denied3");
        messageText.gameObject.SetActive(true);
        messageText.text = msg;
        messageText.color = Color.white;

        messageCoroutine = StartCoroutine(ShopMessageFade(time));
    }

    IEnumerator ShopMessageFade(float waitTime)
    {
        float time = 0f;
        yield return new WaitForSeconds(waitTime);
        while (time < 1f)
        {
            messageText.color = Color.Lerp(Color.white, new Color(1, 1, 1, 0), time);
            time += Time.deltaTime * 2f;
            yield return null;
        }
        messageText.gameObject.SetActive(false);
        messageCoroutine = null;
    }
    #endregion
}
