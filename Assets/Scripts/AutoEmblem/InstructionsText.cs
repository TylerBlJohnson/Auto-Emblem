using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InstructionsText : MonoBehaviour
{
    [SerializeField] private GameObject[] pages;
    private int curPage = 0;

    private void Start()
    {
        if (pages != null)
        {
            for (int i = 0; i < pages.Length; i++)
            {
                pages[i].SetActive(false);
            }
            pages[curPage].SetActive(true);
        }
    }
    public void NextButton()
    {
        //Debug.Log("Next button clicked");
        if (pages != null)
        {
            pages[curPage].SetActive(false);
            curPage++;
            if (curPage >= pages.Length)
            {
                curPage = 0;
            }
            pages[curPage].SetActive(true);
        }
    }
}