using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameGenerator : MonoBehaviour
{
    [SerializeField] private string[] allyNames;
    [SerializeField] private string[] enemyNames;
    private int numAllyNamesUsed = 0;

    private void Awake()
    {
        Shuffle(ref allyNames);
    }

    private void Start()
    {
        //Shuffle(ref allyNames);
    }

    public string GetAllyName()
    {
        if (numAllyNamesUsed >= allyNames.Length)
        {
            Shuffle(ref allyNames);
            numAllyNamesUsed = 0;
        }

        string allyName = allyNames[numAllyNamesUsed];
        numAllyNamesUsed++;
        return allyName;
    }

    public string GetEnemyName()
    {
        int nameValue = Random.Range(0, enemyNames.Length);
        return enemyNames[nameValue];
    }

    private void Shuffle(ref string[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int randNum = Random.Range(i, array.Length);
            string temp = array[i];
            array[i] = array[randNum];
            array[randNum] = temp;
        }
    }
}
