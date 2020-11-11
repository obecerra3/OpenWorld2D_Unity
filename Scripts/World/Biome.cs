using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Biome {

    //==============
    // Data
    //==============
    public string name;
    public int ID;
    public List<string> plants;

    public Biome(string n) {
        name = n;
        ID = Utils.getID(name);
        Biomes.biome_ids.Add(ID, this);
    }
}
