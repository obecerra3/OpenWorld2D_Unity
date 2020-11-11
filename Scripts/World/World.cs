using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static TilemapManager;
using static Notifications;
using static Storage;

[System.Serializable]
public class World : Event
{
    public World()
    {
        TilemapManager.initialize();
    }

    public override void load()
    {
        TilemapManager.load();
    }

    public override void render()
    {
        TilemapManager.render();
    }

    public override void hide()
    {
        TilemapManager.hide();
    }

    public override void enable()
    {
        render();
    }

    public override void disable()
    {

    }

    public override void onNotify(Notifications notification, List<object> data)
    {
        switch (notification)
        {
            case PLAYER_POS_CHANGED:
                TilemapManager.checkPlayerPosition((Vector2Int) data[0]);
                break;
        }
    }

    public override void update()
    {

    }

}
