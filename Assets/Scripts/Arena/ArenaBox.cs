using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaBox : MonoBehaviour
{
    private void OnTriggerExit(Collider other)
    {
        PlayerCar player = null;
        if (other.gameObject.GetComponentInParent<PlayerCar>() != null)
            player = other.gameObject.GetComponentInParent<PlayerCar>();

        player.RespawnPlayer();
    }
}
