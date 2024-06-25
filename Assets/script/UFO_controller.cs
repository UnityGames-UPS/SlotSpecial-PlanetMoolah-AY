using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
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
    void Start()
    {


        for (int i = 0; i < ufo_list.Length; i++)
        {
            if (i % 2 == 0)
                tweeners[i]=StartOscillation(ufo_list[i].transform.parent, -20f);
            else
                tweeners[i]=StartOscillation(ufo_list[i].transform.parent, 20f);
        }
    }



    internal IEnumerator ShootLaser(List<int> iconToShoot)
    {

        //for (int i = 0; i < iconToShoot.Count; i++)
        //{

        //    StartCoroutine(LaserCoroutine(i, i, iconToShoot[i]));

        //    print(iconToShoot[i]);
        //}
        yield return StartCoroutine(LaserCoroutine(0, 0,0, iconToShoot));

        //yield return new WaitForSeconds(0.8f);
        //yield return StartCoroutine(BlastAnimation(iconToShoot));

        //yield return new WaitForSeconds(1);
    }

    IEnumerator BlastAnimation(List<int> iconsToRemove)
    {


        for (int i = 0; i < iconsToRemove.Count; i++)
        {
            slot_Controller.reels[i].currentReelItems[iconsToRemove[i]].transform.GetChild(1).gameObject.SetActive(true);

        }

        yield return new WaitForSeconds(2);

        for (int i = 0; i < iconsToRemove.Count; i++)
        {
            slot_Controller.reels[i].currentReelItems[iconsToRemove[i]].transform.GetChild(1).gameObject.SetActive(false);
            slot_Controller.reels[i].currentReelItems[iconsToRemove[i]].gameObject.SetActive(false);

            tweeners[i].Play();
        }


    }

    IEnumerator LaserCoroutine(int index, int posX = 0, int posY = 0, List<int> iconToShoot=null)
    {
        //0: 0,-4
        //2: 2,-2
        //3: 3,-1
        //4: 4,0
       

        bool pull;
        for (int i = 0; i < iconToShoot.Count; i++)
        {
            pull = false;
            posY = iconToShoot[i];
            //float elapsedTime = 0.0f;
            //int col_no = -index + posX;
            int col_no = -i+i;

            int row_no = 2 - posY;
            tweeners[i].Pause();
            Vector2[] point;

            //point = new Vector2[] { Vector2.zero, new Vector2(col_no * distance.x, -row_no * distance.y - distance.y) };
            point = new Vector2[] { Vector2.zero, Vector2.zero };
            foreach (var item in slot_Controller.reels[i].currentReelItems)
            {
                if (item.pos == posY)
                {
                    item.transform.parent = ufo_list[i].transform.parent.transform;
                    
                    if (item.id == 13) pull = true;
                    if (!pull) item.transform.SetAsFirstSibling();
                    if(pull) point[1] = new Vector2(0, item.transform.localPosition.y);
                    else point[1] = new Vector2(0, item.transform.localPosition.y+50);
                }
            }

            //ufo_list[i].StartAnimation();
            //yield return new WaitUntil(() => !ufo_list[i].isplaying);
            //ufo_list[i].StopAnimation();

            if (pull)
            {

                laser_list[i].lineThickness = 250;
                laser_list[i].sprite = pull_prite;
            }
            else
                laser_list[i].lineThickness = 30;


            Vector2[] positions = new Vector2[2];
            positions[0] = point[0];
            positions[1] = point[1];


            laser_list[i].Points = point;
            slot_Controller.reels[i].currentReelItems[iconToShoot[i]].transform.GetChild(1).gameObject.SetActive(true);
            //yield return new WaitForSeconds(1);
            //laser_list[i].Points = new Vector2[] { Vector2.zero };
        }
        yield return new WaitForSeconds(1.5f);
        for (int i = 0; i < iconToShoot.Count; i++)
        {
            slot_Controller.reels[i].currentReelItems[iconToShoot[i]].transform.GetChild(1).gameObject.SetActive(false);
            slot_Controller.reels[i].currentReelItems[iconToShoot[i]].gameObject.SetActive(false);

            tweeners[i].Play();
        }

        foreach (var item in laser_list)
        {
            item.Points = new Vector2[] { Vector2.zero };
        }

        //while (elapsedTime < duration)
        //{
        //    elapsedTime += Time.deltaTime; // Increment time based on frame time
        //    float t = Mathf.Clamp01(elapsedTime / duration);
        //    Vector2 currentPosition = Vector2.Lerp(point[0], point[1], t);


        //Vector2[] positions = new Vector2[2];
        //positions[0] = point[0];
        //positions[1] = point[1];

        //    laser_list[index].Points = positions;
        //    yield return null;
        //}


    }



    Tweener StartOscillation(Transform ufo, float pos)
    {

        return ufo.DOLocalMoveY(ufo.localPosition.y + pos, 1.5f)
        .SetEase(Ease.InOutSine)
        .SetLoops(-1, LoopType.Yoyo);



    }
}
