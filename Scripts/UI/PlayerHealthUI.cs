using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    private Image[] healthCase; // �÷��̾� ü�� ���̽�
    private Image[] healthCont; // �÷��̾� ��Ʈ

    void Start()
    {
        // ������Ʈ ����
        SetComponents();

        // �÷��̾� ü�� ǥ��
        SetMaxHealthUI(GameManager.PlayerInstance.MaxHealth);
    }

    private void SetComponents()
    {
        int _max = gameObject.transform.childCount;
        healthCase = new Image[_max];
        healthCont = new Image[_max];
        for (int i = 0; i < _max; i++)
        {
            healthCase[i] = transform.GetChild(i).GetComponentInChildren<Image>();
            healthCont[i] = healthCase[i].transform.GetChild(0).GetComponentInChildren<Image>();
        }
    }

    //ü�� UI ������Ʈ
    public void SetHealthUI(int _health)
    {
        for (int i = 0; i < healthCont.Length; i++)
        {
            int max = (i + 1) * 4;
            if (max <= _health)
                healthCont[i].fillAmount = 1f;
            else if (max - 1 == _health)
                healthCont[i].fillAmount = 0.75f;
            else if (max - 2 == _health)
                healthCont[i].fillAmount = 0.5f;
            else if (max - 3 == _health)
                healthCont[i].fillAmount = 0.25f;
            else
                healthCont[i].fillAmount = 0f;
        }
    }

    public void SetMaxHealthUI(int _maxHealth)
    {
        for (int i = 0; i < healthCont.Length; i++)
            healthCase[i].gameObject.SetActive(i < _maxHealth);
    }
}
