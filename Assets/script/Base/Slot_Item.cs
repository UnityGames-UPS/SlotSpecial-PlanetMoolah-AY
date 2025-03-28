using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[Serializable]
public class Slot_Item : MonoBehaviour
{
    [SerializeField] internal Image image;
    [SerializeField] internal int id;
    [SerializeField] internal ImageAnimation ownAnim;
    [SerializeField] internal ImageAnimation blastAnim;
    [SerializeField] internal Image boder;
    [SerializeField] internal int wildVariation=-1;
}
