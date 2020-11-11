using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Utils;
using static TilemapManager;
using static Plants;

public static class Biomes {

    //==============
    // Data
    //==============
    public static int[,] biome_map;
    public static Dictionary<int, Biome> biome_ids = new Dictionary<int, Biome>();
    public static Biome grasslands;

    //==============
    // Initialize
    //==============
    public static void initialize() {
        biome_map = new int[CHUNK_WIDTH, CHUNK_HEIGHT];

        initBiomeTypes();
    }

    //==============
    // Load
    //==============
    public static void load() {
        fillMap();
    }

    //==============
    // Reload
    //==============
    public static void reload() {
        // load new resources
        load();
    }

    //================
    // Helpers
    //================
    public static void fillMap() {
        for (int y = 0; y < biome_map.GetLength(1); y++) {
            for (int x = 0; x < biome_map.GetLength(0); x++) {
                biome_map[x, y] = grasslands.ID;
            }
        }
    }

    public static void initBiomeTypes() {
        grasslands = new Biome("grasslands");
        grasslands.plants = new List<string> {
            "pebble_tile", "tall_grass", "grass", "grass_tile", "flower",
            "flower_tile", "mushroom", "mushroom_tile"
        };
    }
}
