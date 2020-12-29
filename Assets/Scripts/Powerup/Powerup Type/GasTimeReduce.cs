using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GasTimeReduce : Powerup
{

    private PlayerCar player;

    private void Start()
    {
        player = GetComponentInParent<PlayerCar>();
    }

    private void Update()
    {
        if (isActive)
        {
            duration -= Time.deltaTime;

            if (duration <= 0)
                PowerEnd();
        }
    }

    public override void PowerStart(float _time)
    {
        if (!isActive)
            player.SetMaxHoldTime(player.defaultMaxHoldTime / 2);

        base.PowerStart(_time);
    }

    public override void PowerEnd()
    {
        base.PowerEnd();

        player.SetMaxHoldTime(player.defaultMaxHoldTime);
    }

}
