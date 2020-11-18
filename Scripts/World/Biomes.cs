using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Utils;
using static TilemapManager;

namespace Biomes {

    public static class BiomeSpawn {
        public static int[,] biome_map;
        public static Dictionary<int, BiomeData> biome_ids = new Dictionary<int, BiomeData>();
        public static BiomeData grasslands;

        //==============
        // Initialize
        //==============
        public static void initialize() {
            biome_map = new int[CHUNK_WIDTH, CHUNK_HEIGHT];

            initBiomeTypes();
        }

        public static void initBiomeTypes() {
            grasslands = new BiomeData("grasslands");
            grasslands.plants = new List<string> {
                "pebble_tile", "tall_grass", "grass", "grass_tile", "flower",
                "flower_tile", "mushroom", "mushroom_tile"
            };
        }

        //==============
        // Load
        //==============
        public static void load() {
            fillMap();
        }

        public static void fillMap() {
            for (int y = 0; y < biome_map.GetLength(1); y++) {
                for (int x = 0; x < biome_map.GetLength(0); x++) {
                    biome_map[x, y] = grasslands.ID;
                }
            }
        }

        //==============
        // Reload
        //==============
        public static void reload() {
            // load new resources
            load();
        }
    }

    public class BiomeData {
        public string name;
        public int ID;
        public List<string> plants;

        public BiomeData(string n) {
            name = n;
            ID = Utils.getID(name);
            BiomeSpawn.biome_ids.Add(ID, this);
        }
    }
}
