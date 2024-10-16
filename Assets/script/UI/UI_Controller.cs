using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting;

public class UI_Controller : MonoBehaviour
{


    // [SerializeField] private SocketIOManager socketIOManager;

    [Header("Menu UI")]
    [SerializeField] private Transform settings_button;
    [SerializeField] private Transform info_button;
    [SerializeField] private Button Menu_Button;

    [Header("Bet info")]
    [SerializeField] private TMP_Text betPerLineText;
    [SerializeField] private TMP_Text totalBetText;

    [Header("Popus UI")]
    [SerializeField]
    private GameObject MainPopup_Object;
    [Header("win Popup")]
    [SerializeField] private GameObject winPopUpObject;
    [SerializeField] private TMP_Text winTitle;
    [SerializeField] private TMP_Text winAmountText;

    [Header("disconnection Popup")]
    [SerializeField] private GameObject disconnectPopupObject;
    [SerializeField] private Button disconnectCloseButton;

    [Header("Low balance Popup")]
    [SerializeField] private GameObject lowBalPopupObject;
    [SerializeField] private Button lowbalCloseButton;

    [Header("Paytable Popup")]
    [SerializeField] private GameObject PaytablePopup_Object;
    [SerializeField] private Button PaytableExit_Button;
    [SerializeField] private TMP_Text[] SymbolsText;
    [SerializeField] private TMP_Text[] SpecialSymbolsText;
    [SerializeField] private Button Paytable_Button;

    [Header("pagination button")]
    [SerializeField] private Button next;
    [SerializeField] private Button prev;


    [Header("Settings Popup")]
    [SerializeField]
    private Button Settings_Button;
    [SerializeField]
    private GameObject SettingsPopup_Object;
    [SerializeField]
    private Button SettingsExit_Button;
    [SerializeField]
    private Button Sound_Button;
    [SerializeField]
    private Button Music_Button;

    [SerializeField]
    private GameObject MusicOn_Object;
    [SerializeField]
    private GameObject MusicOff_Object;
    [SerializeField]
    private GameObject SoundOn_Object;
    [SerializeField]
    private GameObject SoundOff_Object;



    [SerializeField]
    private Button GameExit_Button;

    [SerializeField] private bool isMusic = true;
    [SerializeField] private bool isSound = true;
    private bool isMenuOpen = false;
    [SerializeField] private GameObject[] pageList;
    [SerializeField] private int currentPage = 0;

    [Header("Info UI")]
    [SerializeField] private TMP_Text[] symbol_texts;
    [SerializeField] private TMP_Text wild_text;
    [SerializeField] private TMP_Text jackpot_texts;

    [Header("player Info")]
    [SerializeField] private TMP_Text playerBalance;
    [SerializeField] private TMP_Text playerWinning;

    [SerializeField] GameObject currentPopup = null;

    internal Action Exitgame;
    private void Start()
    {



        if (Paytable_Button) Paytable_Button.onClick.RemoveAllListeners();
        if (Paytable_Button) Paytable_Button.onClick.AddListener(delegate { OpenPopup(PaytablePopup_Object); });

        if (PaytableExit_Button) PaytableExit_Button.onClick.RemoveAllListeners();
        if (PaytableExit_Button) PaytableExit_Button.onClick.AddListener(delegate { ClosePopup(); });

        if (Settings_Button) Settings_Button.onClick.RemoveAllListeners();
        if (Settings_Button) Settings_Button.onClick.AddListener(delegate { OpenPopup(SettingsPopup_Object); });

        if (SettingsExit_Button) SettingsExit_Button.onClick.RemoveAllListeners();
        if (SettingsExit_Button) SettingsExit_Button.onClick.AddListener(delegate { ClosePopup(); });

        if (prev) prev.onClick.RemoveAllListeners();
        if (prev) prev.onClick.AddListener(delegate { TogglePage(true); });


        if (next) next.onClick.RemoveAllListeners();
        if (next) next.onClick.AddListener(delegate { TogglePage(false); });


        if (Menu_Button) Menu_Button.onClick.RemoveAllListeners();
        if (Menu_Button) Menu_Button.onClick.AddListener(OpenMenu);

        if (Sound_Button) Sound_Button.onClick.RemoveAllListeners();
        if (Sound_Button) Sound_Button.onClick.AddListener(ToggleMusic);

        if (Music_Button) Music_Button.onClick.RemoveAllListeners();
        if (Music_Button) Music_Button.onClick.AddListener(ToggleMusic);

        if (MusicOn_Object) MusicOn_Object.SetActive(true);
        if (MusicOff_Object) MusicOff_Object.SetActive(false);

        if (SoundOn_Object) SoundOn_Object.SetActive(true);
        if (SoundOff_Object) SoundOff_Object.SetActive(false);

        if (GameExit_Button) GameExit_Button.onClick.RemoveAllListeners();
        if (GameExit_Button) GameExit_Button.onClick.AddListener(() =>
        {
            Exitgame();
        });

        //if (FreeSpin_Button) FreeSpin_Button.onClick.RemoveAllListeners();
        //if (FreeSpin_Button) FreeSpin_Button.onClick.AddListener(delegate { StartFreeSpins(FreeSpins); });

        //if (audioController) audioController.ToggleMute(false);

        isMusic = true;
        isSound = true;

        if (Sound_Button) Sound_Button.onClick.RemoveAllListeners();
        if (Sound_Button) Sound_Button.onClick.AddListener(ToggleSound);

        if (Music_Button) Music_Button.onClick.RemoveAllListeners();
        if (Music_Button) Music_Button.onClick.AddListener(ToggleMusic);

        if (disconnectCloseButton) disconnectCloseButton.onClick.RemoveAllListeners();
        if (disconnectCloseButton) disconnectCloseButton.onClick.AddListener(() =>
        {
            Exitgame();
        });

        if (lowbalCloseButton) lowbalCloseButton.onClick.RemoveAllListeners();
        if (lowbalCloseButton) lowbalCloseButton.onClick.AddListener(() =>
        {
            Exitgame();
        });

    }

    internal void UpdatePlayerInfo(double currentWinning = -1, double balance =-1)
    {
        Debug.Log("balance "+balance);
        if (balance >= 0)
            playerBalance.text = balance.ToString();
        if (currentWinning >= 0)
            playerWinning.text = currentWinning.ToString();

    }
    private void OpenMenu()
    {
        if (!isMenuOpen)
        {
            settings_button.gameObject.SetActive(true);
            info_button.gameObject.SetActive(true);
            settings_button.transform.DOLocalMoveY(-135, 0.2f);
            info_button.transform.DOLocalMoveY(-270, 0.5f);
            isMenuOpen = true;
        }
        else
        {
            settings_button.DOLocalMoveY(0, 0.2f);
            info_button.DOLocalMoveY(0, 0.2f);

            DOVirtual.DelayedCall(0.1f, () =>
            {
                settings_button.gameObject.SetActive(false);
                info_button.gameObject.SetActive(false);
                isMenuOpen = false;
            });


        }

    }


    private void CallOnExitFunction()
    {
        //slotManager.CallCloseSocket();

    }

    private void OpenPopup(GameObject Popup)
    {
        //if (audioController) audioController.PlayButtonAudio();

        if (currentPopup != null)
        {

            if (currentPopup.name.ToUpper() == "DISCONNECTPOPUP" || currentPopup.name.ToUpper() == "LOWBALANCEPOPUP")
                return;
            currentPopup.SetActive(false);
        }

        currentPopup = Popup;
        if (Popup) Popup.SetActive(true);
        if (MainPopup_Object) MainPopup_Object.SetActive(true);

    }

    private void ClosePopup()
    {
        //if (audioController) audioController.PlayButtonAudio();
        if (currentPopup != null)
        {
            if (currentPopup.name.ToUpper() == "DISCONNECTPOPUP" || currentPopup.name.ToUpper() == "LOWBALANCEPOPUP")
                return;

            currentPopup.SetActive(false);
            currentPopup = null;
            // if (Popup) Popup.SetActive(false);
            if (MainPopup_Object) MainPopup_Object.SetActive(false);
        }


    }

    internal void UpdateBetInfo(double betPerline, double totaleBet){

        betPerLineText.text=betPerline.ToString();
        totalBetText.text=totaleBet.ToString();

    }

    internal void ShowDisconnectPopup()
    {

        OpenPopup(disconnectPopupObject);

    }

    internal void ShowLowBalPopup(){
        OpenPopup(lowBalPopupObject);
    }
    private void ToggleMusic()
    {
        print("triggered");

        isMusic = !isMusic;
        if (isMusic)
        {
            if (MusicOn_Object) MusicOn_Object.SetActive(true);
            if (MusicOff_Object) MusicOff_Object.SetActive(false);
            //audioController.ToggleMute(false, "bg");
        }
        else
        {
            if (MusicOn_Object) MusicOn_Object.SetActive(false);
            if (MusicOff_Object) MusicOff_Object.SetActive(true);
            //audioController.ToggleMute(true, "bg");
        }
    }

    private void ToggleSound()
    {
        isSound = !isSound;
        if (isSound)
        {
            if (SoundOn_Object) SoundOn_Object.SetActive(true);
            if (SoundOff_Object) SoundOff_Object.SetActive(false);
            //audioController.ToggleMute(false, "bg");
        }
        else
        {
            if (SoundOn_Object) SoundOn_Object.SetActive(false);
            if (SoundOff_Object) SoundOff_Object.SetActive(true);
            //audioController.ToggleMute(true, "bg");
        }
    }

    internal IEnumerator ShowWinPopup(int type, double amount)
    {

        switch (type)
        {
            case 0:
                winTitle.text = "Big win";
                break;
            case 1:
                winTitle.text = "Huge win";
                break;
            case 2:
                winTitle.text = "Mega win";
                break;
            case 3:
                winTitle.text = "Jackpot";
                break;
            default:
                yield break;

        }

        winAmountText.text = amount.ToString();

        OpenPopup(winPopUpObject);
        yield return new WaitForSeconds(3f);
        ClosePopup();

    }
    void TogglePage(bool decrease)
    {

        if (decrease)
            currentPage--;
        else
            currentPage++;

        if (currentPage < 0)
            currentPage = 0;
        else if (currentPage > (pageList.Length - 1))
            currentPage = pageList.Length - 1;

        foreach (var item in pageList)
        {
            item.SetActive(false);
        }
        pageList[currentPage].SetActive(true);


    }


    internal void InitUI(List<Symbol> symbolInfo)
    {

        for (int i = 0; i < symbol_texts.Length; i++)
        {

            SetSymboltext(symbolInfo[i], symbol_texts[i]);
        }


        // for (int i = 0; i < paylines.symbols.Count; i++)
        // {
        //     if (i <=4)
        //         continue;

        //     string text = null;
        //     if (paylines.symbols[i].multiplier._5x != 0)
        //     {
        //         text += "5x - " + paylines.symbols[i].multiplier._5x;
        //     }
        //     if (paylines.symbols[i].multiplier._4x != 0)
        //     {
        //         text += "\n4x - " + paylines.symbols[i].multiplier._4x;
        //     }
        //     if (paylines.symbols[i].multiplier._3x != 0)
        //     {
        //         text += "\n3x - " + paylines.symbols[i].multiplier._3x;
        //     }
        //     if (paylines.symbols[i].multiplier._2x != 0)
        //     {
        //         text += "\n2x - " + paylines.symbols[i].multiplier._2x;
        //     }

        //     if (major_symbol_texts[i-5]) major_symbol_texts[i-5].text = text;

        // }



        // string text1 = null;
        // if (paylines.symbols[0].multiplier._5x != 0)
        // {
        //     text1 += "5x - " + paylines.symbols[0].multiplier._5x;
        // }
        // if (paylines.symbols[0].multiplier._4x != 0)
        // {
        //     text1 += "\n4x - " + paylines.symbols[0].multiplier._4x;
        // }
        // if (paylines.symbols[0].multiplier._3x != 0)
        // {
        //     text1 += "\n3x - " + paylines.symbols[0].multiplier._3x;
        // }
        // if (paylines.symbols[0].multiplier._2x != 0)
        // {
        //     text1 += "\n2x - " + paylines.symbols[0].multiplier._2x;
        // }
        // if (minor_symbol_text) minor_symbol_text.text = text1;



    }


    void SetSymboltext(Symbol symbolInfo, TMP_Text minor_symbol_text)
    {
        minor_symbol_text.text = "";
        string info = "";
        for (int i = 0; i < symbolInfo.Multiplier.Count(); i++)
        {
            info += $"{5 - i}X - " + symbolInfo.Multiplier[i][0].ToString() + "\n";
        }
        minor_symbol_text.text = info;
    }

}
