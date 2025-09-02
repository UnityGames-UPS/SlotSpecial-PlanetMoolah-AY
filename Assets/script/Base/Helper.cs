using System.Collections.Generic;
using UnityEngine;
using System.Linq;
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

    internal static List<int> ConvertSymbolToPos(List<string> positons)
    {
        List<int> resultlist = new List<int>();
        foreach (var pos in positons)
        {
            string[] values = pos.Split(',');
            int[] modifiedPos = new int[2];
            modifiedPos[0] = int.Parse(values[0]);
            modifiedPos[1] = int.Parse(values[1]);
            resultlist.Add(modifiedPos[0]);
        }
        return resultlist;

    }

    internal static List<string> Flatten2DList(List<List<string>> twoDList)
    {
        List<string> flatList = new List<string>();

        foreach (var sublist in twoDList)
        {
            foreach (var item in sublist)
            {
                if (!flatList.Contains(item))
                    flatList.Add(item);
            }
        }

        return flatList;
    }

    // internal static List<string> Flatten2DList(List<List<positionToFill>> twoDList)
    // {
    //     List<string> flatList = new List<string>();

    //     foreach (var sublist in twoDList)
    //     {
    //         foreach (var item in sublist)
    //         {
    //             if (!flatList.Contains(item))
    //                 flatList.Add(item);
    //         }
    //     }

    //     return flatList;
    // }
    internal static int findSymbolID(int x, int y, List<List<int>> symbolToFill)
    {
        for (int i = 0; i < symbolToFill.Count; i++)
        {

            if (symbolToFill[i][0] == x && symbolToFill[i][1] == y)
            {
                return symbolToFill[i][2];
            }


        }
        return -1;
    }

    internal static List<List<int>> FindEmitingSymbol(int lineIndex, List<int> position, List<List<int>> initialLines)
    {
        List<List<int>> resultList = new List<List<int>>();
        Debug.Log("ashu Test new:" + lineIndex);

        for (int i = 0; i < position.Count; i++)
        {
            List<int> temp = new List<int>();
            temp.Add(i);                                     // Add index
            temp.Add(initialLines[lineIndex][position[i]]); // Add symbol value

            resultList.Add(temp); // Add the sublist to the result list
        }

        return resultList;
    }
    internal static List<List<int>> ConvertToCoordinates(List<string> input)
    {
        return input
        .Select(s => s.Split(',')
                      .Select(int.Parse)
                      .ToList())
        .Select(pair => new List<int> { pair[1], pair[0] }) // reverse x,y → y,x
        .ToList();
    }


}