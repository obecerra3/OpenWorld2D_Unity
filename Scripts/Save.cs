using System.Collections;
using System.Collections.Generic;
using Structs;

[System.Serializable]
public class Save
{
    public PlayerSave player_save = new PlayerSave();
    public WorldSave world_save = new WorldSave();

    public Save(Player _player, World _world)
    {
        player_save.name = _player.name;
        player_save.position = _player.position;
        player_save.health = _player.health;
        player_save.mana = _player.mana;
        player_save.max_health = _player.max_health;
        player_save.max_mana = _player.max_mana;
    }

}
