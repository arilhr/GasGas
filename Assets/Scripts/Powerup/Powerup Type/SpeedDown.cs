using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SpeedDown : Powerup
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
            player.SetMaxForceNow(player.defaultForce / 2);

        base.PowerStart(_time);
    }

    public override void PowerEnd()
    {
        base.PowerEnd();

        player.SetMaxForceNow(player.defaultForce);
    }
}
