using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(Vector3.up, 60f * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        // if (other.tag == "Player")
        // {
        //     Destroy(gameObject);
        // }
    }
}
