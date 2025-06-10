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
     //   socketManager.InitiateUI = InitiateUI;
        
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
        yield return OnSpinEnd2();


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
        uI_Controller.UpdatePlayerInfo(0, socketManager.PlayerData.balance);
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
       // var spinData = new { data = new { currentBet = betCounter, currentLines = totalLines, spins = 1 }, id = "SPIN" };
        socketManager.AccumulateResult(betCounter);
        yield return reel_Controller.ClearReel();
        Debug.Log("Reel Feel");
        yield return new WaitUntil(() => socketManager.isResultdone);
        Debug.Log("Reel Feel");
        yield return reel_Controller.FillReelv1(socketManager.ResultData.matrix, () => audioController.PlaySpinAudio("stop"));
        Debug.Log("Reel Feel");
        if (StopSpin_Button.gameObject.activeSelf)
        {
            StopSpin_Button.gameObject.SetActive(false);

        }
        if (immediateStop)
            yield return new WaitForSeconds(0.5f);



    }
    IEnumerator OnSpinEnd()
    {
        yield return null;
        List<ImageAnimation> pullingAnimList = new List<ImageAnimation>();
        List<string> SymbolsToEmit;
        List<string>[] symbols = new List<string>[2];
        List<int[]> coords = new List<int[]>();
        
        int lineId = -1;
        var cascadeData = socketManager.ResultData.cascades;
        Color borderColor;
        spinInfoText.text = "";
        if (isFreeSpin)
            spinInfoText.text += $"free spin left {freeSpinCount} ";
        
       // var playerData = socketManager.socketModel.playerData;

        currentBalance = socketManager.ResultData.totalWin;
        
        if(socketManager.ResultData.totalWin> 0)
        winHighlight = uI_Controller.HighLightWin();

        Debug.Log("AshuTest : " + socketManager.ResultData.cascades.Count);

        if (socketManager.ResultData.cascades.Count > 0)
        {

            audioController.PlayWLAudio();

            //    for (int k = 0; k < cascadeData.Count; k++)
            //    {
            //        coords.Clear();

            //        SymbolsToEmit = Helper.Flatten2DList(cascadeData[k].symbolsToFill);                    // Changed Here Ashu
            //        //separating which symbols to pull and which to blast
            //     //   symbols = SeparateSymbols(SymbolsToEmit);

            //        uI_Controller.UpdatePlayerInfo(cascadeData[k].currentCascadeWin);

            //        if (isFreeSpin)
            //            spinInfoText.text = $"free spin left {freeSpinCount} You Won {cascadeData[k].currentCascadeWin} ";
            //        else
            //            spinInfoText.text = $"You Won {cascadeData[k].currentCascadeWin} ";

            //        ShowAllLinesAtOnce(cascadeData[k].winningLines,Helper.Flatten2DList(cascadeData[k].symbolsToFill));                    // Changed Here Ashu
            //        yield return new WaitForSeconds(0.7f);
            //        RemoveAllLine(cascadeData[k].winningLines);                    // Changed Here Ashu
            //        yield return new WaitForSeconds(0.1f);

            //        if (!isFreeSpin && !isAutoSpin)
            //        {
            //            for (int i = 0; i < cascadeData[k].winningLines.Count; i++)                    // Changed Here Ashu
            //            {
            //                lineId = cascadeData[k].winningLines[i].lineIndex - 1;                    // Changed Here Ashu
            //                borderColor = payline_Controller.GeneratePayline(lineId);
            //                reel_Controller.HighlightIconByLine(socketManager.InitialData.lines[lineId],cascadeData[k].winningLines[i].positions, borderColor);                    // Changed Here Ashu
            //                yield return new WaitForSeconds(reel_Controller.minClearDuration + 0.2f);
            //                reel_Controller.StopHighlightIcon(socketManager.InitialData.lines[lineId]);
            //                payline_Controller.DestroyPayline(lineId);
            //                yield return new WaitForSeconds(reel_Controller.minClearDuration + 0.2f);
            //            }
            //        }
            //        // symbols to blast
            //        if (symbols[1].Count > 0)
            //        {
            //            audioController.PlayShootAudio();
            //            for (int i = 0; i < symbols[1].Count; i++)
            //            {
            //                coords.Add(uFO_Controller.uFoAnimation(Helper.ConvertSymbolPos(symbols[1][i])));
            //            }

            //            yield return new WaitForSeconds(0.6f);
            //            uFO_Controller.StopUfoAnimation();
            //            for (int i = 0; i < coords.Count; i++)
            //            {
            //                uFO_Controller.Shoot(coords[i]);
            //                yield return new WaitForSeconds(0.05f);
            //            }
            //            audioController.PlayBlastAudio();

            //        }
            //        // symbols to pull
            //        if (symbols[0].Count > 0)
            //        {

            //            audioController.PlayPullAudio();
            //            for (int i = 0; i < symbols[0].Count; i++)
            //            {
            //                pullingAnimList.Add(uFO_Controller.Pull(Helper.ConvertSymbolPos(symbols[0][i])));

            //            }
            //        }
            //        yield return new WaitForSeconds(uFO_Controller.shootSpeed - 0.1f);
            //        //to handle blast animation
            //        reel_Controller.HanldleSymbols(symbols[1]);

            //        uI_Controller.UpdatePlayerInfo(cascadeData[k].currentCascadeWin);
            //        //to handle wild animation
            //        reel_Controller.HandleWildSymbols(symbols[0]);
            //        yield return new WaitForSeconds(1f);

            //        uFO_Controller.StartUfoVerticalMove();

            //        reel_Controller.StopSymbolAnimation(SymbolsToEmit);
            //        for (int i = 0; i < pullingAnimList.Count; i++)
            //        {
            //            pullingAnimList[i].StopAnimation();
            //            Destroy(pullingAnimList[i].gameObject);
            //        }
            //        pullingAnimList.Clear();
            //        yield return new WaitForSeconds(0.2f);
            //        yield return reel_Controller.ReArrangeMatrix(cascadeData[k].winningLines);                    // Changed Here Ashu
            //        uI_Controller.SetFreeSpinCount(k, isFreeSpin);
            //        yield return new WaitForSeconds(0.5f);

            //    }
            //    // yield return new WaitForSeconds(1f);

            //}
        }


        uI_Controller.UpdatePlayerInfo(socketManager.ResultData.totalWin, socketManager.ResultData.player.balance);
        double winAmount = socketManager.ResultData.totalWin;
        int winType = -1;

        if (winAmount > 0)
            spinInfoText.text = $"Total Winnings {winAmount} !";
        else
            spinInfoText.text = $"Better Luck Next Time";


        Debug.Log("before checking -" + socketManager.ResultData.totalWin);

        //if (socketManager.ResultData.jackpot.isTriggered)
        //{
        //    winAmount = socketManager.ResultData.jackpot.amount;
        //    winType = 3;
        //}
        //else if (winAmount >= currentTotalBet * 10 && currentTotalBet * 15 > winAmount) winType = 0;

        //else if (winAmount >= currentTotalBet * 15 && currentTotalBet * 20 > winAmount) winType = 1;

        //else if (winAmount >= currentTotalBet * 20) winType = 2;

        //if (winType >= 0)
        //{
        //    checkPopUpCompletion = false;
        //    uI_Controller.ShowWinPopup(winType, winAmount);
        //    yield return new WaitWhile(() => !checkPopUpCompletion);

        //}

        //if (socketManager.ResultData.freeSpin.isFreeSpin)
        //{
        //    isFreeSpin = true;
        //    isAutoSpin = false;
        //    if (autoSpinCoroutine != null)
        //    {
        //        StopCoroutine(autoSpinCoroutine);
        //        wasAutoSpinOn = true;
        //    }

        //    freeSpinCount = socketManager.ResultData.freeSpin.count;

        //    if (freeSpinRoutine != null)
        //    {
        //        isFreeSpin = false;
        //        StopCoroutine(freeSpinRoutine);
        //        freeSpinRoutine = null;
        //        // uI_Controller.ShowFreeSpinPopup(freeSpinCount, false);
        //        // yield return new WaitForSeconds(3f);
        //        // freeSpinRoutine = StartCoroutine(FreeSpinRoutine());
        //    }

        //    uI_Controller.ShowFreeSpinPopup(freeSpinCount);
        //    yield return new WaitForSeconds(2f);
        //    freeSpinRoutine = StartCoroutine(FreeSpinRoutine());
        //}

        uI_Controller.UpdatePlayerInfo(socketManager.ResultData.totalWin, socketManager.ResultData.player.balance);
        if (!isAutoSpin && !isFreeSpin)
        {

            isSpinning = false;
            ToggleButtonGrp(true);
        }

    }

    IEnumerator OnSpinEnd2()
    {
        List<ImageAnimation> pullingAnimList = new List<ImageAnimation>();
        List<List<int>> SymbolsToEmit = new List<List<int>>();
        List<List<int>> symbols = new List<List<int>>();
        List<List<int>> WildSymbols = new List<List<int>>();
        List<int[]> coords = new List<int[]>();

        int lineId = -1;
        var cascadeData = socketManager.ResultData.cascades;
        Color borderColor;
        spinInfoText.text = "";
        if (isFreeSpin)
            spinInfoText.text += $"free spin left {freeSpinCount} ";

        // var playerData = socketManager.socketModel.playerData;

        currentBalance = socketManager.ResultData.player.balance;

        if (socketManager.ResultData.totalWin > 0)
            winHighlight = uI_Controller.HighLightWin();

        Debug.Log("AshuTest : " + socketManager.ResultData.cascades.Count);

        if (socketManager.ResultData.cascades.Count > 0)
        {

            audioController.PlayWLAudio();

            for (int k = 0; k < cascadeData.Count; k++)
            {
                coords.Clear();
                WildSymbols.Clear();
                symbols.Clear();
                SymbolsToEmit.Clear();

                foreach (var item in socketManager.ResultData.cascades[k].winningLines)
                {
                    SymbolsToEmit.AddRange( Helper.FindEmitingSymbol(item.lineIndex,item.positions,socketManager.InitialData.lines));                    // Changed Here Ashu

                }
                   
                //separating which symbols to pull and which to blast
                WildSymbols = SeparateSymbols(SymbolsToEmit,true);
                symbols = SeparateSymbols(SymbolsToEmit,false);

                uI_Controller.UpdatePlayerInfo(cascadeData[k].currentCascadeWin,socketManager.ResultData.player.balance);

                if (isFreeSpin)
                    spinInfoText.text = $"free spin left {freeSpinCount} You Won {cascadeData[k].currentCascadeWin} ";
                else
                    spinInfoText.text = $"You Won {cascadeData[k].currentCascadeWin} ";

                ShowAllLinesAtOnce(cascadeData[k].winningLines, Helper.Flatten2DList(cascadeData[k].symbolsToFill));                    // Changed Here Ashu
                yield return new WaitForSeconds(0.7f);
                RemoveAllLine(cascadeData[k].winningLines);                    // Changed Here Ashu
                yield return new WaitForSeconds(0.1f);

                if (!isFreeSpin && !isAutoSpin)
                {
                    for (int i = 0; i < cascadeData[k].winningLines.Count; i++)                    // Changed Here Ashu
                    {
                        lineId = cascadeData[k].winningLines[i].lineIndex ;                    // Changed Here Ashu
                        
                        borderColor = payline_Controller.GeneratePayline(lineId);
                        reel_Controller.HighlightIconByLine(socketManager.InitialData.lines[lineId], cascadeData[k].winningLines[i].positions, borderColor);                    // Changed Here Ashu
                        yield return new WaitForSeconds(reel_Controller.minClearDuration + 0.2f);
                        reel_Controller.StopHighlightIcon(socketManager.InitialData.lines[lineId]);
                        payline_Controller.DestroyPayline(lineId);
                        yield return new WaitForSeconds(reel_Controller.minClearDuration + 0.2f);
                    }
                }
                // symbols to blast
                if (symbols.Count > 0)
                {
                    audioController.PlayShootAudio();
                    for (int i = 0; i < symbols.Count; i++)
                    {
                        Debug.Log("DevTest : " + coords.Count);
                        coords.Add(uFO_Controller.uFoAnimation(symbols[i]));
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
                if (symbols.Count > 0)
                {

                    audioController.PlayPullAudio();
                    for (int i = 0; i < WildSymbols.Count; i++)
                    {
                        pullingAnimList.Add(uFO_Controller.Pull(WildSymbols[i]));

                    }
                }
                yield return new WaitForSeconds(uFO_Controller.shootSpeed - 0.1f);
                //to handle blast animation
                reel_Controller.HanldleSymbols(symbols);

                uI_Controller.UpdatePlayerInfo(cascadeData[k].currentCascadeWin,socketManager.ResultData.player.balance);
                //to handle wild animation
                reel_Controller.HandleWildSymbols(WildSymbols);
                yield return new WaitForSeconds(1f);

                uFO_Controller.StartUfoVerticalMove();

                reel_Controller.StopSymbolAnimation(SymbolsToEmit);                    // Changed Here Ashu
                for (int i = 0; i < pullingAnimList.Count; i++)
                {
                    pullingAnimList[i].StopAnimation();
                    Destroy(pullingAnimList[i].gameObject);
                }
                pullingAnimList.Clear();
                yield return new WaitForSeconds(0.2f);
                yield return reel_Controller.ReArrangeMatrix(cascadeData[k].symbolsToFill);                    // Changed Here Ashu
                uI_Controller.SetFreeSpinCount(k, isFreeSpin);
                yield return new WaitForSeconds(0.5f);

            }
            // yield return new WaitForSeconds(1f);

        }



        uI_Controller.UpdatePlayerInfo(socketManager.ResultData.totalWin, socketManager.ResultData.player.balance);
        double winAmount = socketManager.ResultData.totalWin;
        int winType = -1;

        if (winAmount > 0)
            spinInfoText.text = $"Total Winnings {winAmount} !";
        else
            spinInfoText.text = $"Better Luck Next Time";


        Debug.Log("before checking -" + socketManager.ResultData.totalWin);

        //if (socketManager.ResultData.jackpot.isTriggered)
        //{
        //    winAmount = socketManager.ResultData.jackpot.amount;
        //    winType = 3;
        //}
        //else if (winAmount >= currentTotalBet * 10 && currentTotalBet * 15 > winAmount) winType = 0;

        //else if (winAmount >= currentTotalBet * 15 && currentTotalBet * 20 > winAmount) winType = 1;

        //else if (winAmount >= currentTotalBet * 20) winType = 2;

        //if (winType >= 0)
        //{
        //    checkPopUpCompletion = false;
        //    uI_Controller.ShowWinPopup(winType, winAmount);
        //    yield return new WaitWhile(() => !checkPopUpCompletion);

        //}

        //if (socketManager.ResultData.freeSpin.isFreeSpin)
        //{
        //    isFreeSpin = true;
        //    isAutoSpin = false;
        //    if (autoSpinCoroutine != null)
        //    {
        //        StopCoroutine(autoSpinCoroutine);
        //        wasAutoSpinOn = true;
        //    }

        //    freeSpinCount = socketManager.ResultData.freeSpin.count;

        //    if (freeSpinRoutine != null)
        //    {
        //        isFreeSpin = false;
        //        StopCoroutine(freeSpinRoutine);
        //        freeSpinRoutine = null;
        //        // uI_Controller.ShowFreeSpinPopup(freeSpinCount, false);
        //        // yield return new WaitForSeconds(3f);
        //        // freeSpinRoutine = StartCoroutine(FreeSpinRoutine());
        //    }

        //    uI_Controller.ShowFreeSpinPopup(freeSpinCount);
        //    yield return new WaitForSeconds(2f);
        //    freeSpinRoutine = StartCoroutine(FreeSpinRoutine());
        //}

        uI_Controller.UpdatePlayerInfo(socketManager.ResultData.totalWin, socketManager.ResultData.player.balance);
        if (!isAutoSpin && !isFreeSpin)
        {

            isSpinning = false;
            ToggleButtonGrp(true);
        }

    }
    void ShowAllLinesAtOnce(List<WinningLine> lineToEmit, List<string> winingSymbols)
    {
        Color borderColor;
        for (int i = 0; i < lineToEmit.Count; i++)
        {
            borderColor = payline_Controller.GeneratePayline(lineToEmit[i].lineIndex);
            reel_Controller.HighlightIconByLine(socketManager.InitialData.lines[lineToEmit[i].lineIndex], lineToEmit[i].positions, borderColor);
        }

    }

    void ShowAllLinesAtOnce(List<int> lineToEmit, List<string> winingSymbols)
    {
        Color borderColor;
        for (int i = 0; i < lineToEmit.Count; i++)
        {
            borderColor = payline_Controller.GeneratePayline(lineToEmit[i] - 1);
            reel_Controller.HighlightIconByLine(socketManager.InitialData.lines[lineToEmit[i] - 1], winingSymbols, borderColor);
        }

    }
    void RemoveAllLine(List<WinningLine> lineToEmit)
    {
        for (int i = 0; i < lineToEmit.Count; i++)
        {

            reel_Controller.StopHighlightIcon(socketManager.InitialData.lines[lineToEmit[i].lineIndex ]);
            payline_Controller.DestroyPayline(lineToEmit[i].lineIndex);

        }
    }
    void RemoveAllLine(List<int> lineToEmit)
    {
        for (int i = 0; i < lineToEmit.Count; i++)
        {

            reel_Controller.StopHighlightIcon(socketManager.InitialData.lines[lineToEmit[i] - 1]);
            payline_Controller.DestroyPayline(lineToEmit[i] - 1);

        }
    }

    List<List<int>> SeparateSymbols(List<List<int>> SymbolsToEmit,bool wild = true)
    {
       

        List<List<int>> wildSymbol = new List<List<int>>();
        List<List<int>> symbol = new List<List<int>>();

        for (int i = 0; i < SymbolsToEmit.Count; i++)
        {
            List<int> pos = SymbolsToEmit[i];
            if (reel_Controller.CheckIfWild(pos))
            {
                wildSymbol.Add(pos);
            }
            else
            {
                symbol.Add(pos);
            }
        }


        if (wild) return wildSymbol;
        else return symbol;


    }

    void ChangeBet(bool inc)
    {
        audioController.PlayButtonAudio();
        if (inc)
        {
                betCounter++;
            if (betCounter > socketManager.InitialData.bets.Count - 1)
                betCounter=0;
        }
        else
        {

                betCounter--;
            if (betCounter < 0)
                betCounter=socketManager.InitialData.bets.Count - 1;



        }
        currentTotalBet = socketManager.InitialData.bets[betCounter] * socketManager.InitialData.lines.Count;
        spinInfoText.text = $"Total lines: {totalLines} x Bet per line: {socketManager.InitialData.bets[betCounter]} = Total bet: {currentTotalBet}";
        uI_Controller.UpdateBetInfo(socketManager.InitialData.bets[betCounter], currentTotalBet, totalLines);



    }
    internal void InitiateUI(List<Symbol> uiData, double playerData)
    {
        if (inititated)
        {
           // uI_Controller.InitUI(uiData, freeSpinData);
            return;

        }
      //  uI_Controller.InitUI(uiData, freeSpinData);
        uI_Controller.UpdatePlayerInfo(0, playerData);
        currentBalance = playerData;
        totalLines = socketManager.InitialData.lines.Count;
        currentTotalBet = socketManager.InitialData.bets[betCounter] * totalLines;
        spinInfoText.text = $"Total lines: {totalLines} x Bet per line: {socketManager.InitialData.bets[betCounter]} = Total bet: {currentTotalBet}";
        uI_Controller.UpdateBetInfo(socketManager.InitialData.bets[betCounter], currentTotalBet, totalLines);
        inititated = true;
        CompareBalance();



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
