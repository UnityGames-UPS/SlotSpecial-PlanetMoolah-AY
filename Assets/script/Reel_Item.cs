using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class Reel_Item : MonoBehaviour
{
    [SerializeField] internal Image image;
    [SerializeField] internal int id;
    [SerializeField] internal int pos;
    [SerializeField] internal Transform selfTransform;
    [SerializeField] internal ImageAnimation imageAnimation;
}
