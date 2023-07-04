using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateScript : MonoBehaviour
{
    // ����Ʈ ��ǥ ����
    private const float CLOSE_HEIGHT = 0f;
    private const float OPEN_HEIGHT = 3f;

    // ����Ʈ ����, ���� ��� �ӵ�
    private const float GATE_MOTION_SPEED = 2f;

    public void ControlGate(bool _open)
    {
        StopAllCoroutines();
        StartCoroutine(OpenGameCoroutine(_open));
    }

    IEnumerator OpenGameCoroutine(bool _open)
    {
        Vector3 point = Vector3.forward * -2;
        float sPoint = CLOSE_HEIGHT, ePoint = OPEN_HEIGHT;
        float time = 0f;

        if (!_open)
        {
            sPoint = CLOSE_HEIGHT;
            ePoint = OPEN_HEIGHT;
        }
        while (time < 1)
        {
            point.y = Mathf.Lerp(sPoint, ePoint, time);
            transform.localPosition = point;
            time += Time.fixedDeltaTime * GATE_MOTION_SPEED;
            yield return new WaitForFixedUpdate();
        }
        point.y = ePoint;
        transform.localPosition = point;
    }
}
