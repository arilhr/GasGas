using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public abstract class Powerup : NetworkBehaviour, IPowerup
{

    protected float duration;

    protected bool isActive = false;

    [SerializeField] protected GameObject panelEffect = null;

    [Client]
    public virtual void PowerStart(float _time)
    {
        Debug.Log("Power Start");

        isActive = true;

        duration = _time;

        panelEffect.SetActive(true);
    }

    [Client]
    public virtual void PowerEnd()
    {
        Debug.Log("Power End");

        duration = 0;

        isActive = false;

        panelEffect.SetActive(false);
    }
}
