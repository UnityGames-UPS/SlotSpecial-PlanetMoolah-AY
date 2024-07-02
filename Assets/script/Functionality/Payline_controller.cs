using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Payline_controller : MonoBehaviour
{
    [SerializeField] internal GameObject[] paylines;
    [SerializeField] private Slot_Controller slot_Controller;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    internal IEnumerator TogglePayline(List<int> lineID,List<string> symbolstoEmmit) {


        //setIconActive(symbolstoEmmit);
        //yield return new WaitForSeconds(1f);
        //setIconInActive(symbolstoEmmit);
        //yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < lineID.Count; i++)
        {
            //setIconActive(symbolstoEmmit[i == 0 ? 1 : i]);
            //paylines[lineID[i]].SetActive(true);
            yield return StartCoroutine(blinkroutine(paylines[lineID[i]],3));
            //yield return new WaitForSeconds(1f);
            //paylines[lineID[i]].SetActive(false);
            //setIconInActive(symbolstoEmmit[i == 0 ? 1 : i]);
        }

        yield return new WaitForSeconds(0.5f);
    }

    void setIconActive(List<string> symbols) {

        for (int i = 0; i < symbols.Count; i++)
        {
            print("symbols"+symbols[i]);
            int pos = int.Parse(symbols[i].Replace(",", ""));
            int posY = 2 - (pos >= 10 ? (pos % 10) : pos);
            int posX = pos >= 10 ? (pos / 10) % 10 : 0;
            //for (int i = 0; i < slot_Controller.reels[reelIndex].currentReelItems.Count; i++)
            //{
            //if (slot_Controller.reels[reelIndex].currentReelItems[i].pos == posY)
            slot_Controller.reels[posX].currentReelItems[posY].boder.gameObject.SetActive(true);
            slot_Controller.reels[posX].currentReelItems[posY].boder.color= UnityEngine.Random.ColorHSV();
            //}

        }


    }

    void setIconInActive(List<string> symbols) {
        for (int i = 0; i < symbols.Count; i++)
        {
            print("symbols" + symbols[i]);
            int pos = int.Parse(symbols[i].Replace(",", ""));
            int posY = 2 - (pos >= 10 ? (pos % 10) : pos);
            int posX = pos >= 10 ? (pos / 10) % 10 : 0;
            //for (int i = 0; i < slot_Controller.reels[reelIndex].currentReelItems.Count; i++)
            //{
            //if (slot_Controller.reels[reelIndex].currentReelItems[i].pos == posY)
            slot_Controller.reels[posX].currentReelItems[posY].transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
            //}

        }

    }

    IEnumerator blinkroutine(GameObject objectToBlink, int repeatation)
    {

        print("triggered");
        for (int i = 0; i < repeatation; i++)
        {
            yield return new WaitForSeconds(0.1f);
            objectToBlink.SetActive(true);
            yield return new WaitForSeconds(0.1f);
            objectToBlink.SetActive(false);

        }

    }
}
