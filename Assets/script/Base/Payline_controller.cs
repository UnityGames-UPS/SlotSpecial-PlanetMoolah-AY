using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Payline_controller : MonoBehaviour
{
    [SerializeField] internal Image[] paylines;
    [SerializeField] internal Color[] paylineColor;

    internal Color GeneratePayline(int id)
    {
            paylines[id].gameObject.SetActive(true);
            paylines[id].color=paylineColor[id];
            return paylineColor[id];

    }

    internal void DestroyPayline(int id){
            paylines[id].gameObject.SetActive(false);

    }
}
