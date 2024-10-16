using System.Collections;
using System.Collections.Generic;
using Best.SocketIO;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Slot_Manager : MonoBehaviour
{
    [SerializeField] private Reel_Controller reel_Controller;
    [SerializeField] private UFO_controller uFO_Controller;
    [SerializeField] private SocketController socketManager;
    [SerializeField] private AudioController audioController;
    [SerializeField] private Payline_controller payline_Controller;
    [SerializeField] private UI_Controller uI_Controller;

    [Header("Buttons")]
    [SerializeField] private Button start_Button;
    [SerializeField] private Button autoStart_Button;
    [SerializeField] private Button autoStop_Button;

    [SerializeField] private Button betPlus_Button;
    [SerializeField] private Button betMinus_Button;

    [SerializeField] private bool isAutoSpin;
    [SerializeField] private bool isSpinning;

    [SerializeField] private double currentTotalBet;
    [SerializeField] private double currentBalance;
    [SerializeField] private int betCounter=0;
    void Awake()
    {
        uI_Controller.Exitgame=socketManager.CloseSocket;
        socketManager.ShowDisconnectionPopUp=uI_Controller.ShowDisconnectPopup;
        socketManager.InitiateUI=InitiateUI;
        socketManager.OpenSocket();
        

    }
    void Start()
    {
        // uI_Controller.UpdatePlayerInfo(socketManager.socketModel.playerData);



        reel_Controller.PopulateSlot();




        start_Button.onClick.AddListener(() => StartCoroutine(SpinRoutine()));
        autoStart_Button.onClick.AddListener(() => StartCoroutine(AutoSpinRoutine()));
        autoStop_Button.onClick.AddListener(() =>
        {
            isAutoSpin = false;
            autoStop_Button.gameObject.SetActive(false);
            autoStart_Button.gameObject.SetActive(true);
        });

        betPlus_Button.onClick.AddListener(delegate{ChangeBet(true);});
        betMinus_Button.onClick.AddListener(delegate{ChangeBet(false);});
    }

    IEnumerator AutoSpinRoutine()
    {

        if (isSpinning || isAutoSpin)
            yield break;

        isAutoSpin = true;
        autoStop_Button.gameObject.SetActive(true);
        autoStart_Button.gameObject.SetActive(false);
        while (isAutoSpin)
        {

            yield return SpinRoutine();
            yield return new WaitForSeconds(1f);
        }

    }



    IEnumerator SpinRoutine()
    {
        bool start=OnSpinStart();
        if(!start)
        yield break;
        isSpinning = true;
        yield return new WaitForSeconds(1.6f);

        yield return new WaitUntil(() => socketManager.isResultdone);

        yield return OnSpin(socketManager.socketModel.resultGameData.ResultReel);

        yield return OnSpinEnd();
        isSpinning = false;



    }

    bool OnSpinStart()
    {
        reel_Controller.ClearReel();

        ToggleButtonGrp(false);
        if(currentBalance<currentTotalBet){
            uI_Controller.ShowLowBalPopup();
            return false;
        }
        uI_Controller.UpdatePlayerInfo(0,socketManager.socketModel.playerData.Balance);
        var spinData = new { data = new { currentBet = betCounter, currentLines = socketManager.socketModel.initGameData.lineData.Count, spins = 1 }, id = "SPIN" };
        socketManager.SendData("message", spinData);
        return true;
    }

    IEnumerator OnSpin(List<List<int>> resultData)
    {
        List<ImageAnimation> pullingAnimList = new List<ImageAnimation>();
        List<string> SymbolsToEmit;
        List<string>[] symbols = new List<string>[2];
        int lineId = -1;
        var cascadeData = socketManager.socketModel.resultGameData.cascadeData;
        Color borderColor;
        reel_Controller.FillReel1(resultData);
        yield return new WaitForSeconds(1.6f);

        if (cascadeData.Count > 0)
        {
            audioController.PlayWLAudio();

            for (int k = 0; k < cascadeData.Count; k++)
            {

                SymbolsToEmit = Helper.Flatten2DList(cascadeData[k].winingSymbols);
                symbols = SeparateSymbols(SymbolsToEmit);

                uI_Controller.UpdatePlayerInfo(cascadeData[k].currentWining);
                for (int i = 0; i < cascadeData[k].lineToEmit.Count; i++)
                {
                    lineId = cascadeData[k].lineToEmit[i] - 1;
                    borderColor = payline_Controller.GeneratePayline(lineId);
                    reel_Controller.HighlightIcon(socketManager.socketModel.initGameData.lineData[lineId], cascadeData[k].winingSymbols[i], borderColor);
                    yield return new WaitForSeconds(0.5f);
                    reel_Controller.StopHighlightIcon(socketManager.socketModel.initGameData.lineData[lineId]);
                    payline_Controller.DestroyPayline(lineId);
                    yield return new WaitForSeconds(0.5f);
                }
                Debug.Log("current winning"+cascadeData[k].currentWining);

                HandleShootAndPullAnim(pullingAnimList, symbols);
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
    }
    IEnumerator OnSpinEnd()
    {
        var playerData = socketManager.socketModel.playerData;
        uI_Controller.UpdatePlayerInfo(playerData.CurrentWining,playerData.Balance);

        double winAmount = 0;
        int winType = -1;

        if (socketManager.socketModel.resultGameData.jackpot > 0)
        {
            winAmount = socketManager.socketModel.resultGameData.jackpot;
            winType = 3;

        }
        else if (playerData.CurrentWining * 10 > currentTotalBet && currentTotalBet < playerData.CurrentWining * 15)
        {
            winAmount = playerData.CurrentWining;
            winType = 0;
        }
        else if (playerData.CurrentWining * 15 > currentTotalBet && currentTotalBet < playerData.CurrentWining * 20)
        {
            winAmount = playerData.CurrentWining;
            winType = 1;
        }
        else if (playerData.CurrentWining * 20 > currentTotalBet)
        {
            winAmount = playerData.CurrentWining;
            winType = 2;
        }

        yield return uI_Controller.ShowWinPopup(winType, winAmount);

        ToggleButtonGrp(true);
        uI_Controller.UpdatePlayerInfo(socketManager.socketModel.playerData.CurrentWining,socketManager.socketModel.playerData.Balance);

    }




    private void HandleShootAndPullAnim(List<ImageAnimation> pullingAnimList, List<string>[] symbols)
    {
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

    void ChangeBet(bool inc){

        if(inc){
            if(betCounter<socketManager.socketModel.initGameData.Bets.Count-1)
            betCounter++;

        }else{

            if(betCounter>0)
            betCounter--;


        }
        currentTotalBet=socketManager.socketModel.initGameData.Bets[betCounter]*socketManager.socketModel.initGameData.lineData.Count;
        uI_Controller.UpdateBetInfo(socketManager.socketModel.initGameData.Bets[betCounter],currentTotalBet);

        if(currentBalance<currentTotalBet)
        uI_Controller.ShowLowBalPopup();


    }
    void InitiateUI(List<Symbol> uiData, PlayerData playerData)
    {

        uI_Controller.UpdatePlayerInfo(0, playerData.Balance);
        currentBalance=playerData.Balance;
        Debug.Log("balance "+playerData.Balance);
        uI_Controller.InitUI(uiData);
        currentTotalBet = socketManager.socketModel.initGameData.Bets[betCounter] * socketManager.socketModel.initGameData.lineData.Count;
        uI_Controller.UpdateBetInfo(socketManager.socketModel.initGameData.Bets[betCounter],currentTotalBet);

        if(currentBalance<currentTotalBet)
        uI_Controller.ShowLowBalPopup();
    }
    void ToggleButtonGrp(bool toggle)
    {

        start_Button.interactable = toggle;
        autoStart_Button.interactable = toggle;
        betMinus_Button.interactable=toggle;
        betPlus_Button.interactable=toggle;

    }

}
