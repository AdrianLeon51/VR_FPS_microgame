using Meta.WitAi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockRooms : MonoBehaviour
{
    public GameObject previousWall;
    private void OnTriggerExit(Collider other)
    {
        previousWall.SetActive(true);
        this.gameObject.DestroySafely();
    }
}
