using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
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
    [SerializeField] private Button freeSpin_Button;

    [SerializeField] private Button betPlus_Button;
    [SerializeField] private Button betMinus_Button;
    [SerializeField] private Button StopSpin_Button;
    [SerializeField] private Button Turbo_Button;

    [SerializeField] private bool isAutoSpin;
    [SerializeField] private bool isFreeSpin;
    [SerializeField] private bool isSpinning;

    [SerializeField] private double currentTotalBet;
    [SerializeField] private int betCounter = 0;

    [SerializeField] private GameObject turboAnim;
    [SerializeField] private int totalLines;
    [SerializeField] private double currentBalance;
    [SerializeField] private int freeSpinCount;

    [SerializeField] private TMP_Text spinInfoText;
    public bool isNewAdded;

    [SerializeField] private Coroutine freeSpinRoutine;
    [SerializeField] private Coroutine autoSpinCoroutine;


    private Tween winHighlight = null;

    private bool inititated = false;

    static internal bool immediateStop;
    static internal bool checkPopUpCompletion;

    private bool turboMode;

    private bool wasAutoSpinOn;

    void Awake()
    {
        uI_Controller.Exitgame = socketManager.CloseSocket;
        uI_Controller.OnToggleAudio = audioController.ToggleMute;
        uI_Controller.OnPlayButton = audioController.PlayButtonAudio;
        socketManager.ShowDisconnectionPopUp = uI_Controller.ShowDisconnectPopup;
        socketManager.InitiateUI = InitiateUI;
        socketManager.OpenSocket();
        uFO_Controller.StartUfoVerticalMove();

    }
    void Start()
    {
        reel_Controller.PopulateSlot();

        start_Button.onClick.AddListener(() => StartCoroutine(SpinRoutine()));
        autoStart_Button.onClick.AddListener(() =>
        {
            if (!isAutoSpin && !isFreeSpin && !isSpinning)
                autoSpinCoroutine = StartCoroutine(AutoSpinRoutine());

        });
        autoStop_Button.onClick.AddListener(() =>
        {
            if (isAutoSpin)
            {

                isAutoSpin = false;
                autoStop_Button.interactable = false;
                StartCoroutine(AutoSpinStopRoutine());
            }
        });

        betPlus_Button.onClick.AddListener(delegate { ChangeBet(true); });
        betMinus_Button.onClick.AddListener(delegate { ChangeBet(false); });

        StopSpin_Button.onClick.AddListener(() => {audioController.PlayButtonAudio(); StartCoroutine(StopSpin());});

        Turbo_Button.onClick.AddListener(()=>{audioController.PlayButtonAudio(); ToggleTurboMode();});

    }

    void ToggleTurboMode()
    {
        turboMode = !turboMode;
        if (turboMode)
        {
            reel_Controller.minClearDuration = 0.1f;
            turboAnim.SetActive(true);
        }
        else
        {
            reel_Controller.minClearDuration = 0.2f;
            turboAnim.SetActive(false);
        }

    }
    IEnumerator StopSpin()
    {
        if (isAutoSpin || isFreeSpin || immediateStop)
            yield break;
        immediateStop = true;
        StopSpin_Button.interactable = false;
        yield return new WaitUntil(() => !isSpinning);
        immediateStop = false;
        StopSpin_Button.interactable = true;
    }

    IEnumerator AutoSpinRoutine()
    {

        isAutoSpin = true;
        autoStop_Button.gameObject.SetActive(true);
        autoStart_Button.gameObject.SetActive(false);
        autoStart_Button.interactable = false;
        wasAutoSpinOn = false;
        while (isAutoSpin)
        {

            yield return SpinRoutine();
            yield return new WaitForSeconds(1f);
        }
        isAutoSpin = false;
        isSpinning = false;

    }

    IEnumerator AutoSpinStopRoutine()
    {

        yield return new WaitUntil(() => !isSpinning);
        // ToggleButtonGrp(true);
        if (autoSpinCoroutine != null)
            StopCoroutine(autoSpinCoroutine);

        ToggleButtonGrp(true);
        autoStop_Button.gameObject.SetActive(false);
        autoStop_Button.interactable = true;
        autoStart_Button.gameObject.SetActive(true);
        autoStart_Button.interactable = true; ;

    }

    IEnumerator FreeSpinRoutine()
    {
        if (!isFreeSpin)
            yield break;

        audioController.playBgAudio("FP");
        // isFreeSpin = true;
        if (autoStop_Button.gameObject.activeSelf)
            autoStop_Button.gameObject.SetActive(false);
        if (!autoStart_Button.gameObject.activeSelf)
        {
            autoStart_Button.interactable = false;
            autoStart_Button.gameObject.SetActive(true);
        }
        uI_Controller.SetFreeSpinUI();
        while (freeSpinCount > 0)
        {
            freeSpinCount--;
            yield return SpinRoutine();
            yield return new WaitForSeconds(1f);
        }

        isSpinning = false;
        isFreeSpin = false;
        uI_Controller.SetDefaultUI();
        freeSpinRoutine = null;

        if (wasAutoSpinOn)
        {
            if (autoSpinCoroutine != null)
                StopCoroutine(autoSpinCoroutine);
            autoSpinCoroutine = StartCoroutine(AutoSpinRoutine());
        }
        else
            ToggleButtonGrp(true);


    }

    IEnumerator SpinRoutine()
    {
        bool start = OnSpinStart();
        if (!start)
        {
            isSpinning = false;
            if (isAutoSpin)
            {
                isAutoSpin = false;
                autoStop_Button.interactable = false;
                StartCoroutine(AutoSpinStopRoutine());
            }
            ToggleButtonGrp(true);
            yield break;
        }
        yield return OnSpin();
        yield return OnSpinEnd();


    }

    bool OnSpinStart()
    {

        isSpinning = true;

        spinInfoText.text = "Good Luck";
        audioController.PlayButtonAudio("spin");
        uI_Controller.SetFreeSpinCount(-1, false);
        winHighlight?.Kill();
        winHighlight = null;
        uI_Controller.ResetWin();
        uI_Controller.UpdatePlayerInfo(0, socketManager.socketModel.playerData.Balance);
        ToggleButtonGrp(false);
        if (!isFreeSpin)
        {
            bool start = CompareBalance();
            if (start)
                uI_Controller.DeductBalance(currentTotalBet);
            return start;
        }
        return true;
    }

    IEnumerator OnSpin()
    {
        if (!isAutoSpin && !isFreeSpin)
            StopSpin_Button.gameObject.SetActive(true);

        audioController.PlaySpinAudio();
        var spinData = new { data = new { currentBet = betCounter, currentLines = totalLines, spins = 1 }, id = "SPIN" };
        socketManager.SendData("message", spinData);
        yield return reel_Controller.ClearReel();
        yield return new WaitUntil(() => socketManager.isResultdone);

        yield return reel_Controller.FillReelv1(socketManager.socketModel.resultGameData.resultSymbols, () => audioController.PlaySpinAudio("stop"));
        if (StopSpin_Button.gameObject.activeSelf)
        {
            StopSpin_Button.gameObject.SetActive(false);

        }
        if (immediateStop)
            yield return new WaitForSeconds(0.5f);



    }
    IEnumerator OnSpinEnd()
    {
        List<ImageAnimation> pullingAnimList = new List<ImageAnimation>();
        List<string> SymbolsToEmit;
        List<string>[] symbols = new List<string>[2];
        List<int[]> coords = new List<int[]>();
        
        int lineId = -1;
        var cascadeData = socketManager.socketModel.resultGameData.cascading;
        Color borderColor;
        spinInfoText.text = "";
        if (isFreeSpin)
            spinInfoText.text += $"free spin left {freeSpinCount} ";
        
        var playerData = socketManager.socketModel.playerData;

        currentBalance = playerData.Balance;
        
        if( playerData.CurrentWining>0)
        winHighlight = uI_Controller.HighLightWin();

        if (cascadeData.Count > 0)
        {

            audioController.PlayWLAudio();

            for (int k = 0; k < cascadeData.Count; k++)
            {
                coords.Clear();

                SymbolsToEmit = Helper.Flatten2DList(cascadeData[k].winingSymbols);
                //separating which symbols to pull and which to blast
                symbols = SeparateSymbols(SymbolsToEmit);

                uI_Controller.UpdatePlayerInfo(cascadeData[k].currentWining);

                if (isFreeSpin)
                    spinInfoText.text = $"free spin left {freeSpinCount} You Won {cascadeData[k].currentWining} ";
                else
                    spinInfoText.text = $"You Won {cascadeData[k].currentWining} ";

                ShowAllLinesAtOnce(cascadeData[k].lineToEmit,Helper.Flatten2DList(cascadeData[k].winingSymbols));
                yield return new WaitForSeconds(0.7f);
                RemoveAllLine(cascadeData[k].lineToEmit);
                yield return new WaitForSeconds(0.1f);

                if (!isFreeSpin && !isAutoSpin)
                {
                    for (int i = 0; i < cascadeData[k].lineToEmit.Count; i++)
                    {
                        lineId = cascadeData[k].lineToEmit[i] - 1;
                        borderColor = payline_Controller.GeneratePayline(lineId);
                        reel_Controller.HighlightIconByLine(socketManager.socketModel.initGameData.lineData[lineId],cascadeData[k].winingSymbols[i], borderColor);
                        yield return new WaitForSeconds(reel_Controller.minClearDuration + 0.2f);
                        reel_Controller.StopHighlightIcon(socketManager.socketModel.initGameData.lineData[lineId]);
                        payline_Controller.DestroyPayline(lineId);
                        yield return new WaitForSeconds(reel_Controller.minClearDuration + 0.2f);
                    }
                }
                // symbols to blast
                if (symbols[1].Count > 0)
                {
                    audioController.PlayShootAudio();
                    for (int i = 0; i < symbols[1].Count; i++)
                    {
                        coords.Add(uFO_Controller.uFoAnimation(Helper.ConvertSymbolPos(symbols[1][i])));
                    }

                    yield return new WaitForSeconds(0.6f);
                    uFO_Controller.StopUfoAnimation();
                    for (int i = 0; i < coords.Count; i++)
                    {
                        uFO_Controller.Shoot(coords[i]);
                        yield return new WaitForSeconds(0.05f);
                    }
                    audioController.PlayBlastAudio();

                }
                // symbols to pull
                if (symbols[0].Count > 0)
                {

                    audioController.PlayPullAudio();
                    for (int i = 0; i < symbols[0].Count; i++)
                    {
                        pullingAnimList.Add(uFO_Controller.Pull(Helper.ConvertSymbolPos(symbols[0][i])));

                    }
                }
                yield return new WaitForSeconds(uFO_Controller.shootSpeed - 0.1f);
                //to handle blast animation
                reel_Controller.HanldleSymbols(symbols[1]);

                uI_Controller.UpdatePlayerInfo(cascadeData[k].currentWining);
                //to handle wild animation
                reel_Controller.HandleWildSymbols(symbols[0]);
                yield return new WaitForSeconds(1f);

                uFO_Controller.StartUfoVerticalMove();

                reel_Controller.StopSymbolAnimation(SymbolsToEmit);
                for (int i = 0; i < pullingAnimList.Count; i++)
                {
                    pullingAnimList[i].StopAnimation();
                    Destroy(pullingAnimList[i].gameObject);
                }
                pullingAnimList.Clear();
                yield return new WaitForSeconds(0.2f);
                yield return reel_Controller.ReArrangeMatrix(cascadeData[k].symbolsToFill);
                uI_Controller.SetFreeSpinCount(k, isFreeSpin);
                yield return new WaitForSeconds(0.5f);

            }
            // yield return new WaitForSeconds(1f);

        }



        uI_Controller.UpdatePlayerInfo(playerData.CurrentWining, playerData.Balance);
        double winAmount = playerData.CurrentWining;
        int winType = -1;

        if (winAmount > 0)
            spinInfoText.text = $"Total Winnings {winAmount} !";
        else
            spinInfoText.text = $"Better Luck Next Time";


        Debug.Log("before checking -" + playerData.CurrentWining);

        if (socketManager.socketModel.resultGameData.jackpot > 0)
        {
            winAmount = socketManager.socketModel.resultGameData.jackpot;
            winType = 3;
        }
        else if (winAmount >= currentTotalBet * 10 && currentTotalBet * 15 > winAmount) winType = 0;

        else if (winAmount >= currentTotalBet * 15 && currentTotalBet * 20 > winAmount) winType = 1;

        else if (winAmount >= currentTotalBet * 20) winType = 2;

        if (winType >= 0)
        {
            checkPopUpCompletion = false;
            uI_Controller.ShowWinPopup(winType, winAmount);
            yield return new WaitWhile(() => !checkPopUpCompletion);

        }

        if (socketManager.socketModel.resultGameData.isFreeSpin)
        {
            isFreeSpin = true;
            isAutoSpin = false;
            if (autoSpinCoroutine != null)
            {
                StopCoroutine(autoSpinCoroutine);
                wasAutoSpinOn = true;
            }

            freeSpinCount = socketManager.socketModel.resultGameData.freeSpinCount;

            if (freeSpinRoutine != null)
            {
                isFreeSpin = false;
                StopCoroutine(freeSpinRoutine);
                freeSpinRoutine = null;
                // uI_Controller.ShowFreeSpinPopup(freeSpinCount, false);
                // yield return new WaitForSeconds(3f);
                // freeSpinRoutine = StartCoroutine(FreeSpinRoutine());
            }

            uI_Controller.ShowFreeSpinPopup(freeSpinCount);
            yield return new WaitForSeconds(2f);
            freeSpinRoutine = StartCoroutine(FreeSpinRoutine());
        }

        uI_Controller.UpdatePlayerInfo(socketManager.socketModel.playerData.CurrentWining, socketManager.socketModel.playerData.Balance);
        if (!isAutoSpin && !isFreeSpin)
        {

            isSpinning = false;
            ToggleButtonGrp(true);
        }

    }


    void ShowAllLinesAtOnce(List<int> lineToEmit, List<string> winingSymbols)
    {
        Color borderColor;
        for (int i = 0; i < lineToEmit.Count; i++)
        {
            borderColor = payline_Controller.GeneratePayline(lineToEmit[i] - 1);
            reel_Controller.HighlightIconByLine(socketManager.socketModel.initGameData.lineData[lineToEmit[i] - 1], winingSymbols, borderColor);
        }

    }

    void RemoveAllLine(List<int> lineToEmit)
    {
        for (int i = 0; i < lineToEmit.Count; i++)
        {

            reel_Controller.StopHighlightIcon(socketManager.socketModel.initGameData.lineData[lineToEmit[i] - 1]);
            payline_Controller.DestroyPayline(lineToEmit[i] - 1);

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

    void ChangeBet(bool inc)
    {
        audioController.PlayButtonAudio();
        if (inc)
        {
                betCounter++;
            if (betCounter > socketManager.socketModel.initGameData.Bets.Count - 1)
                betCounter=0;
        }
        else
        {

                betCounter--;
            if (betCounter < 0)
                betCounter=socketManager.socketModel.initGameData.Bets.Count - 1;



        }
        currentTotalBet = socketManager.socketModel.initGameData.Bets[betCounter] * socketManager.socketModel.initGameData.lineData.Count;
        spinInfoText.text = $"Total lines: {totalLines} x Bet per line: {socketManager.socketModel.initGameData.Bets[betCounter]} = Total bet: {currentTotalBet}";
        uI_Controller.UpdateBetInfo(socketManager.socketModel.initGameData.Bets[betCounter], currentTotalBet, totalLines);



    }
    void InitiateUI(List<Symbol> uiData, List<List<int>> freeSpinData, PlayerData playerData)
    {
        if (inititated)
        {
            uI_Controller.InitUI(uiData, freeSpinData);
            return;

        }
        uI_Controller.InitUI(uiData, freeSpinData);
        uI_Controller.UpdatePlayerInfo(0, playerData.Balance);
        currentBalance = playerData.Balance;
        totalLines = socketManager.socketModel.initGameData.lineData.Count;
        currentTotalBet = socketManager.socketModel.initGameData.Bets[betCounter] * totalLines;
        spinInfoText.text = $"Total lines: {totalLines} x Bet per line: {socketManager.socketModel.initGameData.Bets[betCounter]} = Total bet: {currentTotalBet}";
        uI_Controller.UpdateBetInfo(socketManager.socketModel.initGameData.Bets[betCounter], currentTotalBet, totalLines);
        inititated = true;
        CompareBalance();
        Application.ExternalCall("window.parent.postMessage", "OnEnter", "*");


    }

    private bool CompareBalance()
    {

        if (currentBalance < currentTotalBet)
        {
            uI_Controller.ShowLowBalPopup();

            return false;
        }
        else
        {
            return true;
        }
    }

    void ToggleButtonGrp(bool toggle)
    {
        Debug.Log("enetered here");
        start_Button.interactable = toggle;
        autoStart_Button.interactable = toggle;
        betMinus_Button.interactable = toggle;
        betPlus_Button.interactable = toggle;
        uI_Controller.ToggleBtnGrp(toggle);

    }

}
