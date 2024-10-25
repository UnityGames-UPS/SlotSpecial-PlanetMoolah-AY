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
    // [SerializeField] private Transform info_button;
    // [SerializeField] private Button Menu_Button;

    [Header("Bet info")]
    [SerializeField] private TMP_Text betPerLineText;
    [SerializeField] private TMP_Text totalBetText;
    [SerializeField] private TMP_Text totalLineText;

    [Header("Popus UI")]
    [SerializeField] private GameObject MainPopup_Object;
    [Header("win Popup")]
    [SerializeField] private GameObject winPopUpObject;
    [SerializeField] private TMP_Text winTitle;
    [SerializeField] private TMP_Text winAmountText;

    [Header("Free Spin Info")]
    [SerializeField] private TMP_Text[] freeSpinCounters;

    [Header("Free Spin Popup")]
    [SerializeField] private GameObject freeSpinObject;
    [SerializeField] private Sprite freeSpinBG;
    [SerializeField] private Sprite defaultBG;
    [SerializeField] private Sprite freeSpinReel;
    [SerializeField] private Sprite defaultReel;
    [SerializeField] private TMP_Text freeSpinText;
    [SerializeField] private Image bg;
    [SerializeField] private Image reeelBg;
    [SerializeField] private Image[] planets;
    [SerializeField] private GameObject[] freeSpincounts;
    [SerializeField] private Color freeSpinColor;
    [SerializeField] private Button freeSpinStartButton;
    [SerializeField] private GameObject freeSpinCounterObject;


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
    [SerializeField] private TMP_Text wildText;
    [SerializeField] private TMP_Text jackPotText;
    [SerializeField] private TMP_Text freeSpinDetails;
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

    [Header("Quit popup")]
    [SerializeField] private Button GameExit_Button;
    [SerializeField] private GameObject quitPopupObject;
    [SerializeField] private Button quitPopUpYesButton;
    [SerializeField] private Button quitPopUpNoButton;


    [SerializeField] private bool isMusic = true;
    [SerializeField] private bool isSound = true;
    private bool isMenuOpen = false;
    [SerializeField] private GameObject[] pageList;
    [SerializeField] private int currentPage = 0;


    [Header("player Info")]
    [SerializeField] private TMP_Text playerBalance;
    [SerializeField] private TMP_Text playerWinning;

    [SerializeField] GameObject currentPopup = null;

    internal Action<bool, string> OnToggleAudio;
    internal Action<string> OnPlayButton;
    internal Action Exitgame;
    private void Start()
    {


        if (Paytable_Button) Paytable_Button.onClick.RemoveAllListeners();
        if (Paytable_Button) Paytable_Button.onClick.AddListener(delegate
        {
            OpenPopup(PaytablePopup_Object);
            OnPlayButton("default");

        });

        if (PaytableExit_Button) PaytableExit_Button.onClick.RemoveAllListeners();
        if (PaytableExit_Button) PaytableExit_Button.onClick.AddListener(delegate
        {
            ClosePopup();
            OnPlayButton("default");

        });

        if (Settings_Button) Settings_Button.onClick.RemoveAllListeners();
        if (Settings_Button) Settings_Button.onClick.AddListener(delegate
        {
            OpenPopup(SettingsPopup_Object);
            OnPlayButton("default");


        });

        if (SettingsExit_Button) SettingsExit_Button.onClick.RemoveAllListeners();
        if (SettingsExit_Button) SettingsExit_Button.onClick.AddListener(delegate
        {
            ClosePopup();
            OnPlayButton("default");


        });

        if (prev) prev.onClick.RemoveAllListeners();
        if (prev) prev.onClick.AddListener(delegate
        {
            TogglePage(true);
            OnPlayButton("default");


        });


        if (next) next.onClick.RemoveAllListeners();
        if (next) next.onClick.AddListener(delegate
        {
            TogglePage(false);
            OnPlayButton("default");

        });


        // if (Menu_Button) Menu_Button.onClick.RemoveAllListeners();
        // if (Menu_Button) Menu_Button.onClick.AddListener(OpenMenu);

        if (Sound_Button) Sound_Button.onClick.RemoveAllListeners();
        if (Sound_Button) Sound_Button.onClick.AddListener(delegate
        {
            ToggleMusic();
            OnPlayButton("default");

        });

        if (Music_Button) Music_Button.onClick.RemoveAllListeners();
        if (Music_Button) Music_Button.onClick.AddListener(delegate
        {
            ToggleMusic();
            OnPlayButton("default");

        });

        if (Sound_Button) Sound_Button.onClick.RemoveAllListeners();
        if (Sound_Button) Sound_Button.onClick.AddListener(delegate
        {
            ToggleSound();
            OnPlayButton("default");
        });

        if (MusicOn_Object) MusicOn_Object.SetActive(true);
        if (MusicOff_Object) MusicOff_Object.SetActive(false);

        if (SoundOn_Object) SoundOn_Object.SetActive(true);
        if (SoundOff_Object) SoundOff_Object.SetActive(false);

        if (GameExit_Button) GameExit_Button.onClick.RemoveAllListeners();
        if (GameExit_Button) GameExit_Button.onClick.AddListener(() =>
        {
            OnPlayButton("default");
            OpenPopup(quitPopupObject);
        });

        if (quitPopUpYesButton) quitPopUpYesButton.onClick.AddListener(() =>
        {
            OnPlayButton("default");
            Exitgame();
        });

        if (quitPopUpNoButton) quitPopUpNoButton.onClick.AddListener(() =>
        {
            OnPlayButton("default");
            ClosePopup();
        });

        isMusic = true;
        isSound = true;

        if (disconnectCloseButton) disconnectCloseButton.onClick.RemoveAllListeners();
        if (disconnectCloseButton) disconnectCloseButton.onClick.AddListener(() =>
        {
            Exitgame();
        });

        if (lowbalCloseButton) lowbalCloseButton.onClick.RemoveAllListeners();
        if (lowbalCloseButton) lowbalCloseButton.onClick.AddListener(() =>
        {
            ClosePopup();
        });

    }

    internal void UpdatePlayerInfo(double currentWinning = -1, double balance = -1)
    {
        if (balance >= 0)
            playerBalance.text = balance.ToString();
        if (currentWinning >= 0)
            playerWinning.text = currentWinning.ToString();

    }
    // private void OpenMenu()
    // {
    //     if (!isMenuOpen)
    //     {
    //         settings_button.gameObject.SetActive(true);
    //         info_button.gameObject.SetActive(true);
    //         settings_button.transform.DOLocalMoveY(-135, 0.2f);
    //         info_button.transform.DOLocalMoveY(-270, 0.5f);
    //         isMenuOpen = true;
    //     }
    //     else
    //     {
    //         settings_button.DOLocalMoveY(0, 0.2f);
    //         info_button.DOLocalMoveY(0, 0.2f);

    //         DOVirtual.DelayedCall(0.1f, () =>
    //         {
    //             settings_button.gameObject.SetActive(false);
    //             info_button.gameObject.SetActive(false);
    //             isMenuOpen = false;
    //         });


    //     }

    // }



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
            if (currentPopup.name.ToUpper() == "DISCONNECTPOPUP")
                return;

            currentPopup.SetActive(false);
            currentPopup = null;
            // if (Popup) Popup.SetActive(false);
            if (MainPopup_Object) MainPopup_Object.SetActive(false);
        }


    }

    internal void SetFreeSpinCount(int count, bool isFreeSpin)
    {
        if(isFreeSpin)
        return;

        if (count < 0)
        {

            foreach (var item in freeSpincounts)
            {
                item.SetActive(false);
            }
            return;
        }

        if (count >= 0 && count < freeSpincounts.Length)
        {

            freeSpincounts[count].SetActive(true);
        }
    }

    internal void SetFreeSpinUI()
    {
        ClosePopup();
        freeSpinText.text = "";
        freeSpinStartButton.gameObject.SetActive(true);
        freeSpinCounterObject.SetActive(false);
        bg.sprite = freeSpinBG;
        reeelBg.sprite = freeSpinReel;
        foreach (var item in planets)
        {
            item.color = freeSpinColor;
        }
    }

    internal void SetDefaultUI()
    {
        bg.sprite = defaultBG;
        freeSpinCounterObject.SetActive(true);

        reeelBg.sprite = defaultReel;
        foreach (var item in planets)
        {
            item.color = Color.white;
        }

    }
    internal void UpdateBetInfo(double betPerline, double totaleBet, int totalLine)
    {
        totalLineText.text = totalLine.ToString();

        betPerLineText.text = betPerline.ToString();
        totalBetText.text = totaleBet.ToString();

    }

    internal void ShowDisconnectPopup()
    {

        OpenPopup(disconnectPopupObject);

    }

    internal void ShowLowBalPopup()
    {
        OpenPopup(lowBalPopupObject);
    }

    internal void ShowFreeSpinPopup(int value, bool showStart=true)
    {
        freeSpinText.text = $"You are awarded {value} free plays Press start to play.";
        freeSpinStartButton.gameObject.SetActive(showStart);
        OpenPopup(freeSpinObject);
        if(!showStart)
        Invoke("SetFreeSpinUI",3f);
    }
    private void ToggleMusic()
    {

        isMusic = !isMusic;
        if (isMusic)
        {
            if (MusicOn_Object) MusicOn_Object.SetActive(true);
            if (MusicOff_Object) MusicOff_Object.SetActive(false);
            OnToggleAudio(false, "bg");
        }
        else
        {
            if (MusicOn_Object) MusicOn_Object.SetActive(false);
            if (MusicOff_Object) MusicOff_Object.SetActive(true);
            OnToggleAudio(true, "bg");

        }
    }

    private void ToggleSound()
    {
        isSound = !isSound;
        if (isSound)
        {
            if (SoundOn_Object) SoundOn_Object.SetActive(true);
            if (SoundOff_Object) SoundOff_Object.SetActive(false);
            OnToggleAudio(false, "wl");
            OnToggleAudio(false, "button");

        }
        else
        {
            if (SoundOn_Object) SoundOn_Object.SetActive(false);
            if (SoundOff_Object) SoundOff_Object.SetActive(true);
            OnToggleAudio(true, "wl");
            OnToggleAudio(true, "button");
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

        OpenPopup(winPopUpObject);
        double currentValue = 0;
        DOTween.To(() => currentValue, x => currentValue = x, amount, 2f)
    .OnUpdate(() =>
    {
        winAmountText.text = currentValue.ToString("f3");
    })
    .OnComplete(() =>
    {
        winAmountText.text = amount.ToString();
    });


        yield return new WaitForSeconds(3f);
        ClosePopup();

    }

    internal void DeductBalance(double bet)
    {

        double balance = Double.Parse(playerBalance.text);
        double currentValue = balance;
        DOTween.To(() => currentValue, x => currentValue = x, (balance - bet), 0.4f)
        .OnUpdate(() =>
        {
            playerBalance.text = currentValue.ToString("f3");
        })
        .OnComplete(() =>
        {
            playerBalance.text = (balance - bet).ToString();
        });

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


    internal void InitUI(List<Symbol> symbolInfo, List<List<int>> freeSpinInfo)
    {

        for (int i = 0; i < symbolInfo.Count; i++)
        {

            SetSymboltext(symbolInfo[i], i);
        }

        for (int i = 0; i < freeSpinCounters.Length; i++)
        {
            freeSpinCounters[i].text = freeSpinInfo[i][0].ToString();
        }

        for (int i = 0; i < freeSpinInfo.Count; i++)
        {
            if (i == 0)
                freeSpinDetails.text += $"{freeSpinInfo[i][0]} or more consecutive cascades trigger the free spins\n\n";

            freeSpinDetails.text += $"{freeSpinInfo[i][0]} consecutive cascades awards {freeSpinInfo[i][1]} free plays\n";
        }

    }



    void SetSymboltext(Symbol symbolInfo, int k)
    {
        if (symbolInfo.Name.ToUpper() == "JACKPOT")
        {
            jackPotText.text = symbolInfo.description.ToString();
            return;
        }
        else if (symbolInfo.Name.ToUpper() == "WILD")
        {

            wildText.text = symbolInfo.description.ToString();
            return;
        }
        // symbol_texts[i]

        SymbolsText[k].text = "";
        string info = "";
        for (int i = 0; i < symbolInfo.Multiplier.Count(); i++)
        {
            info += $"{5 - i}X - " + symbolInfo.Multiplier[i][0].ToString() + "\n";
        }
        if (SymbolsText[k]) SymbolsText[k].text = info;

    }

    internal void ToggleBtnGrp(bool toggle)
    {

        Paytable_Button.interactable = toggle;
        Settings_Button.interactable = toggle;
        Sound_Button.interactable = toggle;
        Music_Button.interactable = toggle;
    }

    internal Tweener HighLightWin()
    {

        Tweener winTween = playerWinning.transform.DOScale(1.2f, 1f).SetLoops(-1, LoopType.Yoyo);

        return winTween;

    }

    internal void ResetWin()
    {
        playerWinning.transform.localScale = Vector3.one;
    }
}
