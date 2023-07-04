using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BossPortal : MonoBehaviour
{
    private bool isAction = false; // Æ÷Å» Å¾½Â Áß

    void OnTriggerEnter(Collider other)
    {
        EnterPortal(other);
    }

    private void EnterPortal(Collider other)
    {
        if (!isAction && other.gameObject.CompareTag("Player"))
        {
            if (GameManager.DEBUG_MODE)
            {
                GameManager.Instance.isCinematic = true;
                SoundManager.Instance.BgmStop(false);
                UI.instance.ShowGameInterfaces(false);
                GameManager.Instance.isLobby = false;
                SceneManager.LoadScene("Boss" + GameManager.Instance.Stage + "Scene");
                return;
            }

            UI.instance.ShowGameInterfaces(false);
            GameManager.CameraInstance.CameraNoise(0.1f, 0.1f, 1f);
            SoundManager.Instance.SFXPlay("BossPortalSE");
            EffectManager.Instance.CreateEffect("PortalBlink", transform.position, Quaternion.identity, 2.0f);
            isAction = true;
            GameManager.Instance.isCinematic = true;
            SoundManager.Instance.BgmStop(true);
            other.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
            UI.instance.SetFilter(new Color(0, 0, 0, 0), new Color(0.8f, 0.8f, 1f, 0.6f), 0.8f);
            StartCoroutine(PortalLoading());
        }
    }

    IEnumerator PortalLoading()
    {
        float time = 0;
        Vector3 p_vec = GameManager.PlayerInstance.transform.position;
        Vector3 t_vec = transform.position;

        t_vec.y = p_vec.y;
        while (time < 1f)
        {
            GameManager.PlayerInstance.transform.position = Vector3.Lerp(p_vec, t_vec, time);
            time += Time.deltaTime * 4f;
            yield return null;
        }
        yield return new WaitForSeconds(0.75f);
        SoundManager.Instance.SFXPlay("BossPortalEndSE");
        GameManager.PlayerInstance.gameObject.SetActive(false);
        GameManager.Instance.isLobby = false;
        UI.instance.SetFilter(new Color(1, 1, 1, 0.96f), new Color(0, 0, 0, 1), 1f);
        yield return new WaitForSeconds(1.5f);
        isAction = false;
        SceneManager.LoadScene("Boss" + GameManager.Instance.Stage + "Scene");
    }
}
