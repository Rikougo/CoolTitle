using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxItem : MonoBehaviour
{
    public Transform m_camera;
    public float relativeMoveX = .3f;
    public float relativeMoveY = .1f;

    void Update()
    {
        transform.position = new Vector2((m_camera.position.x * relativeMoveX) * 0.2f, transform.position.y);
    }
}