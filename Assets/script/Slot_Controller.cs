using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

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
    //[SerializeField] private List<Reel_Item>


    [SerializeField] private Transform bullet;

    [Header("Buttons")]
    [SerializeField] private Button spin_button;
    [SerializeField] private Button auto_spin_button;
    [SerializeField] private Button AutoSpinStop_Button;
    [SerializeField] private Button paytable_button;

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

    [SerializeField] private UFO_controller ufoController;
    // Duration of the animation in seconds

    private void Start()
    {
        if (spin_button) spin_button.onClick.RemoveAllListeners();
        if (spin_button) spin_button.onClick.AddListener(StartSLot);

        if (auto_spin_button) auto_spin_button.onClick.RemoveAllListeners();
        if (auto_spin_button) auto_spin_button.onClick.AddListener(AutoSpin);

        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.RemoveAllListeners();
        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.AddListener(StopAutoSpin);

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



        for (int i = 0; i < resultData[0].Count; i++)
        {
            List<int> temp = new List<int>();
            for (int j = 0; j < resultData.Count; j++)
            {
                temp.Add(int.Parse(resultData[j][i]));

            }
            result.Add(temp);
        }

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
            item.ClearReel(Random.Range(0, 0.2f));

        }
        yield return new WaitForSeconds(1.5f);
        for (int i = 0; i < reels.Length; i++)
        {
            reels[i].FillReel(result[i]);
            yield return new WaitForSeconds(0.2f);
        }
        yield return new WaitForSeconds(0.2f);

        yield return GeneratePayline(paylinePoints);


        yield return StartCoroutine(CheckWinCoroutine());

        isSpinning = false;
        if (!isAutoSpin)
            ToggleButtonGrp(true);

    }




    IEnumerator CheckWinCoroutine()
    {
        for (int i = 0; i < iconsToRemove.Count; i++)
        {

            yield return ufoController.ShootLaser(iconsToRemove[i]);

            //yield return BlastAnimation(iconsToRemove[i]);

            yield return OnWIn(iconsToRemove[i], iconsToFill[i]);

            yield return new WaitForSeconds(1.3f);
        }
        yield return null;

    }
    //IEnumerator BlastAnimation(List<int> iconsToRemove)
    //{


    //    for (int i = 0; i < iconsToRemove.Count; i++)
    //    {
    //        reels[i].currentReelItems[iconsToRemove[i]].transform.GetChild(2).gameObject.SetActive(true);

    //    }

    //    yield return new WaitForSeconds(2);

    //    for (int i = 0; i < iconsToRemove.Count; i++)
    //    {
    //        reels[i].currentReelItems[iconsToRemove[i]].transform.GetChild(2).gameObject.SetActive(false);

    //    }

    //}

    IEnumerator OnWIn(List<int> iconsToRemove, List<int> iconsToFill)
    {

        for (int j = 0; j < iconsToRemove.Count; j++)
        {
            reels[j].Remove(new List<int> { iconsToRemove[j] }, new List<int> { iconsToFill[j] });
        }
        yield return null;
    }

    IEnumerator GeneratePayline(int[] paylinePoints)
    {

        GameObject line = Instantiate(linePrefab, payLineContainer);
        line.transform.localPosition = new Vector2(initialPosition.x, initialPosition.y);
        UILineRenderer payLine = line.GetComponent<UILineRenderer>();
        List<Vector2> paylinePoint = new List<Vector2>();
        Vector2 temp;
        for (int i = 0; i < paylinePoints.Length; i++)
        {
            reels[i].currentReelItems[paylinePoints[i]].transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
            if (paylinePoints[i] == 0)
            temp = new Vector2(i * distance.x, 2 * -1 * distance.y);
                
            else if (paylinePoints[i] == 2)
                temp = new Vector2(i * distance.x, 0 * -1 * distance.y);
            else
                temp = new Vector2(i * distance.x, paylinePoints[i] * -1 * distance.y);

            paylinePoint.Add(temp);
        }

     
        payLine.Points = paylinePoint.ToArray();

        yield return new WaitForSeconds(1f);

        Destroy(line);
        for (int i = 0; i < paylinePoints.Length; i++)
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

        // Initialize columns with empty lists
        for (int j = 0; j < columns; j++)
        {
            transposed.Add(new List<T>(rows));
        }

        // Fill transposed list
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                transposed[j].Add(input[i][j]);
            }
        }

        return transposed;
    }

    internal void populateSLot(int reelNo,List<int > icons)
    {
        SocketIOManager.PrintList(icons);

        //List<Sprite> sprites = new List<Sprite>();


        //for (int i = 0; i < reels.Length; i++)
        //{
        //for (int j = 0; j < initialData[i].Count; j++)
        //{
        //    sprites.Add(iconList[int.Parse(initialData[i][j])]);
        //}
        reels[reelNo].populateReel(icons);
            //sprites.Clear();
        //}
    }

    void ToggleButtonGrp(bool toggle)
    {

        spin_button.interactable = toggle;
        auto_spin_button.interactable = toggle;
        paytable_button.interactable = toggle;
    }



}
