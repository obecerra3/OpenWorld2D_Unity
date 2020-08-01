using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Storage;
using static States;
using static Notifications;

public class Main : MonoBehaviour
{
    TitleScreen title_screen;
    GameScreen game_screen;

    Event current_screen;

    void Start()
    {
        //set the RNG seed
        Random.InitState(69);

        //read save data
        loadGame();

        //initialize screens
        title_screen = new TitleScreen();
        game_screen = new GameScreen();

        //render screens
        title_screen.load();
        game_screen.load();

        //enable title screen
        current_screen = title_screen;
        title_screen.enable();
    }

    void Update()
    {
        if (current_screen != null)
        {
            if (current_screen.current_state == EXPIRED)
            {
                switch (current_screen.next_event)
                {
                    case ("TITLE"):
                        changeScreen(title_screen);
                        break;
                    case ("GAME"):
                        changeScreen(game_screen);
                        break;
                }
            }

            current_screen.update();
        }

    }

    void changeScreen(Event _screen)
    {
        current_screen.disable();
        current_screen = _screen;
        current_screen.enable();
    }
}
