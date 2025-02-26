using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Payline_controller : MonoBehaviour
{
    [SerializeField] internal Image[] paylines;
    [SerializeField] internal Color[] paylineColor;
    void Start()
    {

    }

    internal Color GeneratePayline(int id)
    {


            paylines[id].gameObject.SetActive(true);
            paylines[id].color=paylineColor[id];
            return paylineColor[id];

    }

    internal void DestroyPayline(int id){
            paylines[id].gameObject.SetActive(false);

    }
    internal void ResetPayline()
    {
        for (int i = 0; i < paylines.Length; i++)
        {
            if (paylines[i].gameObject.activeSelf)
                paylines[i].gameObject.SetActive(false);
        }
    }
}
