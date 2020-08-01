using System.Collections;
using System.Collections.Generic;
// using static System.Math;
using UnityEngine;
using System.Linq;

public static class Utils
{
    private static int perlin_seed = 69;

    //==================================
    //  PROCEDURAL CONTENT GENERATION
    //==================================
    public static float noise(float x, float y, float frequency = 1, int octaves = 1, float multiplier = 15f, float amplitude = 1f, float lacunarity = 2, float persistence = 0.25f)
    {
        //convert v2 values into floating point
        Vector2 v2 = new Vector2((x / multiplier) + 0.1f,(y / multiplier) + 0.1f) * frequency;
        float value = 0;
        for (int n = 0; n < octaves; n++)
        {
            value += Mathf.PerlinNoise(v2.x + perlin_seed, v2.y + perlin_seed) * amplitude;
            v2 *= lacunarity;
            amplitude *= persistence;
        }
        return (float) System.Math.Round(value, 2);
    }

    public static float weightedRange(float[] w_range)
    {
        List<float> return_values = new List<float>();
        for (int i = 0; i < w_range.Length - 2; i += 3)
        {
            for (int j = 0; j < w_range[i + 2]; j++)
            {
                return_values.Add(Random.Range(w_range[i], w_range[i + 1]));
            }
        }
        return return_values[Random.Range(0, return_values.Count)];
    }

    //==================================
    //  Vector Utils
    //==================================

    public static bool AlmostEqual(Vector3 v1, Vector3 v2, float epsilon)
    {
        return Mathf.Abs(v1.x - v2.x) <= epsilon &&
               Mathf.Abs(v1.y - v2.y) <= epsilon &&
               Mathf.Abs(v1.z - v2.z) <= epsilon;
    }

    public static float L1(Vector2 v1, Vector2 v2)
    {
        return Mathf.Max(Mathf.Abs(v1.x - v2.x), Mathf.Abs(v1.y - v2.y));
    }

    public static float L1(Vector3 v1, Vector3 v2)
    {
        return Mathf.Max(Mathf.Abs(v1.x - v2.x), Mathf.Abs(v1.y - v2.y), Mathf.Abs(v1.z - v2.z));
    }

    //==================================
    //  Debug Printing
    //==================================

    public static void printList<T>(IList<T> values)
    {
        string value_string = "[";
        foreach (T value in values)
        {
            value_string += value + ", ";
        }
        value_string += "]";
        Debug.Log(value_string);
    }

    public static void print3dArray(int[,,] values)
    {
        string value_str = "\n[\n";
        float val;

        for (int z = 0; z < values.GetLength(2); z++)
        {
            value_str += "[";
            for (int y = 0; y < values.GetLength(1); y++)
            {
                value_str += "[";
                for (int x = 0; x < values.GetLength(0); x++)
                {
                    val = values[x, y, z];
                    value_str += val + ", ";
                }
                value_str += "],\n";
            }
            value_str += "],\n";
        }
        value_str += "]\n";
        Debug.Log(value_str);
    }

    public static void print2dArray(int[,] values)
    {
        try
        {
            float max_value = Mathf.NegativeInfinity;
            float min_value = Mathf.Infinity;
            float sum = 0;
            float mean = 0;
            float variance = 0;
            float size = values.GetLength(0) * values.GetLength(1);
            string value_str = "\n[\n";
            float val = 0;

            for (int i = 0; i < values.GetLength(0); i++)
            {
                value_str += "[";
                for (int j = 0; j < values.GetLength(1); j++)
                {
                    val = (float) values[i, j];
                    value_str += val + ", ";
                    sum += val;
                    if (val > max_value) max_value = val;
                    if (val < min_value) min_value = val;
                }
                value_str += "],\n";
            }
            value_str += "]";

            mean = sum / size;
            sum = 0;

            //variance
            for (int i = 0; i < values.GetLength(0); i++)
            {
                for (int j = 0; j < values.GetLength(1); j++)
                {
                    val = Mathf.Pow(values[i, j] - mean, 2);
                    sum += val;
                }
                value_str += "],\n";
            }

            variance = sum / (size - 1);

            //STATS STRING LOGGING
            string stats_str = "Max: " + max_value;
            stats_str += "\nMin: " + min_value;
            stats_str += "\nMean: " + mean;
            stats_str += "\nVariance: " + variance;
            stats_str += "\nSize: " + size;
            Debug.Log(stats_str + value_str);
        }
        catch
        {
            Debug.Log("\nERROR Utils,printing2dArray generic function:");
        }
    }

    public static void print2dArray(float[,] values)
    {
        try
        {
            float max_value = Mathf.NegativeInfinity;
            float min_value = Mathf.Infinity;
            float sum = 0;
            float mean = 0;
            float variance = 0;
            float size = values.GetLength(0) * values.GetLength(1);
            string value_str = "\n[\n";
            float val = 0;

            for (int i = 0; i < values.GetLength(0); i++)
            {
                value_str += "[";
                for (int j = 0; j < values.GetLength(1); j++)
                {
                    val = (float) values[i, j];
                    value_str += val + ", ";
                    sum += val;
                    if (val > max_value) max_value = val;
                    if (val < min_value) min_value = val;
                }
                value_str += "],\n";
            }
            value_str += "]";

            mean = sum / size;
            sum = 0;

            //variance
            for (int i = 0; i < values.GetLength(0); i++)
            {
                for (int j = 0; j < values.GetLength(1); j++)
                {
                    val = Mathf.Pow(values[i, j] - mean, 2);
                    sum += val;
                }
                value_str += "],\n";
            }

            variance = sum / (size - 1);

            //STATS STRING LOGGING
            string stats_str = "Max: " + System.Math.Round(max_value, 2);
            stats_str += "\nMin: " + min_value;
            stats_str += "\nMean: " + mean;
            stats_str += "\nVariance: " + variance;
            stats_str += "\nSize: " + size;
            Debug.Log(stats_str + value_str);
        }
        catch
        {
            Debug.Log("\nERROR Utils,printing2dArray generic function:");
        }
    }

    public static void printSpriteArray(Sprite[] values)
    {
        printList<Sprite>(values.ToList());
    }

}
