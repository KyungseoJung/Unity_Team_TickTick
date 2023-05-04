using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapCamera : MonoBehaviour  //#11-5 
{
    Transform playerTransform;
    private float zoomInDist = 21f; //7.3f;
    private float zoomOutDist = 33f;    //23f;
    public float minimapCameraDist;
    Vector3 minimapCameraPos;

    public csPhotonGame csPG;

    void Start()
    {
        // 처음 시작할 때 Rotation의 x는 90으로
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        minimapCameraDist = zoomOutDist; 
    }
    
    void LateUpdate()
    {
        if (csPG.myPlyerCtrl == null)
        {
            return;
        }
        else if(csPG.myPlyerCtrl != null && playerTransform == null)
        {
            playerTransform = csPG.myPlyerCtrl.transform;
        }

        if(playerTransform != null)
        {
            minimapCameraPos = playerTransform.position;
            minimapCameraPos.y = minimapCameraDist;
        }
        // 계속 플레이어 머리 위에 따라다니도록
        transform.position = minimapCameraPos;
    }

    public void ZoomInCamera()
    {
        minimapCameraDist = zoomInDist;
    }
    
    public void ZoomOutCamera()
    {
        minimapCameraDist = zoomOutDist;
    }



}
