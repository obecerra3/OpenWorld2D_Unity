using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class Observer
{
    public virtual void onNotify(Notifications _notification, List<object> _data) {}

}
