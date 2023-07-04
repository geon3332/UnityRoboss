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
        string name; // 이름
        int price; // 가격
        Sprite icon; // 아이콘

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
        private const string MSG_HAVE_ITEM = "현재 가진 아이템이 너무 많습니다!";

        private const int MAX_HAVE_ITEM = 7; // 플레이어가 가질 수 있는 최대 아이템 개수
        private const int MAX_ITEM_LIST = 3; // 상점에 판매하는 아이템의 최대 개수

        private static List<ItemData> itemList; // 모든 아이템의 정보가 담긴 리스트
        private ItemData isEmptyItems; // 비어있는 아이템 데이터

        // 상점 아이템
        private ItemData[] saleItems; // 판매중인 아이템들

        // 보유 아이템
        private ItemData[] playerItems; // 플레이어 아이템들
        private Image[] ownerItemIcons; // 플레이어 아이템칸의 아이콘
        private int ownerItemCount; // 현재 가진 아이템의 수

        // 전달 값
        [SerializeField] private Canvas serializeCanvas; // 메인 캔버스
        [SerializeField] private Image serializeBackdrop; // 메인 배경
        [SerializeField] private Text serializeMessageText; // 메세지 텍스트
        [SerializeField] private GameObject serializeRobotArm; // 로봇 팔
        [SerializeField] private GameObject serializeRobotArmParticle; // 로봇 팔 용접 이펙트

        [SerializeField] private Text[] saleItemNamesText; // 판매 아이템의 텍스트 UI
        [SerializeField] private Text[] saleItemPricesText; // 판매 아이템의 가격 UI
        [SerializeField] private Image[] saleItemIcons; // 판매 아이템의 아이콘 UI
        [SerializeField] private Image[] ownerItemCases; // 플레이어 아이템 테두리
        [SerializeField] private Sprite[] saleItemSprites; // 판매 아이템의 아이콘 모음

        #region FirstInitialize
        private void Awake()
        {
            isEmptyItems = new ItemData("X", 0, saleItemSprites[0]); // 빈 아이템 값 초기화
            InitializeArrayData(); // 배열 변수 초기화
            InitializeItemList(); // 아이템 리스트 초기화
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

            itemList.Add(new ItemData("텔레포터", 15, saleItemSprites[0]));
            itemList.Add(new ItemData("바퀴", 10, saleItemSprites[1]));
            itemList.Add(new ItemData("쉴드", 20, saleItemSprites[2]));
        }
        #endregion

        #region Initialize
        private void Start()
        {
            PassValues(); // 부모 프레임의 변수로 값 옮기기
            SetComponents(); // 컴포넌트 설정
            
            SetShopAllItem(); // 모든 칸 아이템 랜덤 배치
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

        // 랜덤으로 아이템 리스트의 데이터를 가져오는 함수
        private ItemData GetRandomItem()
        {
            int targetItem = Random.Range(0, itemList.Count - 1);
            ItemData temp = itemList[targetItem];
            itemList.RemoveAt(targetItem);
            return temp;
        }

        // 원하는 칸의 아이템을 배치
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

        // 플레이어 아이템 칸에 원하는 아이템 추가
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

        // 아이템 구입
        public void BuyItem(int itemOrder)
        {
            // 돈 여부 확인
            if (GameManager.Instance.Screw >= saleItems[itemOrder].Price)
            {
                // 가진 아이템 수 비교
                if (ownerItemCount < MAX_HAVE_ITEM)
                {
                    GameManager.Instance.Screw -= saleItems[itemOrder].Price;

                    SoundManager.Instance.SFXPlay("Signal01");
                    GameManager.Instance.PlayerGetItem(saleItems[itemOrder]);
                    AddItem(saleItems[itemOrder]);

                    //상점 아이템 빈 값으로 변경
                    SetShopItem(itemOrder, isEmptyItems);
                }
                else
                    ShopMessage(MSG_HAVE_ITEM, 1.0f);
            }
            else
                ShopMessage(MSG_SCREW_LACK, 0.5f);
        }

        //아이템 판매
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