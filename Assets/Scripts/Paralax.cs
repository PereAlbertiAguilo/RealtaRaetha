using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paralax : MonoBehaviour
{
    public Camera cam;
    public Transform player;
    public bool useY = true;

    Vector2 startPos;
    float startZ;

    Vector2 travel => (Vector2)cam.transform.position - startPos;

    float distFromPlayer => transform.position.z - player.position.z;

    float clippingPlane => (cam.transform.position.z + (distFromPlayer > 0 ? cam.farClipPlane : cam.nearClipPlane));

    float parallaxValue => Mathf.Abs(distFromPlayer) / clippingPlane;

    void Start()
    {
        cam = Camera.main;
        player = FindObjectOfType<PlayerController>().transform;

        startPos = transform.position;
        startZ = transform.position.z;
    }

    void Update()
    {
        Vector2 newPos = transform.position = startPos + travel * parallaxValue;
        transform.position = new Vector3(newPos.x, (useY ? newPos.y : transform.position.y), startZ);
    }
}
