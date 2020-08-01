using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InputHandler : Subject
{
    public bool isEnabled;

    public abstract void update();

}
