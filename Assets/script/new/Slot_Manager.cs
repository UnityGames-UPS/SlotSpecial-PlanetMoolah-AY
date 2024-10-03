using System.Collections;
using System.Collections.Generic;
using Best.SocketIO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class Slot_Manager : MonoBehaviour
{
    [SerializeField] private Reel_Controller reel_Controller;
    [SerializeField] private UFO_controller uFO_Controller;
    [SerializeField] private SocketController socketManager;
    [SerializeField] private AudioController audioController;

    [SerializeField] private Button start_Button;

    public List<List<string>> resultData;
    public List<List<int>> iconsToFill;

    void Start()
    {
        reel_Controller.PopulateSlot();
        resultData = new List<List<string>> {
            new List<string> { "2", "4", "13","7","3" },
            new List<string> { "5", "1", "4","6","4" },
            new List<string> { "1", "1", "13","0","4" },
        };

        iconsToFill = new List<List<int>>{

            new List<int> {4 },
            new List<int> {2,5},
            new List<int> {3,1 },
            new List<int> {},
            new List<int> {},
            };

        // SymbolsToEmit = new List<string> { "0,2", "1,2", "2,2", "1,1", "2,0" };
        start_Button.onClick.AddListener(() => StartCoroutine(SpinRoutine()));
    }

    void OnSpinStart()
    {


        reel_Controller.ClearReel();
        var spinData = new { data = new { currentBet = 0, currentLines = 25, spins = 1 }, id = "SPIN" };
        socketManager.SendData("message", spinData);

    }

    void OnSpin(List<List<int>> resultData)
    {
        reel_Controller.FillReel1(resultData);
    }
    void OnSpinEnd()
    {
        // uFO_Controller.Shoot(SymbolsToEmit);
        // reel_Controller.DeleteWinSymbols(SymbolsToEmit);
        // reel_Controller.ReArrangeMatrix();
    }


    IEnumerator SpinRoutine()
    {
        OnSpinStart();
        yield return new WaitForSeconds(1.6f);
        yield return new WaitUntil(() => socketManager.isResultdone);
        OnSpin(socketManager.socketModel.resultGameData.ResultReel);
        yield return new WaitForSeconds(1.6f);
        if (socketManager.socketModel.resultGameData.cascadeData.Count > 0)
        {

            for (int k = 0; k < socketManager.socketModel.resultGameData.cascadeData.Count; k++)
            {

                List<ImageAnimation> pullingAnimList = new List<ImageAnimation>();
                List<string> SymbolsToEmit = Helper.Flatten2DList(socketManager.socketModel.resultGameData.cascadeData[k].winingSymbols);

                List<string>[] symbols = SeparateSymbols(SymbolsToEmit);
                audioController.PlayWLAudio();
                reel_Controller.GeneratePayline(socketManager.socketModel.resultGameData.cascadeData[k].lineToEmit);
                yield return new WaitForSeconds(0.5f);
                reel_Controller.ResetPayline();

                if (symbols[1].Count > 0)
                {
                    audioController.PlayShootAudio();
                    for (int i = 0; i < symbols[1].Count; i++)
                    {
                        uFO_Controller.Shoot(Helper.ConvertSymbolPos(symbols[1][i]));
                    }
                }

                if (symbols[0].Count > 0)
                {

                    audioController.PlayPullAudio();
                    for (int i = 0; i < symbols[0].Count; i++)
                    {
                        pullingAnimList.Add(uFO_Controller.Pull(Helper.ConvertSymbolPos(symbols[0][i])));

                    }
                }
                yield return new WaitForSeconds(uFO_Controller.shootSpeed + 0.1f);
                reel_Controller.HanldleSymbols(symbols[1]);

                // yield return new WaitForSeconds(0.2f);
                reel_Controller.HandleWildSymbols(symbols[0]);
                yield return new WaitForSeconds(1f);

                reel_Controller.StopSymbolAnimation(SymbolsToEmit);
                for (int i = 0; i < pullingAnimList.Count; i++)
                {
                    pullingAnimList[i].StopAnimation();
                    Destroy(pullingAnimList[i].gameObject);
                }
                pullingAnimList.Clear();
                yield return new WaitForSeconds(0.2f);
                reel_Controller.ReArrangeMatrix(socketManager.socketModel.resultGameData.cascadeData[k].symbolsToFill);

                yield return new WaitForSeconds(1f);
            }

        }
        OnSpinEnd();
        // }


    }

    List<string>[] SeparateSymbols(List<string> SymbolsToEmit)
    {
        List<string>[] symboltypes = new List<string>[2];

        List<string> wildSymbol = new List<string>();
        List<string> symbol = new List<string>();
        for (int i = 0; i < SymbolsToEmit.Count; i++)
        {
            int[] pos = Helper.ConvertSymbolPos(SymbolsToEmit[i]);
            Debug.Log("pos" + JsonConvert.SerializeObject(pos));
            if (reel_Controller.CheckIfWild(pos))
            {
                wildSymbol.Add(SymbolsToEmit[i]);
            }
            else
            {
                symbol.Add(SymbolsToEmit[i]);
            }

        }

        symboltypes[0] = wildSymbol;
        symboltypes[1] = symbol;

        return symboltypes;

    }

}
