using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageObjects : MonoBehaviour
{
    public int isDamage;

    private void OnParticleCollision(GameObject other)
    {
        if (other.tag == "Player")
        {
            IObjectInfo info = other.gameObject.GetComponent<IObjectInfo>();
            info.GetReceiveDamage(isDamage, Vector3.zero);
        }
    }
}
