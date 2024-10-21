using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using System.Linq;
using Newtonsoft.Json;
using Unity.Mathematics;

public class Reel_Controller : MonoBehaviour
{
    [SerializeField] private Sprite[] iconList;
    [SerializeField] private Sprite[] wildIconList;
    [SerializeField] private Sprite[] balstAnimList;

    [SerializeField] private Sprite[] wildAnimationSprite0;
    [SerializeField] private Sprite[] wildAnimationSprite1;
    [SerializeField] private Sprite[] wildAnimationSprite2;

    [SerializeField] private float minClearDuration;
    [SerializeField] private int iconSize;

    [SerializeField] private List<Slot_col> slot_matrix;

    public Sprite empty;

    [Serializable]
    public class Slot_col
    {
        public List<Slot_Item> row;

    }
    internal void PopulateSlot()
    {
        Debug.Log("called");
        for (int i = 0; i < slot_matrix.Count; i++)
        {
            for (int j = 0; j < slot_matrix[i].row.Count; j++)
            {
                int RandomIndex = UnityEngine.Random.Range(0, iconList.Length);
                slot_matrix[i].row[j].image.sprite = iconList[RandomIndex];
                slot_matrix[i].row[j].id = RandomIndex;
                slot_matrix[i].row[j].name = $"{i}{j}";
                // slot_matrix[i].row[j].pos=;
            }
        }
    }

    internal void ClearReel()
    {
        for (int i = 0; i < slot_matrix.Count; i++)
        {
            for (int j = 0; j < slot_matrix[i].row.Count; j++)
            {
                slot_matrix[i].row[j].transform.DOLocalMoveY(-4 * iconSize, (minClearDuration + UnityEngine.Random.Range(0, 0.2f)) * (2 - j + 1)).SetEase(Ease.Linear);
            }
        }


    }

    internal IEnumerator FillReel1(List<List<int>> result)
    {

        for (int i = 0; i < 5; i++)
        {
            for (int j = slot_matrix[i].row.Count - 1; j >= 0; j--)
            {
                slot_matrix[i].row[j].transform.localPosition = new Vector2(0, 5 * iconSize);
                int id = result[j][i];
                slot_matrix[i].row[j].id = id;
                if (id == 12)
                {
                    int randomWild = UnityEngine.Random.Range(0, wildIconList.Length);
                    slot_matrix[i].row[j].image.sprite = wildIconList[randomWild];
                    slot_matrix[i].row[j].wildVariation = randomWild;
                }
                else
                {

                    slot_matrix[i].row[j].image.sprite = iconList[id];
                }


                slot_matrix[i].row[j].transform.DOLocalMoveY((2 - j) * iconSize, minClearDuration * (2 - j + 1)).SetEase(Ease.Linear);
            }

            yield return new WaitForSeconds(minClearDuration);

        }
        yield return new WaitForSeconds(0.25f);
    }


    internal IEnumerator ReArrangeMatrix(List<List<int>> iconsToFill)
    {
        for (int i = 0; i < slot_matrix.Count; i++)
        {
            var negativeOnes = slot_matrix[i].row.Where(x => x.id == -1).ToList();

            var otherValues = slot_matrix[i].row.Where(x => x.id != -1).ToList();

            if (negativeOnes.Count == 0)
                continue;

            foreach (var item in otherValues)
            {
                negativeOnes.Add(item);
            }

            slot_matrix[i].row.Clear();
            slot_matrix[i].row.AddRange(negativeOnes);


            for (int j = 0; j < slot_matrix[i].row.Count; j++)
            {
                if (slot_matrix[i].row[j].id == -1)
                {
                    slot_matrix[i].row[j].transform.localPosition = new Vector3(slot_matrix[i].row[j].transform.localPosition.x, 5 * iconSize, slot_matrix[i].row[j].transform.localPosition.z);
                    slot_matrix[i].row[j].id = iconsToFill[i][j];
                    if (iconsToFill[i][j] == 12)
                    {
                        int randomWild = UnityEngine.Random.Range(0, wildIconList.Length);
                        slot_matrix[i].row[j].image.sprite = wildIconList[randomWild];
                        slot_matrix[i].row[j].wildVariation = randomWild;
                    }
                    else
                    {

                        slot_matrix[i].row[j].image.sprite = iconList[iconsToFill[i][j]];
                    }
                }

                slot_matrix[i].row[j].transform.DOLocalMoveY((2 - j) * iconSize, minClearDuration).SetEase(Ease.InOutQuad);
            }
        }
        yield return new WaitForSeconds(0.1f);

    }



    internal void HanldleSymbols(List<string> symbolsToEmit)
    {
        List<int> yPos = new List<int>();
        List<int> xPos = new List<int>();

        for (int i = 0; i < symbolsToEmit.Count; i++)
        {
            int[] values = Helper.ConvertSymbolPos(symbolsToEmit[i]);

            yPos.Add(values[0]);
            xPos.Add(values[1]);

        }

        for (int i = 0; i < yPos.Count; i++)
        {

            slot_matrix[yPos[i]].row[xPos[i]].blastAnim.textureArray = balstAnimList.ToList();
            slot_matrix[yPos[i]].row[xPos[i]].blastAnim.StartAnimation();

        }
    }

    internal void StopSymbolAnimation(List<string> symbolsToEmit)
    {

        List<int> yPos = new List<int>();
        List<int> xPos = new List<int>();
        for (int i = 0; i < symbolsToEmit.Count; i++)
        {
            int[] values = Helper.ConvertSymbolPos(symbolsToEmit[i]);

            yPos.Add(values[0]);
            xPos.Add(values[1]);

        }

        for (int i = 0; i < yPos.Count; i++)
        {

            slot_matrix[yPos[i]].row[xPos[i]].blastAnim.StopAnimation();
            slot_matrix[yPos[i]].row[xPos[i]].ownAnim.StopAnimation();
            slot_matrix[yPos[i]].row[xPos[i]].blastAnim.textureArray.Clear();
            slot_matrix[yPos[i]].row[xPos[i]].ownAnim.textureArray.Clear();

        }
        // yield return new WaitForSeconds(0.2f);
        for (int i = 0; i < yPos.Count; i++)
        {
            slot_matrix[yPos[i]].row[xPos[i]].id = -1;
            slot_matrix[yPos[i]].row[xPos[i]].image.sprite = empty;
        }

    }
    internal void HandleWildSymbols(List<string> symbolsToEmit)
    {
        //[x]: PM Check wild Animation
        List<int> yPos = new List<int>();
        List<int> xPos = new List<int>();

        for (int i = 0; i < symbolsToEmit.Count; i++)
        {
            int[] values = Helper.ConvertSymbolPos(symbolsToEmit[i]);

            yPos.Add(values[0]);
            xPos.Add(values[1]);
        }

        for (int i = 0; i < yPos.Count; i++)
        {
            if (slot_matrix[yPos[i]].row[xPos[i]].wildVariation == 0)
                slot_matrix[yPos[i]].row[xPos[i]].ownAnim.textureArray = wildAnimationSprite0.ToList();
            else if (slot_matrix[yPos[i]].row[xPos[i]].wildVariation == 1)
                slot_matrix[yPos[i]].row[xPos[i]].ownAnim.textureArray = wildAnimationSprite1.ToList();
            else if (slot_matrix[yPos[i]].row[xPos[i]].wildVariation == 2)
                slot_matrix[yPos[i]].row[xPos[i]].ownAnim.textureArray = wildAnimationSprite2.ToList();

            if (slot_matrix[yPos[i]].row[xPos[i]].ownAnim.textureArray.Count > 0) slot_matrix[yPos[i]].row[xPos[i]].ownAnim.StartAnimation();

        }

    }
    internal bool CheckIfWild(int[] value)
    {

        if (slot_matrix[value[0]].row[value[1]].id == 12) return true;
        else return false;

    }

    internal void HighlightIcon(List<int> payline, List<string> symbols, Color highlightColor)
    {


        // [x]: PM adding highlight
        for (int i = 0; i < symbols.Count; i++)
        {
            // string cord = $"{i},{payline[i]}";
            // if (symbols.Contains(cord))
            // {
            slot_matrix[i].row[payline[i]].boder.gameObject.SetActive(true);
            slot_matrix[i].row[payline[i]].boder.color = highlightColor;
            // }

        }
    }

    internal void StopHighlightIcon(List<int> payline)
    {

        for (int i = 0; i < slot_matrix.Count; i++)
        {
            slot_matrix[i].row[payline[i]].boder.gameObject.SetActive(false);
            slot_matrix[i].row[payline[i]].boder.color = Color.white;

        }
    }


}


