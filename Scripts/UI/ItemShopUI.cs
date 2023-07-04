using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Random = UnityEngine.Random;
using UnityEngine.EventSystems;

namespace ItemNameSpace
{
    public class ItemData
    {
        string name; // �̸�
        int price; // ����
        Sprite icon; // ������

        #region Property
        public string Name { get { return name; } }
        public int Price { get { return price; } }
        public Sprite Icon { get { return icon; } }
        #endregion
        public ItemData(string name, int price, Sprite icon)
        {
            this.name = name;
            this.price = price;
            this.icon = icon;
        }
    }

    public class ItemShopUI : ShopUIMain
    {
        private const string MSG_HAVE_ITEM = "���� ���� �������� �ʹ� �����ϴ�!";

        private const int MAX_HAVE_ITEM = 7; // �÷��̾ ���� �� �ִ� �ִ� ������ ����
        private const int MAX_ITEM_LIST = 3; // ������ �Ǹ��ϴ� �������� �ִ� ����

        private static List<ItemData> itemList; // ��� �������� ������ ��� ����Ʈ
        private ItemData isEmptyItems; // ����ִ� ������ ������

        // ���� ������
        private ItemData[] saleItems; // �Ǹ����� �����۵�

        // ���� ������
        private ItemData[] playerItems; // �÷��̾� �����۵�
        private Image[] ownerItemIcons; // �÷��̾� ������ĭ�� ������
        private int ownerItemCount; // ���� ���� �������� ��

        // ���� ��
        [SerializeField] private Canvas serializeCanvas; // ���� ĵ����
        [SerializeField] private Image serializeBackdrop; // ���� ���
        [SerializeField] private Text serializeMessageText; // �޼��� �ؽ�Ʈ
        [SerializeField] private GameObject serializeRobotArm; // �κ� ��
        [SerializeField] private GameObject serializeRobotArmParticle; // �κ� �� ���� ����Ʈ

        [SerializeField] private Text[] saleItemNamesText; // �Ǹ� �������� �ؽ�Ʈ UI
        [SerializeField] private Text[] saleItemPricesText; // �Ǹ� �������� ���� UI
        [SerializeField] private Image[] saleItemIcons; // �Ǹ� �������� ������ UI
        [SerializeField] private Image[] ownerItemCases; // �÷��̾� ������ �׵θ�
        [SerializeField] private Sprite[] saleItemSprites; // �Ǹ� �������� ������ ����

        #region FirstInitialize
        private void Awake()
        {
            isEmptyItems = new ItemData("X", 0, saleItemSprites[0]); // �� ������ �� �ʱ�ȭ
            InitializeArrayData(); // �迭 ���� �ʱ�ȭ
            InitializeItemList(); // ������ ����Ʈ �ʱ�ȭ
        }

        private void InitializeArrayData()
        {
            saleItems = new ItemData[MAX_ITEM_LIST];
            playerItems = new ItemData[MAX_HAVE_ITEM];
            ownerItemIcons = new Image[MAX_HAVE_ITEM];
        }

        private void InitializeItemList()
        {
            if (itemList == null)
                itemList = new List<ItemData>();

            itemList.Add(new ItemData("�ڷ�����", 15, saleItemSprites[0]));
            itemList.Add(new ItemData("����", 10, saleItemSprites[1]));
            itemList.Add(new ItemData("����", 20, saleItemSprites[2]));
        }
        #endregion

        #region Initialize
        private void Start()
        {
            PassValues(); // �θ� �������� ������ �� �ű��
            SetComponents(); // ������Ʈ ����
            
            SetShopAllItem(); // ��� ĭ ������ ���� ��ġ
        }

        protected override void PassValues()
        {
            Canvas = serializeCanvas;
            Backdrop = serializeBackdrop;
            MessageText = serializeMessageText;
            RobotArm = serializeRobotArm;
            RobotArmParticle = serializeRobotArmParticle;
        }

        private void SetComponents()
        {
            for (int i = 0; i < ownerItemCases.Length; i++)
                ownerItemIcons[i] = ownerItemCases[i].transform.GetChild(0).GetComponentInChildren<Image>();
        }

        private void SetShopAllItem()
        {
            for (int i = 0; i < MAX_ITEM_LIST; i++)
                SetShopItem(i, GetRandomItem());
        }
        #endregion

        // �������� ������ ����Ʈ�� �����͸� �������� �Լ�
        private ItemData GetRandomItem()
        {
            int targetItem = Random.Range(0, itemList.Count - 1);
            ItemData temp = itemList[targetItem];
            itemList.RemoveAt(targetItem);
            return temp;
        }

        // ���ϴ� ĭ�� �������� ��ġ
        private void SetShopItem(int itemOrder, ItemData _item)
        {
            saleItems[itemOrder] = _item;
            saleItemNamesText[itemOrder].text = saleItems[itemOrder].Name;
            saleItemIcons[itemOrder].sprite = saleItems[itemOrder].Icon;
            if (saleItems[itemOrder].Price > 0)
                saleItemPricesText[itemOrder].text = saleItems[itemOrder].Price.ToString();
            else
                saleItemPricesText[itemOrder].text = "X";
        }

        // �÷��̾� ������ ĭ�� ���ϴ� ������ �߰�
        private void AddItem(ItemData _item)
        {
            for (int i = 0; i < playerItems.Length; i++)
            {
                if (playerItems[i] == null)
                {
                    ownerItemCount++;
                    playerItems[i] = _item;
                    ownerItemIcons[i].sprite = _item.Icon;
                    ownerItemIcons[i].gameObject.SetActive(true);

                    return;
                }
            }
        }

        // ������ ����
        public void BuyItem(int itemOrder)
        {
            // �� ���� Ȯ��
            if (GameManager.Instance.Screw >= saleItems[itemOrder].Price)
            {
                // ���� ������ �� ��
                if (ownerItemCount < MAX_HAVE_ITEM)
                {
                    GameManager.Instance.Screw -= saleItems[itemOrder].Price;

                    SoundManager.Instance.SFXPlay("Signal01");
                    GameManager.Instance.PlayerGetItem(saleItems[itemOrder]);
                    AddItem(saleItems[itemOrder]);

                    //���� ������ �� ������ ����
                    SetShopItem(itemOrder, isEmptyItems);
                }
                else
                    ShopMessage(MSG_HAVE_ITEM, 1.0f);
            }
            else
                ShopMessage(MSG_SCREW_LACK, 0.5f);
        }

        //������ �Ǹ�
        public void RemoveItem()
        {

        }

        #region OwnerItemEvents
        void OnBeginDrag(PointerEventData eventData)
        {
        }

        void OnDrag(PointerEventData eventData)
        {
        }

        void OnEndDrag(PointerEventData eventData)
        {
        }
        public override void ExitMenuButton()
        {
            ShowUI(false);
        }
        #endregion

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                ShowUI(true);
            }
        }
    }
}