using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class UFO_controller : MonoBehaviour
{

    [SerializeField] private GameObject laserBullet;
    [SerializeField] private GameObject pullPrefab;
    [SerializeField] private ImageAnimation[] ufoList;
    [SerializeField] private Transform pullBeamParent;
    [SerializeField] Vector2 initialPos;
    [SerializeField] float xDitance;
    [SerializeField] float yDistance;

    [SerializeField] internal float shootSpeed = 0.3f;

    internal void Shoot(int[] values)
    {


        StopUfoVerticalMove();
        // int ufoIndex = Helper.GetRandomIndexExcept(ufoList.Length, yPos);
        GameObject projectile = Instantiate(laserBullet, ufoList[values[2]].transform.parent.parent);
        projectile.transform.localPosition = new Vector3(ufoList[values[2]].transform.parent.localPosition.x, -65);
        Vector2 ultDest = new Vector2(initialPos.x + (values[0] * xDitance), -(values[1] * yDistance - initialPos.y));
        projectile.transform.DOLocalMove(ultDest, shootSpeed).SetEase(Ease.Linear).OnComplete(() =>
        {
            Destroy(projectile);
        }).SetEase(Ease.InOutExpo);


    }

    internal int[] uFoAnimation(int[] values)
    {
        int yPos = values[0];
        int xPos = values[1];
        int ufoIndex = Helper.GetRandomIndexExcept(ufoList.Length, yPos);
        ufoList[ufoIndex].StartAnimation();
        return new int[] { yPos, xPos, ufoIndex };
    }

    internal void StopUfoAnimation()
    {

        for (int i = 0; i < ufoList.Length; i++)
        {
            ufoList[i].StopAnimation();
        }
    }
    internal ImageAnimation Pull(int[] values)
    {
        StopUfoVerticalMove();

        float yScale = 1;

        if (values[1] == 0)
            yScale = 1;
        else if (values[1] == 1)
            yScale = 1.75f;
        else if (values[1] == 2)
            yScale = 2.5f;

        GameObject beam = Instantiate(pullPrefab, pullBeamParent);
        beam.transform.localPosition = ufoList[values[0]].transform.parent.localPosition;
        beam.transform.localScale = new Vector3(1, yScale, 1);

        ImageAnimation pull = beam.GetComponent<ImageAnimation>();
        pull.StartAnimation();
        return pull;

    }

    internal void StartUfoVerticalMove()
    {

        for (int i = 0; i < ufoList.Length; i++)
        {
            if (i % 2 == 0){
                ufoList[i].transform.parent.localPosition= new Vector2(ufoList[i].transform.parent.localPosition.x,25);
                ufoList[i].transform.parent.DOLocalMoveY(-25, 2.5f).SetLoops(-1,LoopType.Yoyo).SetDelay(0);
            }
            else{
                ufoList[i].transform.parent.localPosition= new Vector2(ufoList[i].transform.parent.localPosition.x,-25);
                ufoList[i].transform.parent.DOLocalMoveY(25, 2.5f).SetLoops(-1,LoopType.Yoyo).SetDelay(0);
            }

        }
    }

    internal void StopUfoVerticalMove()
    {

        for (int i = 0; i < ufoList.Length; i++)
        {
            DOTween.Kill( ufoList[i].transform.parent);
            ufoList[i].transform.parent.localPosition=new Vector2( ufoList[i].transform.parent.localPosition.x, 0);

        }
    }
}
