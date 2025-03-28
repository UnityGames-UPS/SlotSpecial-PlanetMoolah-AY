using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System;
using Newtonsoft.Json;
public class Helper
{

    internal static int GetRandomIndexExcept(int length, int exception)
    {
        List<int> randomIndex = new List<int>() { 0, 1, 2, 3, 4 };
        randomIndex.Remove(exception);
        int index = randomIndex[UnityEngine.Random.Range(0, randomIndex.Count)];
        return index;

    }

    internal static int[] ConvertSymbolPos(string pos)
    {
        string[] values = pos.Split(',');
        int[] modifiedPos = new int[2];
        modifiedPos[0] = int.Parse(values[0]);
        modifiedPos[1] = int.Parse(values[1]);

        return modifiedPos;

    }

    internal static List<string> Flatten2DList(List<List<string>> twoDList)
    {
        List<string> flatList = new List<string>();

        foreach (var sublist in twoDList)
        {
            foreach (var item in sublist)
            {
                if(!flatList.Contains(item))
                flatList.Add(item);
            }
        }

        return flatList;
    }



}