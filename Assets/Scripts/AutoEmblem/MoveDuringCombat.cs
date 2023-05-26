using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveDuringCombat : MonoBehaviour
{
    [SerializeField] private Vector3 moveBy;
    [SerializeField] private bool unhideOnNextLevel = true;
    private bool isHidden = false;

    private Vector3 startLoc;
    private Vector3 hideLoc;

    private void Awake()
    {
        Messenger.AddListener(GameEvent.COMBAT_STATE_CHANGED, OnCombatStateChanged);
        Messenger.AddListener(GameEvent.NEXT_LEVEL, OnNextLevel);
    }

    private void OnDestroy()
    {
        Messenger.RemoveListener(GameEvent.COMBAT_STATE_CHANGED, OnCombatStateChanged);
        Messenger.RemoveListener(GameEvent.NEXT_LEVEL, OnNextLevel);
    }

    private void Start()
    {
        startLoc = gameObject.transform.position;
        hideLoc = startLoc + moveBy;
    }

    private void OnCombatStateChanged()
    {
        if (!isHidden || !unhideOnNextLevel)
        {
            MoveObject();
        }
    }

    private void OnNextLevel()
    {
        if (isHidden && unhideOnNextLevel)
        {
            MoveObject();
        }
    }

    private void MoveObject()
    {
        if (!isHidden)
        {
            isHidden = true;
            iTween.MoveTo(gameObject, hideLoc, 1.0f);
        }
        else
        {
            isHidden = false;
            iTween.MoveTo(gameObject, startLoc, 1.0f);
        }
    }
}
