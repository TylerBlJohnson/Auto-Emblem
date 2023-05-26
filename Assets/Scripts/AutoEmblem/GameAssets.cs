using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAssets : MonoBehaviour
{
    private static GameAssets instance;
    public AudioClip physical_damage;
    public AudioClip magical_damage;
    public AudioClip no_damage;
    public AudioClip level_up;
    public AudioClip not_allowed;
    public AudioClip stat_up;
    public AudioClip craw_snip;

    public static GameAssets Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Instantiate(Resources.Load("GameAssets") as GameObject).GetComponent<GameAssets>();
                DontDestroyOnLoad(Instance);
            }
            return instance;
        }
    }
}
