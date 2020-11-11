using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

//Specifically creates features that allow a understanding of the ground/ elevation/ levels.

public class TerrainTile : Tile {
    //==================
    // Initialization
    //==================
    public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go) {
        return true;
    }

    //==================
    // Render
    //==================
    //Returns the correct sprite according to orthogonally and diagonally adjacent Custom tiles
    //also should decide what to do according to height.
    public override void GetTileData(Vector3Int location, ITilemap tilemap, ref TileData tileData) {
        int mask = HasTerrainTile(tilemap, location + new Vector3Int(0, 1, 0)) ? 1 : 0; //top
        mask += HasTerrainTile(tilemap, location + new Vector3Int(1, 0, 0)) ? 2 : 0; //right
        mask += HasTerrainTile(tilemap, location + new Vector3Int(0, -1, 0)) ? 4 : 0; //bottom
        mask += HasTerrainTile(tilemap, location + new Vector3Int(-1, 0, 0)) ? 8 : 0; //left

        int mask2 = HasTerrainTile(tilemap, location + new Vector3Int(1, 1, 0)) ? 1: 0; //top_right
        mask2 += HasTerrainTile(tilemap, location + new Vector3Int(-1, 1, 0)) ? 2: 0; //top_left
        mask2 += HasTerrainTile(tilemap, location + new Vector3Int(1, -1, 0)) ? 4: 0; //bottom_right
        mask2 += HasTerrainTile(tilemap, location + new Vector3Int(-1, -1, 0)) ? 8: 0; //bottom_left

        int index = GetIndex((byte) mask, (byte) mask2);

        if (index >= 0 && index < TilemapManager.all_sprites.Length) {
            tileData.sprite = TilemapManager.all_sprites[index];
            tileData.color = Color.white;
            tileData.flags = TileFlags.LockTransform;
            tileData.colliderType = ColliderType.None;
        } else {
            Debug.Log("Error index not valid for TerrainTile and index: " + index);
        }
    }

    // The following determines which sprite to use based on the number of adjacent TerrainTiles
    private int GetIndex(byte mask, byte mask2) {
        switch (mask) {
            case 3: //top and right
                return 51; //left bottom corner edge
            case 6: //right and bottom
                return 11; //top left corner
            case 7: //top, right, bottom
                return (int) Utils.weightedRange(new float[] { 31, 31, 1, 96, 96, 1 }); //left side edge
            case 9: //left and top
                return 56;
            case 11: //top, right, left
                return Random.Range(32, 36); //white
                // return Random.Range(52, 56); //rock
            case 12: //left, bottom
                return 16;
            case 13: //top, left, bottom
                return (int) Utils.weightedRange(new float[] { 36, 36, 1, 76, 76, 1 }); //left side edge
            case 14: //right, left, bottom
                return Random.Range(12, 16);
            case 15: //top, right, left, bottom
                switch (mask2) {
                    case 11: //bottom_right empty
                        return 57; //white
                        // return 37; //rock
                    case 7: //bottom_left empty
                        return 58; //white
                        //return 38; //rock
                    case 13: //top left empty
                        return 94;
                    case 14: //top right empty
                        return 95;
                }
                return 0;
        }
        return 2;
    }

    // The following determines which rotation to use based on the positions of adjacent TerrainTiles
    private Quaternion GetRotation(byte mask) {
        // switch (mask)
        // {
        // }
        return Quaternion.Euler(0f, 0f, 0f);
    }


    //==============
    // Refresh
    //==============
    public override void RefreshTile(Vector3Int location, ITilemap tilemap) {
        // refreshes itself and other Custom tiles that are orthogonally and diagonally adjacent
        for (int yd = -1; yd <= 1; yd++)
            for (int xd = -1; xd <= 1; xd++) {
                Vector3Int position = new Vector3Int(location.x + xd, location.y + yd, location.z);
                if (HasTerrainTile(tilemap, position))
                    tilemap.RefreshTile(position);
            }
    }

    //==============================
    //Check Tilemap for TerrainTile
    //==============================
    private bool HasTerrainTile(ITilemap tilemap, Vector3Int position) {
        return tilemap.GetTile(position) == this;
    }

    //==============
    //Save as Asset
    //==============
    #if UNITY_EDITOR
        [MenuItem("Assets/Create/TerrainTile")]
        public static void CreateRoadTile() {
            string path = EditorUtility.SaveFilePanelInProject("Save Terrain Tile", "New Terrain Tile", "Asset", "Save Terrain Tile", "Assets");
            if (path == "")
                return;
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<TerrainTile>(), path);
        }
    #endif
}
