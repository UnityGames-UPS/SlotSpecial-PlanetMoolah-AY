using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using TMPro;
using System;
using System.Linq;

public class Slot_Controller : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] internal Sprite[] iconList;
    [SerializeField] internal Sprite[] wildIconList;
    [SerializeField] internal List<Sprite> wildAnimationSprite;
    [SerializeField] internal List<Sprite> wildAnimationSprite1;
    [SerializeField] internal List<Sprite> wildAnimationSprite2;
    [SerializeField] internal List<Sprite> blastAnimationSprite;
    [SerializeField] internal Reel_Controller[] reels;
    [SerializeField] private GameObject linePrefab;
    [SerializeField] private Transform payLineContainer;
    [SerializeField] private int[] paylinePoints = new int[5];
    [SerializeField] private Vector2 initialPosition;
    [SerializeField] private Vector2 distance;
    [SerializeField] private Payline_controller payline_Controller;
    //[SerializeField] private List<Reel_Item>


    [SerializeField] private Transform bullet;

    [Header("Buttons")]
    [SerializeField] private Button spin_button;
    [SerializeField] private Button auto_spin_button;
    [SerializeField] private Button AutoSpinStop_Button;
    [SerializeField] private Button paytable_button;
    [SerializeField] private Button bet_plus_button;
    [SerializeField] private Button bet_minus_button;

    [Header("Texts")]
    [SerializeField] private TMP_Text TotalBet_text;
    [SerializeField] private TMP_Text BetperLine_text;
    [SerializeField] private TMP_Text Balance_text;
    [SerializeField] private TMP_Text WinAmount_text;

    [SerializeField] private bool isSpinning;
    [SerializeField] private bool isAutoSpin;
    private Coroutine tweenroutine;
    private Coroutine autoSpinRoutine;

    //to be filled by backend
    public List<List<string>> initialData;
    public List<List<string>> resultData;

    public List<List<int>> result = new List<List<int>>();
    public List<List<int>> iconsToRemove = new List<List<int>>();
    public List<List<int>> iconsToFill = new List<List<int>>();

    public Material laserMaterial;

    [Header("Controllers")]
    [SerializeField] private UFO_controller ufoController;
    [SerializeField] private SocketIOManager socketIOManager;
    // Duration of the animation in seconds
    double bet = 1;
    double balance = 0;
    int BetCounter = 0;
    bool isSubSpin=false;
    private void Start()
    {
        if (spin_button) spin_button.onClick.RemoveAllListeners();
        if (spin_button) spin_button.onClick.AddListener(StartSLot);

        if (auto_spin_button) auto_spin_button.onClick.RemoveAllListeners();
        if (auto_spin_button) auto_spin_button.onClick.AddListener(AutoSpin);

        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.RemoveAllListeners();
        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.AddListener(StopAutoSpin);

        if (bet_plus_button) bet_plus_button.onClick.RemoveAllListeners();
        if (bet_plus_button) bet_plus_button.onClick.AddListener(delegate { ChangeBet(true); });

        if (bet_minus_button) bet_minus_button.onClick.RemoveAllListeners();
        if (bet_minus_button) bet_minus_button.onClick.AddListener(delegate { ChangeBet(false); });

        initialData = new List<List<string>>{
            new List<string> { "13", "8", "1","10" },
            new List<string> { "13", "0", "9","1" },
            new List<string> { "13", "5", "3","3"},
            new List<string> { "13", "7", "10","2" },
            new List<string> { "13", "12", "6", "8" }
        };

        resultData = new List<List<string>> {
            new List<string> { "2", "4", "5","1","3" },
            new List<string> { "1", "3", "4","6","4" },
            new List<string> { "13", "13", "13","13","13" },
        };

        iconsToRemove = new List<List<int>>{

            new List<int> {0,0,0,0,0 },
            new List<int> {1,1,1,1,1 },
            new List<int> {2,2,2,2,2 },
            };

        iconsToFill = new List<List<int>>{

            new List<int> {4,4,4,4,4 },
            new List<int> {2,2,2,2,2 },
            new List<int> {3,2,5,2,1 },
            };



        //for (int i = 0; i < resultData[0].Count; i++)
        //{
        //    List<int> temp = new List<int>();
        //    for (int j = 0; j < resultData.Count; j++)
        //    {
        //        temp.Add(int.Parse(resultData[j][i]));

        //    }
        //    result.Add(temp);
        //}

        //populateSLot();

    }
    void Update()
    {

    }



    private void AutoSpin()
    {
        //if (audioController) audioController.PlayWLAudio("spin");
        if (!isAutoSpin)
        {
            isAutoSpin = true;
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(true);
            if (auto_spin_button) auto_spin_button.gameObject.SetActive(false);

            if (autoSpinRoutine != null)
            {
                StopCoroutine(autoSpinRoutine);
                autoSpinRoutine = null;
            }
            autoSpinRoutine = StartCoroutine(AutoSpinCoroutine());
        }
    }


    private void StopAutoSpin()
    {
        //if (audioController) audioController.PlayWLAudio("spin");
        if (isAutoSpin)
        {
            isAutoSpin = false;
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(false);
            if (auto_spin_button) auto_spin_button.gameObject.SetActive(true);
            StartCoroutine(StopAutoSpinCoroutine());
        }
    }

    private IEnumerator AutoSpinCoroutine()
    {

        while (isAutoSpin)
        {
            StartSLot();
            yield return tweenroutine;
            yield return new WaitForSeconds(1.5f);
        }
    }

    private IEnumerator StopAutoSpinCoroutine()
    {
        yield return new WaitUntil(() => !isSpinning);
        ToggleButtonGrp(true);
        if (tweenroutine != null)
        {
            StopCoroutine(autoSpinRoutine);
            StopCoroutine(tweenroutine);
            tweenroutine = null;
            autoSpinRoutine = null;
            StopCoroutine(StopAutoSpinCoroutine());

        }
    }

    void StartSLot()
    {
        if (isSpinning)
            return;
        tweenroutine = StartCoroutine(spinRoutine());

    }

    IEnumerator spinRoutine()
    {
        isSpinning = true;
        ToggleButtonGrp(false);
        foreach (Reel_Controller item in reels)
        {
            item.ClearReel(UnityEngine.Random.Range(0, 0.2f));

        }

        //try
        //{
        //    bet = double.Parse(TotalBet_text.text);
        //    balance = double.Parse(Balance_text.text);
        //}
        //catch (Exception e)
        //{
        //    Debug.Log("Error while conversion " + e.Message);
        //}
        socketIOManager.AccumulateResult(bet);
        yield return new WaitUntil(() => socketIOManager.isResultdone);
        result = TransposeMatrix(socketIOManager.resultData.ResultReel);
        yield return new WaitForSeconds(1.5f);
        for (int i = 0; i < reels.Length; i++)
        {
            reels[i].FillReel(result[i]);
            yield return new WaitForSeconds(0.2f);
        }

        

        List<int> iconsToRemove = ConvertListOfStringsToInts(socketIOManager.resultData.FinalsymbolsToEmit);
        SocketIOManager.PrintList(iconsToRemove);
        yield return CheckWinCoroutine(iconsToRemove);
        //socketIOManager.SubSpin(bet);
        //yield return new WaitUntil(()=>socketIOManager.isResultdone);

        if (WinAmount_text) WinAmount_text.text = socketIOManager.resultData.WinAmout.ToString();
        isSpinning = false;
        if (!isAutoSpin)
            ToggleButtonGrp(true);

    }



    void ChangeBet(bool IncDec)
    {
        //if (audioController) audioController.PlayButtonAudio();
        if (IncDec)
        {
            if (BetCounter < socketIOManager.initialData.bets.Count - 1)
            {
                BetCounter++;
            }
        }
        else
        {
            if (BetCounter > 0)
            {
                BetCounter--;
            }
        }

        if (TotalBet_text) TotalBet_text.text = socketIOManager.initialData.bets[BetCounter].ToString();
        if (BetperLine_text) BetperLine_text.text = ((float)socketIOManager.initialData.bets[BetCounter] / 25f).ToString();
    }


    IEnumerator CheckWinCoroutine(List<int> iconsToRemove)
    {
        //for (int i = 0; i < iconsToRemove.Count; i++)
        //{
        //yield return payline_Controller.TogglePayline(socketIOManager.resultData.linesToEmit,socketIOManager.resultData.FinalsymbolsToEmit);
        isSubSpin = true;
        yield return ufoController.LaserCoroutine(iconsToRemove);

        socketIOManager.SubSpin(bet);
        yield return new WaitUntil(() => socketIOManager.isResultdone);
        if (socketIOManager.resultData.iconsToFill.Count == 0)
            isSubSpin = false;
        if (isSubSpin)
        {
            iconsToRemove = ConvertListOfStringsToInts(socketIOManager.resultData.FinalsymbolsToEmit);
            print("triggered");
            //PrintMatrix(socketIOManager.resultData.iconsToFill);

            yield return OnWIn();
            yield return new WaitForSeconds(1.3f);
        }


        if (isSubSpin)
            yield return CheckWinCoroutine(iconsToRemove);



    }




    IEnumerator OnWIn()
    {
        //List<List<int>> resultTransposed = TransposeMatrix(socketIOManager.resultData.ResultReel);
        //List<List<int>> currentMatrix= new List<List<int>>();

        //for (int i = 0; i < reels.Length; i++)
        //{
        //    List<int> row = new List<int>();
        //    for (int j = 0; j < reels[i].currentReelItems.Count; j++)
        //    {
        //        if(reels[i].currentReelItems[j])
        //            row.Add(reels[i].currentReelItems[j].id);
        //        else
        //            row.Add(-1);

        //    }
        //    currentMatrix.Add(row);
        //}
        //List<List<int>> fillIcons = new List<List<int>>();

        //PrintMatrix(Transpose(currentMatrix));

        //for (int i = 0; i < resultTransposed.Count; i++)
        //{
        //    List<int> row = new List<int>();
        //    for (int k = 0; k < resultTransposed[i].Count; k++)
        //    {
        //        if (resultTransposed[i][k] != reels[i].currentReelItems[k].id) {

        //            fillIcons[i].Add(resultTransposed[i][k]);
        //        }
        //    }
        //    fillIcons.Add(row);
        //}
        //PrintMatrix(socketIOManager.resultData.iconsToFill);

        if (socketIOManager.resultData.iconsToFill?.Count > 0) {


            for (int j = 0; j < socketIOManager.resultData.iconsToFill.Count; j++)
            {
                    if (socketIOManager.resultData.iconsToFill[j]?.Count > 0)
                    reels[j].Remove(socketIOManager.resultData.iconsToFill[j]);
            }

        }



        yield return null;
    }

    IEnumerator GeneratePayline(List<int> paylinePoints)
    {

        GameObject line = Instantiate(linePrefab, payLineContainer);
        line.transform.localPosition = new Vector2(initialPosition.x, initialPosition.y);
        UILineRenderer payLine = line.GetComponent<UILineRenderer>();
        List<Vector2> paylinePoint = new List<Vector2>();
        Vector2 temp;
        for (int i = 0; i < paylinePoints.Count; i++)
        {
        payline_Controller.paylines[i].SetActive(true);
            reels[i].currentReelItems[paylinePoints[i]].transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
            if (paylinePoints[i] == 0)
                temp = new Vector2(i * distance.x, -2 * distance.y);
            else if (paylinePoints[i] == 2)
                temp = new Vector2(i * distance.x, 0 * -1 * distance.y);
            else
                temp = new Vector2(i * distance.x, paylinePoints[i] * -1 * distance.y);

            paylinePoint.Add(temp);
        }


        payLine.Points = paylinePoint.ToArray();

        yield return new WaitForSeconds(1f);

        Destroy(line);
        for (int i = 0; i < paylinePoints.Count; i++)
        {
            reels[i].currentReelItems[paylinePoints[i]].transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
        }

    }


    List<List<int>> CheckIfWild(List<int> iconsToRemove)
    {

        List<List<int>> result = new List<List<int>>();
        List<int> wildIconArray = new List<int>();
        List<int> iconsToShoot = new List<int>();
        List<List<string>> resultArray = Transpose(resultData);

        for (int j = 0; j < resultArray.Count; j++)
        {
            for (int i = 0; i < iconsToRemove.Count; i++)
            {
                if (resultArray[j][iconsToRemove[i]] == "13")
                {
                    wildIconArray.Add(iconsToRemove[i]);
                    continue;
                }
                else
                {
                    iconsToShoot.Add(iconsToRemove[i]);
                    continue;
                }
            }

        }


        result.Add(wildIconArray);
        result.Add(iconsToShoot);

        return result;
    }



    List<List<T>> Transpose<T>(List<List<T>> input)
    {
        if (input == null || input.Count == 0 || input[0].Count == 0)
            return new List<List<T>>();

        int rows = input.Count;
        int columns = input[0].Count;
        var transposed = new List<List<T>>(columns);

        for (int j = 0; j < columns; j++)
        {
            transposed.Add(new List<T>(rows));
        }

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                transposed[j].Add(input[i][j]);
            }
        }

        return transposed;
    }

    internal void populateSLot(int reelNo, List<int> icons)
    {

        //List<Sprite> sprites = new List<Sprite>();


        //for (int i = 0; i < reels.Length; i++)
        //{
        //for (int j = 0; j < initialData[i].Count; j++)
        //{
        //    sprites.Add(iconList[int.Parse(initialData[i][j])]);
        //}
        reels[reelNo].populateReel(icons);
        if (TotalBet_text) TotalBet_text.text = socketIOManager.initialData.bets[BetCounter].ToString();
        if (BetperLine_text) BetperLine_text.text = ((float)socketIOManager.initialData.bets[BetCounter] / 25f).ToString();
        //sprites.Clear();
        //}
    }

    void ToggleButtonGrp(bool toggle)
    {

        spin_button.interactable = toggle;
        auto_spin_button.interactable = toggle;
        paytable_button.interactable = toggle;
        bet_plus_button.interactable = toggle;
        bet_minus_button.interactable = toggle;
    }


    List<List<int>> TransposeMatrix(List<List<string>> matrix)
    {
        // Get the number of rows and columns
        int rowCount = matrix.Count;
        int colCount = matrix[0].Count;

        // Create the transposed matrix
        List<List<int>> transposedMatrix = new List<List<int>>();

        // Initialize the transposed matrix with empty lists
        for (int i = 0; i < colCount; i++)
        {
            transposedMatrix.Add(new List<int>());
        }

        // Populate the transposed matrix
        for (int i = 0; i < rowCount; i++)
        {
            for (int j = 0; j < colCount; j++)
            {
                transposedMatrix[j].Add(int.Parse(matrix[i][j]));
            }
        }

        return transposedMatrix;
    }

    public static List<int> ConvertListOfStringsToInts(List<string> stringList)
    {
        List<int> intList = new List<int>();

        foreach (string str in stringList)
        {
            if (int.TryParse(str, out int number))
            {
                intList.Add(number);
            }
            else
            {
                Debug.LogError($"Failed to convert '{str}' to int.");
            }
        }

        return intList;
    }

    void PrintMatrix(List<List<int>> matrix)
    {
        foreach (List<int> row in matrix)
        {
            string rowString = "";
            foreach (int element in row)
            {
                rowString += element + "\t";
            }
            Debug.Log(rowString);
        }
    }
}
