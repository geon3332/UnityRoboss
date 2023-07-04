using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour
{
    private const float ROBOT_ARM_REPAIR_TIME = 1.0f;
    private const float ROBOT_ARM_SPIN_VALUE = 20f;
    private const float ROBOT_ARM_SPIN_SPEED = 100f;

    private float robotArmFixTime = 0;
    private Coroutine robotArmCoroutine;
    protected GameObject robotArm;
    protected GameObject robotArmParticle;

    // ∑Œ∫ø ∆» ¿€µø
    private void GetRobotArmWork()
    {
        if (robotArmCoroutine != null)
            robotArmFixTime = ROBOT_ARM_REPAIR_TIME;
        else
            robotArmCoroutine = StartCoroutine(RobotArmWork());
    }

    // ∑Œ∫ø ∆» ¿€µø
    IEnumerator RobotArmWork()
    {
        float angle = 0f;

        while (angle < ROBOT_ARM_SPIN_VALUE)
        {
            robotArm.transform.rotation = Quaternion.Euler(new Vector3(angle, 90, 0));
            angle += Time.deltaTime * ROBOT_ARM_SPIN_SPEED;
            yield return null;
        }
        robotArm.transform.rotation = Quaternion.Euler(new Vector3(ROBOT_ARM_SPIN_VALUE, 90, 0));

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

    // ∑Œ∫ø ∆» ¿€µø ¡æ∑·
    IEnumerator RobotArmWorkEnd()
    {
        robotArmParticle.gameObject.SetActive(false);
        SoundManager.Instance.SFXStop("WeldingSE");

        float angle = ROBOT_ARM_SPIN_VALUE;
        while (angle > 0)
        {
            robotArm.transform.rotation = Quaternion.Euler(new Vector3(angle, 90, 0));
            angle -= Time.deltaTime * ROBOT_ARM_SPIN_SPEED;
            yield return null;
        }
        robotArm.transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
    }
}
