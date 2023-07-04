using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeShop : ShopUIMain
{
    private const string DMG_POWER_TXT = "공격력: ";
    private const string ATK_SPEED_TXT = "공격 속도: ";
    private const string MOVE_SPEED_TXT = "이동 속도: ";
    private const string HEALTH_TXT = "체력: ";

    private const string MSG_MAX_UPGRADE = "더 이상 강화할 수 없습니다!";

    private const int PRICE_DMG_POWER = 2;
    private const int PRICE_ATK_SPEED = 2;
    private const int PRICE_MOVE_SPEED = 1;
    private const int PRICE_HEALTH = 4;

    private const int MAX_DMG_POWER = 18;
    private const int MAX_ATK_SPEED = 18;
    private const int MAX_MOVE_SPEED = 18;
    private const int MAX_HEALTH = 9;

    [SerializeField] private Canvas canvas;
    [SerializeField] private Image backdrop;
    [SerializeField] private Image dmgPowerGauge;
    [SerializeField] private Image atkSpeedGauge;
    [SerializeField] private Image moveSpeedGauge;
    [SerializeField] private Text dmgPowerText;
    [SerializeField] private Text atkSpeedText;
    [SerializeField] private Text moveSpeedText;
    [SerializeField] private Text healthPowerText;
    [SerializeField] private Text messageText;
    [SerializeField] private Slider healthGauge;
    [SerializeField] private Button exitButton;
    [SerializeField] private GameObject robotArm;
    [SerializeField] private GameObject robotArmParticle;


    //상점 UI 보이기/끄기

    private void Start()
    {
        PassValues();
        SetLevelText();
    }

    protected override void PassValues()
    {
        Canvas = canvas;
        Backdrop = backdrop;
        MessageText = messageText;
        RobotArm = robotArm;
        RobotArmParticle = robotArmParticle;
    }
    public override void ExitMenuButton()
    {
        ShowUI(false);
    }

    private void SetLevelText()
    {
        dmgPowerText.text = DMG_POWER_TXT + "LV" + GameManager.Instance.isDamageUpgrade;
        atkSpeedText.text = ATK_SPEED_TXT + "LV" + GameManager.Instance.isAtkSpeedUpgrade;
        moveSpeedText.text = MOVE_SPEED_TXT + "LV" + GameManager.Instance.isMoveSpeedUpgrade;
        healthPowerText.text = HEALTH_TXT + "LV" + GameManager.Instance.isHealthUpgrade;
    }

    public void UpgradeAttackPower()
    {
        if (GameManager.Instance.Screw < PRICE_DMG_POWER)
            ShopMessage(MSG_SCREW_LACK, 0.5f);
        else if(GameManager.Instance.isDamageUpgrade >= MAX_DMG_POWER)
            ShopMessage(MSG_MAX_UPGRADE, 1f);
        else
        {
            SoundManager.Instance.SFXPlay("Signal01");
            GameManager.Instance.isDamageUpgrade++;
            GameManager.Instance.Screw -= PRICE_DMG_POWER;
            dmgPowerGauge.fillAmount = (float)GameManager.Instance.isDamageUpgrade / 18;
            dmgPowerText.text = DMG_POWER_TXT + "LV" + GameManager.Instance.isDamageUpgrade;
            GetRobotArmWork();
        }
    }

    public void UpgradeAtkSpeed()
    {
        if (GameManager.Instance.Screw < PRICE_ATK_SPEED)
            ShopMessage(MSG_SCREW_LACK, 0.5f);
        else if (GameManager.Instance.isAtkSpeedUpgrade >= MAX_ATK_SPEED)
            ShopMessage(MSG_MAX_UPGRADE, 1f);
        else
        {
            GameManager.Instance.isAtkSpeedUpgrade++;
            GameManager.PlayerInstance.AttackSpeed = Player.BASIC_ATTACK_SPEED - (GameManager.Instance.isAtkSpeedUpgrade * 0.02f);
            SoundManager.Instance.SFXPlay("Signal01");
            GameManager.Instance.Screw -= PRICE_ATK_SPEED;
            atkSpeedGauge.fillAmount = (float)GameManager.Instance.isAtkSpeedUpgrade / 18;
            atkSpeedText.text = ATK_SPEED_TXT + "LV" + GameManager.Instance.isAtkSpeedUpgrade;
            GetRobotArmWork();
        }
    }

    public void UpgradeMoveSpeed()
    {
        if (GameManager.Instance.Screw < PRICE_MOVE_SPEED)
            ShopMessage(MSG_SCREW_LACK, 0.5f);
        else if (GameManager.Instance.isMoveSpeedUpgrade >= MAX_MOVE_SPEED)
            ShopMessage(MSG_MAX_UPGRADE, 1f);
        else
        {
            GameManager.Instance.isMoveSpeedUpgrade++;
            GameManager.PlayerInstance.MoveSpeed = Player.BASIC_MOVE_SPEED + (GameManager.Instance.isMoveSpeedUpgrade * 0.2f);
            SoundManager.Instance.SFXPlay("Signal01");
            GameManager.Instance.Screw -= PRICE_MOVE_SPEED;
            moveSpeedGauge.fillAmount = (float)GameManager.Instance.isMoveSpeedUpgrade / 18;
            moveSpeedText.text = MOVE_SPEED_TXT + "LV" + GameManager.Instance.isMoveSpeedUpgrade;
            GetRobotArmWork();
        }
    }

    public void UpgradeHealth()
    {
        if (GameManager.Instance.Screw < PRICE_HEALTH)
            ShopMessage(MSG_SCREW_LACK, 0.5f);
        else if (GameManager.Instance.isHealthUpgrade >= MAX_HEALTH)
            ShopMessage(MSG_MAX_UPGRADE, 1f);
        else
        {
            GameManager.Instance.isHealthUpgrade++;
            GameManager.PlayerInstance.MaxHealth = GameManager.Instance.isHealthUpgrade + Player.BASIC_HEALTH;
            GameManager.PlayerInstance.Health = GameManager.PlayerInstance.MaxHealth * 4;

            SoundManager.Instance.SFXPlay("Signal01");
            GameManager.Instance.Screw -= PRICE_HEALTH;
            healthGauge.value = GameManager.Instance.isHealthUpgrade * 0.11f;
            healthPowerText.text = HEALTH_TXT + "LV" + GameManager.Instance.isHealthUpgrade;
            GetRobotArmWork();
        }
    }

    // 상점 입구
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            ShowUI(true);
        }
    }
}
