using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollow : MonoBehaviour
{
    public Transform player;
    private float Speed=0.12f;
    public Vector3 offset;
    private float lookAheadFactor=1.8f;


    void FixedUpdate()
    {
        Vector3 wantPos = player.position + offset;

        float h = Input.GetAxis("Horizontal");
        if (h>0){
            wantPos.x +=lookAheadFactor;
        }
        else if(h<0){
            wantPos.x -=lookAheadFactor;
        }
        float v = Input.GetAxis("Vertical");
        if (v>0){
            wantPos.y +=lookAheadFactor;
        }
        else if(v<0){
            wantPos.y -=lookAheadFactor;
        }

        Vector3 finalPos= Vector3.Lerp(transform.position, wantPos,Speed);
        transform.position = new Vector3(finalPos.x, finalPos.y,transform.position.z);
    }
}
