using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PushToFrontSortingLayer : MonoBehaviour
{
    [SerializeField] private string layerToPushTo;

    void Start()
    {
        GetComponent<Renderer>().sortingLayerName = layerToPushTo;
        //Debug.Log(GetComponent<Renderer>().sortingLayerName);
    }
}
