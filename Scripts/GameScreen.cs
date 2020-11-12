using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Storage;
using static States;
using static Notifications;
using static Utils;

public class GameScreen : Event
{
    Player player;
    World world;

    public GameScreen() {
        GameObject player_obj = GameObject.Find("Player");
        player = player_obj.AddComponent<Player>();
        world = new World();
    }

    public override void load() {
        if (!data_exists) {
            saveGame(player, world);
            loadGame();
        }
        player.load();
        world.load();
        player.addObserver(world);
    }

    public override void enable() {
        if (player == null && world == null) {
            load();
        }

        player.enable();
        world.enable();
    }

    public override void hide() {
        player.hide();
        world.hide();
    }

    public override void disable() {
        saveGame(player, world);
        player.disable();
        world.disable();
    }

    public override void onNotify(Notifications _notification, List<object> _data) {

    }

    public override void update() {
        world.update();
    }

}
