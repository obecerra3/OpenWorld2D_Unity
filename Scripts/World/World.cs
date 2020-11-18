using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static TilemapManager;
using static Notifications;
using static Storage;

[System.Serializable]
public class World : Event {

    public World() {
        TilemapManager.initialize();
        Terrain.initialize();
        Biomes.BiomeSpawn.initialize();
        Plants.PlantSpawn.initialize();
    }

    public override void load() {
        TilemapManager.load();
        Terrain.load();
        Biomes.BiomeSpawn.load();
        Plants.PlantSpawn.load();
    }

    public void reload(Vector2Int new_center) {
        TilemapManager.reload(new_center);
        Terrain.reload();
        Biomes.BiomeSpawn.reload();
        Plants.PlantSpawn.reload();
        render();
    }

    public override void render() {
        TilemapManager.render();
    }

    public override void hide() {
        TilemapManager.hide();
    }

    public override void enable() {
        render();
    }

    public override void disable() {

    }

    public void onNotify(Notifications notification, List<object> data) {
        switch (notification) {
            case PLAYER_POS_CHANGED:
                Vector2Int new_pos = (Vector2Int) data[0];
                if (TilemapManager.checkPlayerPosition(new_pos)) {
                    notify(FREEZE_PLAYER_RB, null);
                    reload(new_pos);
                    notify(UNFREEZE_PLAYER_RB, null);
                }
                break;
        }
    }

    public override void update() {

    }

}
