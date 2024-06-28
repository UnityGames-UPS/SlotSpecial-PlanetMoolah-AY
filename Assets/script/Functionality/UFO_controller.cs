using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class UFO_controller : MonoBehaviour
{
    //[SerializeField] private Transform[] ufo_list;
    [SerializeField] private ImageAnimation[] ufo_list;
    [SerializeField] private UILineRenderer[] laser_list;
    [SerializeField] private GameObject blastAnimation;
    [SerializeField] private Transform mainSlot;
    [SerializeField] private List<GameObject> tempBlastObject;
    [SerializeField] private Slot_Controller slot_Controller;
    [SerializeField] private Sprite pull_prite;
    [SerializeField] private Tweener[] tweeners = new Tweener[5];
    public float duration = 2.0f;
    public Vector2 distance;

    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private GameObject laser_bullet;
    [SerializeField] private GameObject pullImage;
    void Start()
    {


        for (int i = 0; i < ufo_list.Length; i++)
        {
            if (i % 2 == 0)
                tweeners[i] = StartOscillation(ufo_list[i].transform.parent, -20f);
            else
                tweeners[i] = StartOscillation(ufo_list[i].transform.parent, 20f);
        }
    }



    //internal IEnumerator ShootLaser(List<int> iconToShoot)
    //{

    //    //for (int i = 0; i < iconToShoot.Count; i++)
    //    //{

    //    //    StartCoroutine(LaserCoroutine(i, i, iconToShoot[i]));

    //    //    print(iconToShoot[i]);
    //    //}

    //    yield return StartCoroutine(LaserCoroutine(iconToShoot));

    //    //yield return new WaitForSeconds(0.8f);
    //    //yield return StartCoroutine(BlastAnimation(iconToShoot));

    //    //yield return new WaitForSeconds(1);
    //}



    internal IEnumerator LaserCoroutine(List<int> iconToShoot = null)
    {
        //0: 0,-4
        //2: 2,-2
        //3: 3,-1
        //4: 4,0

        SocketIOManager.PrintList(iconToShoot);
        bool pull=false;
        List<GameObject> laserList = new List<GameObject>();
        List<Reel_Item> iconPull = new List<Reel_Item>();
        List<Reel_Item> iconShoot = new List<Reel_Item>();
        List<GameObject> pullList = new List<GameObject>();
        for (int i = 0; i < iconToShoot.Count; i++)
        {
            //pull = false;
            int posX = iconToShoot[i] >= 10 ? (iconToShoot[i] / 10) % 10 : 0;
            int posY = ( (iconToShoot[i] >= 10 ? (iconToShoot[i] % 10) : iconToShoot[i]));
            Vector2[] point;

            int ufoRandomIndex = GetRandomUfoWithrange(posX);
            point = new Vector2[] { Vector2.zero, Vector2.zero };

            //for (int j = 0; j < slot_Controller.reels[posX].currentReelItems.Count; j++)
            //{
            //    if (slot_Controller.reels[posX].currentReelItems[j]?.pos == posY)
            //    {
            //        Reel_Item item = slot_Controller.reels[posX].currentReelItems[j];
            //        slot_Controller.reels[posX].poolReelItems.Add(item);
            //        slot_Controller.reels[posX].currentReelItems[j] = null;

            //        if (item.id == 13) {
            //        pull = true;
            //            ufoRandomIndex = posX;
            //        }
            //        GameObject laserObject = Instantiate(laserPrefab, ufo_list[ufoRandomIndex].transform.parent);
            //        UILineRenderer laser = laserObject.GetComponent<UILineRenderer>();
            //        laserObject.SetActive(false);
            //        tweeners[ufoRandomIndex].Pause();


            //        item.transform.parent = ufo_list[ufoRandomIndex].transform.parent.transform;
            //        point[1] = new Vector2(item.transform.localPosition.x, item.transform.localPosition.y);
            //        item.transform.parent = slot_Controller.reels[posX].transform;
            //        if (pull)
            //            iconPull.Add(item.gameObject);
            //        else
            //            iconShoot.Add(item.gameObject);
            //        if (pull)
            //        {
            //            point[1].y += 50;
            //            laser.lineThickness = 250;
            //            laser.sprite = pull_prite;
            //            pullList.Add(laser);
            //            laser.material = null;
            //            laser.color = Color.white;

            //        }
            //        else
            //        {

            //            laser.lineThickness = 36;
            //            laserList.Add(laser);

            //        }

            //        laser.Points = point;

            //        break;
            //    }
            //}

            for (int j = 0; j < slot_Controller.reels[posX].currentReelItems.Count; j++)
            {
                if (slot_Controller.reels[posX].currentReelItems[j] !=null && slot_Controller.reels[posX].currentReelItems[j]?.pos == posY )
                {
                    Reel_Item item = slot_Controller.reels[posX].currentReelItems[j];
                    item.id = slot_Controller.reels[posX].currentReelItems[j].id;
                    slot_Controller.reels[posX].poolReelItems.Add(item);

                    if (item.id == 13)
                    {
                        pull = true;
                        ufoRandomIndex = posX;
                    print("id" + slot_Controller.reels[posX].currentReelItems[j].id);
                    }
                    item.transform.parent = ufo_list[ufoRandomIndex].transform.parent.transform;
                    point[1] = new Vector2(item.transform.localPosition.x, item.transform.localPosition.y);
                    item.transform.parent = slot_Controller.reels[posX].transform;


                    Debug.Log("pull" + pull);
                    if (pull)
                    {
                        GameObject pullobject = Instantiate(pullImage, ufo_list[ufoRandomIndex].transform.parent);
                        pullobject.SetActive(false);
                        Image img = pullobject.GetComponent<Image>();
                        if (posY == 0)
                            img.rectTransform.sizeDelta = new Vector2(342, 650);
                        else if (posY == 1)
                            img.rectTransform.sizeDelta = new Vector2(342, 500);
                        else if (posY == 2)
                            img.rectTransform.sizeDelta = new Vector2(342, 200);

                        pullobject.transform.localPosition = new Vector2(0, -35); ;
                        iconPull.Add(item);
                        //point[1].y += 50;
                        pullList.Add(pullobject);

                    }
                    else
                    {

                        GameObject laserObject = Instantiate(laser_bullet, ufo_list[ufoRandomIndex].transform.parent);
                        laserObject.transform.localPosition = Vector2.zero;
                        iconShoot.Add(item);
                        laserObject.transform.DOLocalMove(point[1], 0.5f).onComplete=()=>laserObject.SetActive(false);
                        laserList.Add(laserObject);
                    }

                    tweeners[ufoRandomIndex].Pause();

                    slot_Controller.reels[posX].currentReelItems[j] = null;
                    break;
                }
            }


        }

        yield return new WaitForSeconds(0.3f);
        if (iconPull.Count>0)
        StartCoroutine( PullCoroutine(pullList.ToArray(),iconPull.ToArray()));

        yield return ShootCoroutine(laserList.ToArray(), iconShoot.ToArray());


        foreach (var item in tweeners)
        {
            item.Play();
        }

        yield return null;

        
    }

    IEnumerator PullCoroutine(GameObject[] pullList,Reel_Item[] iconPull) {

        foreach (var item in pullList)
        {
            item.gameObject.SetActive(true);
            
        }
        print("icon to pull" + iconPull);
        foreach (var item in iconPull)
        {
            //item.transform.GetChild(1).gameObject.SetActive(true);
            item.imageAnimation.AnimationSpeed = 60;
            item.imageAnimation.gameObject.SetActive(true);
            item.imageAnimation.StartAnimation();
        }
        yield return new WaitForSeconds(1.5f);
        for (int i = 0; i < iconPull.Length; i++)
        {
            iconPull[i].imageAnimation.StopAnimation();
            iconPull[i].transform.GetChild(1).gameObject.SetActive(false);
            iconPull[i].gameObject.SetActive(false);
        }
        yield return new WaitForSeconds(0.8f);
        foreach (var item in pullList)
        {
            Destroy(item.gameObject);

        }

    }

    IEnumerator ShootCoroutine(GameObject[] laserList, Reel_Item[] iconShoot) {
        yield return new WaitForSeconds(0.8f);

        foreach (var item in laserList)
        {
            Destroy(item.gameObject);

        }
        foreach (var item in iconShoot)
        {
            item.imageAnimation.AnimationSpeed = 10;
            item.imageAnimation.gameObject.SetActive(true);
            item.imageAnimation.StartAnimation();
            item.blastTransform.offsetMin = Vector2.one *-100;
            item.blastTransform.offsetMax = Vector2.one * 100;
            item.transform.GetChild(0).gameObject.SetActive(false);
        }
        yield return new WaitForSeconds(1.5f);

        for (int i = 0; i < iconShoot.Length; i++)
        {
            iconShoot[i].imageAnimation.StopAnimation() ;
            iconShoot[i].imageAnimation.gameObject.SetActive(false);
            iconShoot[i].gameObject.SetActive(false);
            iconShoot[i].transform.GetChild(0).gameObject.SetActive(true);
            iconShoot[i].blastTransform.offsetMin = Vector2.zero;
            iconShoot[i].blastTransform.offsetMax = Vector2.zero;
        }


    }

    Tweener StartOscillation(Transform ufo, float pos)
    {

        return ufo.DOLocalMoveY(ufo.localPosition.y + pos, 1.5f)
        .SetEase(Ease.InOutSine)
        .SetLoops(-1, LoopType.Yoyo);
    }

    int GetRandomUfoWithrange(int posX)
    {
        int[] range = new int[2];
        if (posX == 0)
        {

            range[0] = 1;
            range[1] = 2;
        }
        else if (posX == 1)
        {

            range[0] = 0;
            range[1] = 2;
        }
        else if (posX == 2)
        {

            range[0] = 1;
            range[1] = 3;
        }
        else if (posX == 3)
        {

            range[0] = 2;
            range[1] = 4;
        }
        else if (posX == 4)
        {

            range[0] = 2;
            range[1] = 3;
        }

        return range[Random.Range(0, range.Length)];
    }


}
