using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Color[] backgrounds;

    private void Awake()
    {
        Messenger.AddListener(GameEvent.NEXT_LEVEL, OnNextLevel);
    }

    private void OnDestroy()
    {
        Messenger.RemoveListener(GameEvent.NEXT_LEVEL, OnNextLevel);
    }

    private void OnNextLevel()
    {
        ChooseNewBackground();
    }

    private void ChooseNewBackground()
    {
        Color bg = backgrounds[0]; //Gives an error if no default value
        for (int i = 0; i < 100; i++) //Try 100 times to get a background that isn't the current background
        {
            bg = backgrounds[Random.Range(0, backgrounds.Length)];
            if (spriteRenderer.color != bg)
            {
                break;
            }
        } 

        spriteRenderer.color = bg;
    }
}
