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
    [SerializeField] private Payline_controller payline_Controller;

    [Header("Buttons")]
    [SerializeField] private Button start_Button;
    [SerializeField] private Button autoStart_Button;
    [SerializeField] private Button autoStop_Button;

    [SerializeField] private bool isAutoSpin;
    [SerializeField] private bool isSpinning;
    public List<List<string>> resultData;
    public List<List<int>> iconsToFill;

    void Start()
    {
        reel_Controller.PopulateSlot();

        start_Button.onClick.AddListener(() => StartCoroutine(SpinRoutine()));
        autoStart_Button.onClick.AddListener(()=>StartCoroutine(AutoSpinRoutine()));
        autoStop_Button.onClick.AddListener(()=>{isAutoSpin=false;
        
                autoStop_Button.gameObject.SetActive(false);
        autoStart_Button.gameObject.SetActive(true);
        });
    }

    IEnumerator AutoSpinRoutine(){

        if(isSpinning || isAutoSpin)
        yield break;
        isAutoSpin=true;
        autoStop_Button.gameObject.SetActive(true);
        autoStart_Button.gameObject.SetActive(false);
        while(isAutoSpin){

            yield return SpinRoutine();
            yield return new WaitForSeconds(1f);
        }

    }


    void OnSpinStart()
    {


        reel_Controller.ClearReel();

        ToggleButtonGrp(false);
        var spinData = new { data = new { currentBet = 0, currentLines = 25, spins = 1 }, id = "SPIN" };
        socketManager.SendData("message", spinData);
    }

    void OnSpin(List<List<int>> resultData)
    {
        reel_Controller.FillReel1(resultData);
    }
    void OnSpinEnd()
    {
        ToggleButtonGrp(true);

        // uFO_Controller.Shoot(SymbolsToEmit);
        // reel_Controller.DeleteWinSymbols(SymbolsToEmit);
        // reel_Controller.ReArrangeMatrix();
    }


    IEnumerator SpinRoutine()
    {
        OnSpinStart();
        isSpinning=true;
        yield return new WaitForSeconds(1.6f);
        yield return new WaitUntil(() => socketManager.isResultdone);
        OnSpin(socketManager.socketModel.resultGameData.ResultReel);
        List<ImageAnimation> pullingAnimList = new List<ImageAnimation>();
        List<string> SymbolsToEmit;
        List<string>[] symbols= new List<string>[2];
        int lineId=-1; 
        var cascadeData = socketManager.socketModel.resultGameData.cascadeData;
        yield return new WaitForSeconds(1.6f);

        if (cascadeData.Count > 0)
        {
            audioController.PlayWLAudio();

            for (int k = 0; k < cascadeData.Count; k++)
            {

                SymbolsToEmit = Helper.Flatten2DList(cascadeData[k].winingSymbols);
                 symbols= SeparateSymbols(SymbolsToEmit);

                for (int i = 0; i < cascadeData[k].lineToEmit.Count; i++)
                {
                    lineId=cascadeData[k].lineToEmit[i]-1;
                    Color borderColor=payline_Controller.GeneratePayline(lineId);
                    reel_Controller.HighlightIcon(socketManager.socketModel.initGameData.lineData[lineId],SymbolsToEmit,borderColor);
                    yield return new WaitForSeconds(0.5f);
                    reel_Controller.StopHighlightIcon(socketManager.socketModel.initGameData.lineData[lineId]);
                    payline_Controller.DestroyPayline(lineId);
                    yield return new WaitForSeconds(0.5f);
                }

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
                reel_Controller.ReArrangeMatrix(cascadeData[k].symbolsToFill);

                yield return new WaitForSeconds(1f);
            }

        }
        OnSpinEnd();
        isSpinning=false;



    }

    List<string>[] SeparateSymbols(List<string> SymbolsToEmit)
    {
        List<string>[] symboltypes = new List<string>[2];

        List<string> wildSymbol = new List<string>();
        List<string> symbol = new List<string>();
        for (int i = 0; i < SymbolsToEmit.Count; i++)
        {
            int[] pos = Helper.ConvertSymbolPos(SymbolsToEmit[i]);
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

    void ToggleButtonGrp(bool toggle)
    {

        start_Button.interactable = toggle;
        autoStart_Button.interactable = toggle;

    }

}
