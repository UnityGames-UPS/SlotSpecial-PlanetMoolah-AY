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

    [SerializeField] private GameObject[] paylines;    

    [SerializeField] internal List<Slot_Item> currentReelItems;
    [SerializeField] internal List<Slot_Item> poolReelItems;
    [SerializeField] private float minClearDuration;
    [SerializeField] private int iconSize;
    [SerializeField] internal bool isRemoving = false;
    [SerializeField] private Slot_Controller slot_Controller;

    [SerializeField] private List<Slot_col> slot_matrix;

            List<int> yPos = new List<int>();
        List<int> xPos = new List<int>();
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

    internal void FillReel1(List<List<int>> result)
    {

        // List<int> result = new List<int>() { 2, 2, 2,1,2 };
        for (int j = slot_matrix[0].row.Count - 1; j >= 0; j--)
        {
            for (int i = 0; i < 5; i++)
            {
                slot_matrix[i].row[j].transform.localPosition = new Vector2(0, 5 * iconSize);
                int id = result[j][i];
                if (id == 12)
                {
                    int randomWild = UnityEngine.Random.Range(0, wildIconList.Length);
                    slot_matrix[i].row[j].image.sprite = wildIconList[randomWild];
                    slot_matrix[i].row[j].wildVariation = randomWild;
                }
                else
                    slot_matrix[i].row[j].image.sprite = iconList[id];

                slot_matrix[i].row[j].id = id;

                slot_matrix[i].row[j].transform.DOLocalMoveY((2 - j) * iconSize, minClearDuration * (2 - j + 1)).SetEase(Ease.Linear);
            }

        }
    }
   

    internal void ReArrangeMatrix(List<List<int>> iconsToFill)
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
                    slot_matrix[i].row[j].image.sprite = iconList[iconsToFill[i][j]];
                    slot_matrix[i].row[j].id = iconsToFill[i][j];
                }

                slot_matrix[i].row[j].transform.DOLocalMoveY((2 - j) * iconSize, minClearDuration).SetEase(Ease.InOutQuad);
            }
        }

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

    internal void StopSymbolAnimation(List<string> symbolsToEmit){

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
        //TODO: PM Check wild Animation
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

                slot_matrix[yPos[i]].row[xPos[i]].ownAnim.StartAnimation();
        }




    }
    internal bool CheckIfWild(int[] value)
    {

        if (slot_matrix[value[0]].row[value[1]].id == 12) return true;
        else return false;

    }

    internal void Remove(List<int> fillPos)
    {
        if (isRemoving)
            return;
        isRemoving = true;

        //foreach (var item in arrayPos)
        //{
        //    poolReelItems.Add(currentReelItems[item]);
        //    currentReelItems[item] = null;
        //}

        for (int i = 0; i < poolReelItems.Count; i++)
        {
            poolReelItems[i].transform.parent = transform;
            poolReelItems[i].transform.localPosition = new Vector2(0, 5 * iconSize);

        }
        for (int i = 0; i < slot_matrix[0].row.Count; i++)
        {
            slot_matrix[0].row[i].transform.localPosition = new Vector2(0, 5 * iconSize);
        }

        StartCoroutine(Move(fillPos));
    }

    IEnumerator Move(List<int> fillPos)
    {
        yield return new WaitForSeconds(minClearDuration);

        for (int i = 0; i < currentReelItems.Count; i++)
        {
            if (currentReelItems[i] != null) continue;

            for (int j = i + 1; j < currentReelItems.Count; j++)
            {
                if (currentReelItems[j] != null)
                {
                    currentReelItems[j].transform.DOLocalMoveY(i * iconSize, minClearDuration).SetEase(Ease.Linear);
                    //currentReelItems[j].pos = j;
                    currentReelItems[i] = currentReelItems[j];
                    currentReelItems[i].pos = i;
                    currentReelItems[j] = null;
                    break;
                }
            }
        }
        //currentReelItems.OrderBy(item => item?.pos).ToList();
        //currentReelItems.Sort((item1, item2) =>
        //{
        //    if (item1 == null && item2 == null) return 0;
        //    if (item1 == null) return 1; // Place nulls at the end
        //    if (item2 == null) return -1; // Place nulls at the end
        //    return item1.pos.CompareTo(item2.pos);
        //});

        for (int i = 0; i < 3; i++)
        {
            if (currentReelItems[i] == null && poolReelItems.Count > 0)
            {
                poolReelItems[poolReelItems.Count - 1].image.sprite = slot_Controller.iconList[fillPos[poolReelItems.Count - 1]];
                poolReelItems[poolReelItems.Count - 1].blastAnim.textureArray = slot_Controller.blastAnimationSprite;
                if (fillPos[poolReelItems.Count - 1] == 13)
                {
                    int index = UnityEngine.Random.Range(0, slot_Controller.wildIconList.Length);
                    poolReelItems[poolReelItems.Count - 1].image.sprite = slot_Controller.wildIconList[index];

                    if (index == 0)
                        poolReelItems[poolReelItems.Count - 1].blastAnim.textureArray = slot_Controller.wildAnimationSprite;
                    else if (index == 1)
                        poolReelItems[poolReelItems.Count - 1].blastAnim.textureArray = slot_Controller.wildAnimationSprite1;
                    else
                        poolReelItems[poolReelItems.Count - 1].blastAnim.textureArray = slot_Controller.wildAnimationSprite2;

                }

                poolReelItems[poolReelItems.Count - 1].gameObject.SetActive(true);
                poolReelItems[poolReelItems.Count - 1].transform.DOLocalMoveY(i * iconSize, minClearDuration).SetEase(Ease.Linear);
                currentReelItems[i] = poolReelItems[poolReelItems.Count - 1];
                currentReelItems[i].pos = i;
                currentReelItems[i].id = fillPos[poolReelItems.Count - 1];
                poolReelItems.RemoveAt(poolReelItems.Count - 1);
            }

            yield return new WaitForSeconds(minClearDuration);
        }

        isRemoving = false;
    }

    internal void GeneratePayline(List<int> linesToemit){

        for (int i = 0; i < linesToemit.Count; i++)
        {
            paylines[linesToemit[i]-1].SetActive(true);
        }

    }

    internal void ResetPayline(){
        for (int i = 0; i < paylines.Length; i++)
        {
            if(paylines[i].activeSelf)
            paylines[i].SetActive(false);
        }
    }
}


