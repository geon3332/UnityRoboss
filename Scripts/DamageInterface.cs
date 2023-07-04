using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IObjectInfo
{
    bool isDead { get; set; }
    void GetReceiveDamage(float damage, Vector3 angle);
}