using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TextWithBorder : MonoBehaviour
{
    [SerializeField] private TextMesh[] texts;
    private string curValue;

    private void Update()
    {
        if (curValue != texts[0].text)
        {
            for (int i = 1; i < texts.Length; i++)
            {
                texts[i].text = texts[0].text;
            }
            curValue = texts[0].text;
        }
    }

    public void UpdateText(string value)
    {
        texts[0].text = value;
    }
}
