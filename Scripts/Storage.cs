using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class Storage {
    public static int active_save = 0;
    public static Save[] saves = new Save[3];
    public static bool data_exists;

    public static void saveGame(Player _player, World _world) {
        Save save = new Save(_player, _world);
        BinaryFormatter bf = new BinaryFormatter();
        if (!Directory.Exists(Application.persistentDataPath + "/saves/")) {
            Directory.CreateDirectory(Application.persistentDataPath + "/saves/");
        }
        string save_file = "/saves/gamesave"+active_save+".save";
        FileStream file = File.Create(Application.persistentDataPath + save_file);
        bf.Serialize(file, save);
        file.Close();

        Debug.Log("Storage: Game Saved");
    }

    public static void loadGame() {
        if (File.Exists(Application.persistentDataPath + "/saves/gamesave0.save")) {
            Debug.Log("Storage: Save Data Exists");
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/saves/gamesave0.save", FileMode.Open);
            saves[0] = (Save) bf.Deserialize(file);
            file.Close();

            if (File.Exists(Application.persistentDataPath + "/saves/gamesave1.save")) {
                file = File.Open(Application.persistentDataPath + "/saves/gamesave1.save", FileMode.Open);
                saves[1] = (Save) bf.Deserialize(file);
                file.Close();
            }

            if (File.Exists(Application.persistentDataPath + "/saves/gamesave2.save")) {
                file = File.Open(Application.persistentDataPath + "/saves/gamesave2.save", FileMode.Open);
                saves[2] = (Save) bf.Deserialize(file);
                file.Close();
            }
            data_exists = true;
        } else {
            data_exists = false;
            Debug.Log("Storage: No Save Data");
        }

    }

    public static void deleteAllSaves() {
         string path = Application.persistentDataPath + "/saves/";
         DirectoryInfo directory = new DirectoryInfo(path);
         directory.Delete(true);
         Directory.CreateDirectory(path);
     }

    public static Save getActiveSave() {
        return saves[active_save];
    }

}
