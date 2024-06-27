using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class Reel_Controller : MonoBehaviour
{
    [SerializeField] private GameObject iconPrefab;
    [SerializeField] private GameObject[] wild_prefab;
    [SerializeField] private List<GameObject> currentItems;
    [SerializeField] internal List<Reel_Item> currentReelItems;
    [SerializeField] internal List<GameObject> poolItems;
    [SerializeField] internal List<Reel_Item> poolReelItems;
    [SerializeField] private float minClearDuration;
    [SerializeField] private int iconSize;
    [SerializeField] internal bool isRemoving = false;
    [SerializeField] private Slot_Controller slot_Controller;
    void Start()
    {
        
    }


    internal void ClearReel(float randomDelay)
    {
        for (int i = 0; i < 3; i++)
        {

            //currentItems[i].transform.DOLocalMoveY(-4 * iconSize, (minClearDuration + randomDelay) * (i + 1)).SetEase(Ease.Linear);
            currentReelItems[i].transform.DOLocalMoveY(-4 * iconSize, (minClearDuration + randomDelay) * (i + 1)).SetEase(Ease.Linear);
            //poolItems.Add(currentItems[i].gameObject);
            poolReelItems.Add(currentReelItems[i]);
            //currentItems[i] = null;

            //currentReelItems[i]=null;
        }
        currentReelItems.Clear();
        //currentItems.Clear();
    }

    internal void FillReel(List<int> result)
    {
        foreach (Reel_Item item in poolReelItems)
        {
            item.transform.localPosition = new Vector2(0, 5 * iconSize);
            
        }
        for (int i = 0; i < 3; i++)
        {

            //poolItems[i].transform.DOLocalMoveY(i * iconSize, minClearDuration * (i + 1)).SetEase(Ease.Linear);
            if (result[result.Count - 1 - i] == 13)
            {
                int index = UnityEngine.Random.Range(0, slot_Controller.wildIconList.Length);
                poolReelItems[i].image.sprite = slot_Controller.wildIconList[index];
                poolReelItems[i].imageAnimation.AnimationSpeed = 60;
                if (index == 0)
                    poolReelItems[i].imageAnimation.textureArray = slot_Controller.wildAnimationSprite;
                else if (index == 1)
                    poolReelItems[i].imageAnimation.textureArray = slot_Controller.wildAnimationSprite1;
                else
                    poolReelItems[i].imageAnimation.textureArray = slot_Controller.wildAnimationSprite2;

            }
            else
            {

                poolReelItems[i].image.sprite = slot_Controller.iconList[result[result.Count - 1 - i]];
                poolReelItems[i].imageAnimation.textureArray = slot_Controller.blastAnimationSprite;
            }

            poolReelItems[i].image.sprite = slot_Controller.iconList[result[result.Count -1 -i]];
            poolReelItems[i].id = result[result.Count - 1 - i];
            poolReelItems[i].pos = i;
            poolReelItems[i].transform.DOLocalMoveY(i * iconSize, minClearDuration * (i + 1)).SetEase(Ease.Linear);
            currentReelItems.Add(poolReelItems[i]);
            //poolItems[i] = null;
        }
        poolReelItems.Clear();
    }



    internal void populateReel(List<int> initialdata) {
        //Initiate();
        GameObject temp;

        for (int i = 0; i < 3; i++)
        {
            //if (int.Parse(initialdata[i]) == 13){
            //    temp= Instantiate(wild_prefab[UnityEngine.Random.Range(0, wild_prefab.Length)], transform);
            //}else
            temp = Instantiate(iconPrefab, transform);

            Reel_Item reelItem = temp.GetComponent<Reel_Item>();
            temp.transform.localPosition = new Vector2(0, i * iconSize);
            temp.name = i.ToString();
            reelItem.pos = i;
            reelItem.id = initialdata[i];
            reelItem.imageAnimation.textureArray.Clear();
            if (initialdata[i] == 13)
            {
                int index = UnityEngine.Random.Range(0, slot_Controller.wildIconList.Length);
                reelItem.image.sprite = slot_Controller.wildIconList[index];

                if (index == 0)
                    reelItem.imageAnimation.textureArray = slot_Controller.wildAnimationSprite;
                else if (index == 1)
                    reelItem.imageAnimation.textureArray = slot_Controller.wildAnimationSprite1;
                else
                    reelItem.imageAnimation.textureArray = slot_Controller.wildAnimationSprite2;

            }
            else {

            reelItem.image.sprite = slot_Controller.iconList[initialdata[i]];
            reelItem.imageAnimation.textureArray = slot_Controller.blastAnimationSprite;
            }

            currentReelItems.Add(reelItem);
        }
    }

    internal void Remove( List<int>fillPos)
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
                    currentReelItems[j].pos = i;
                    currentReelItems[i] = currentReelItems[j];
                    currentReelItems[j] = null;
                    break;
                }
            }

            if (currentReelItems[i] == null && poolReelItems.Count > 0)
            {
                poolReelItems[poolReelItems.Count - 1].image.sprite = slot_Controller.iconList[fillPos[poolReelItems.Count - 1]];
                poolReelItems[poolReelItems.Count - 1].imageAnimation.textureArray = slot_Controller.blastAnimationSprite;
                if (fillPos[poolReelItems.Count - 1] == 13)
                {
                    int index = UnityEngine.Random.Range(0, slot_Controller.wildIconList.Length);
                    poolReelItems[poolReelItems.Count - 1].image.sprite = slot_Controller.wildIconList[index];

                    if (index == 0)
                        poolReelItems[poolReelItems.Count - 1].imageAnimation.textureArray = slot_Controller.wildAnimationSprite;
                    else if (index == 1)
                        poolReelItems[poolReelItems.Count - 1].imageAnimation.textureArray = slot_Controller.wildAnimationSprite1;
                    else
                        poolReelItems[poolReelItems.Count - 1].imageAnimation.textureArray = slot_Controller.wildAnimationSprite2;

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
}


