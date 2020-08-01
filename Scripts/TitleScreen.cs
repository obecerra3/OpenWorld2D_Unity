using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Storage;
using static States;
using static Notifications;

//graphic/interactive content for title screen
public class TitleScreen : Event
{
    TitleScreenInput input_handler;

    public TitleScreen()
    {

    }

    public override void load()
    {

    }

    public override void enable()
    {
        input_handler = new TitleScreenInput();
        input_handler.addObserver(this);

        //skip to game
        current_state = EXPIRED;
        next_event = "GAME";
    }

    public override void disable()
    {
        //hide and disable ui
    }

    public override void onNotify(Notifications _notification, List<object> _data)
    {
        //determine current_state
        switch(current_state)
        {
            case (COMPANY_INTRO):
                switch (_notification)
                {
                    case (ANYTHING_PRESSED):
                    changeState(INTRO);
                    break;
                }
                break;
        }
    }

    public void changeState(States next_state)
    {
        //disable current_state (graphics and ui)

        //draw the next_state's graphics/ enable it
        switch (next_state)
        {
            case (INTRO):
                //draw Intro graphic content
                break;
        }
    }

    public override void update()
    {
        input_handler.update();
    }

}
