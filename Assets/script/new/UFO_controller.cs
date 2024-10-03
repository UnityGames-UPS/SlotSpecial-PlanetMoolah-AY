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
    [SerializeField] private Transform[] ufoList;

    [SerializeField] private Transform pullBeamParent;
    [SerializeField] Vector2 initialPos;

    [SerializeField] float xDitance;
    [SerializeField] float yDistance;

    [SerializeField] internal float shootSpeed=0.3f;

    internal void Shoot(int[] values)
    {


            int yPos=values[0];
            int xPos=values[1];
            int ufoIndex=Helper.GetRandomIndexExcept(ufoList.Length,yPos);
            GameObject projectile = Instantiate(laserBullet, ufoList[ufoIndex].parent);
            projectile.transform.localPosition = ufoList[ufoIndex].localPosition;
            Vector2 ultDest = new Vector2(initialPos.x + (yPos * xDitance), -(xPos * yDistance - initialPos.y));
            projectile.transform.DOLocalMove(ultDest, shootSpeed).SetEase(Ease.Linear).OnComplete(() =>
            {
                Destroy(projectile);
            });
        

    }

    internal ImageAnimation Pull(int[] values){

        float yScale=1;

        if(values[1]==0)
        yScale=1;
        else if(values[1]==1)
        yScale=1.75f;
        else if(values[1]==2)
        yScale=2.5f;

        GameObject beam = Instantiate(pullPrefab, pullBeamParent);
        beam.transform.localPosition = ufoList[values[0]].localPosition;
        beam.transform.localScale=new Vector3(1,yScale,1);

        ImageAnimation pull=beam.GetComponent<ImageAnimation>();
        pull.StartAnimation();
        return pull;

    }
}
