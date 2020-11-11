using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class ObjPool {

    public static List<GameObject> objs = new List<GameObject>();
    public static Dictionary<string, List<GameObject>> pool = new Dictionary<string, List<GameObject>>();
    public static GameObject parent_obj = null;

    public static void Add(GameObject new_obj, int size) {
        // init parent_obj if it is not initialized already
        if (parent_obj == null) {
            parent_obj = new GameObject("ObjPoolParent");
        }

        // check for existance of new_obj in objs
        foreach (GameObject obj in objs) {
            if (obj.name == new_obj.name) {
                Debug.Log("GameObject exists in ObjPool! for name: " + new_obj.name);
                return;
            }
        }

        // Add to objs and to pool
        objs.Add(new_obj);
        GameObject new_obj_parent = new GameObject(new_obj.name + "_parent");
        new_obj_parent.transform.parent = parent_obj.transform;
        List<GameObject> obj_list = new List<GameObject>();
        for (int i = 0; i < size; ++i) {
            GameObject obj = (GameObject) Object.Instantiate(new_obj, new_obj_parent.transform);
            obj.name = new_obj.name;
            obj.SetActive(false);
            obj_list.Add(obj);
        }
        pool.Add(new_obj.name, obj_list);
    }

    public static GameObject Get(string name) {
        try {
            // get inactive object
            List<GameObject> obj_list = pool[name];
            foreach(GameObject obj in obj_list) {
                if (!obj.activeInHierarchy) {
                    return obj;
                }
            }
            // no inactive object found, expand obj_list
            GameObject obj_to_copy = obj_list[0];
            GameObject new_obj = (GameObject) Object.Instantiate(obj_to_copy, obj_to_copy.transform.parent);
            new_obj.name = obj_to_copy.name;
            new_obj.SetActive(false);
            obj_list.Add(new_obj);
            return new_obj;
        } catch {
            Debug.Log("ObjPool name: " + name + ", does not exist in pool!");
            return null;
        }
    }
}
