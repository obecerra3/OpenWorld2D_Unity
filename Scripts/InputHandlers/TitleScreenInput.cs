using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Notifications;

//Input Handler for the Title Screen
public class TitleScreenInput : InputHandler
{
    //public button for starting game

    public TitleScreenInput()
    {
        //init buttons
    }

    public override void update()
    {
        //handle input
        if (Input.GetKeyDown("space"))
        {
            notify(SPACE_PRESSED, null);
        }
    }

}
