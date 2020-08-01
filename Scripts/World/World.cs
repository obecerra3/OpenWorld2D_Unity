using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static TilemapEngine;
using static Notifications;
using static Storage;

[System.Serializable]
public class World : Event
{
    public World()
    {
        TilemapEngine.initialize();
    }

    public override void load()
    {
        TilemapEngine.load();
    }

    public override void render()
    {
        TilemapEngine.render();
    }

    public override void hide()
    {
        TilemapEngine.hide();
    }

    public override void enable()
    {
        render();
    }

    public override void disable()
    {

    }

    public override void onNotify(Notifications _notification, List<object> _data)
    {
        switch (_notification)
        {
            case PLAYER_POS_CHANGED:
                TilemapEngine.checkPlayerPosition((Vector2Int) _data[0]);
                break;
        }
    }

    public override void update()
    {

    }

}
