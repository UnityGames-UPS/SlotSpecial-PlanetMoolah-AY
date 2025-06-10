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

    internal static List<List<int>> FindEmitingSymbol(int lineIndex, List<int> position, List<List<int>> initialLines)
    {
        List<List<int>> resultList = new List<List<int>>();

        for (int i = 0; i < position.Count; i++)
        {
            List<int> temp = new List<int>();
            temp.Add(i);                                     // Add index
            temp.Add(initialLines[lineIndex][position[i]]); // Add symbol value

            resultList.Add(temp); // Add the sublist to the result list
        }

        return resultList;
    }


}