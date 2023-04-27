﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
// using UnityEngine.Tilemaps;

public enum MAP_TYPE {GROUND = 1, TREE, PLAYER, /*FOOD,*/ GOAL};

public static class MiniMapConstants
{
    // 맵의 폭, 길이 
    public const int MAP_WIDTH = 64;    //갈 수 있는 거리는 0~63.9까지니까..? //100; //40;    //#11-3 더 멀~리 찍히도록 *10씩
    public const int MAP_HEIGHT = 64;   //  100;  //20;
    public const int MINIMAP_WIDTH = 300;   //64 *3;  //1600;  //280;   //그림 약간 튀어나와서 300-20
    public const int MINIMAP_HEIGHT = 300;  //64 *3;  //900;  //180; //그림이 약간 튀어나오길래.. 200-20

    public const int SIZEUP = 5;   
}

public class MiniMap : MonoBehaviour    //@16 미니맵
{
    public MAP_TYPE mapType;    //어떤 오브젝트를 미니맵으로 그릴 거냐
    string enumObjectName;      // 자식으로 새롭게 만들 오브젝트의 이름
// @16-2    땅, 벽 이미지 정상적으로 만들기 위해 - 오브젝트 크기 자체를 줄일 생각
    RectTransform minimapRectT;


// Layer 이름
    // private const string GROUND_LAYER_NAME = "Ground";
    // private const string WALL_LAYER_NAME = "Wall";

//@16-2 미니맵에 그릴 타일맵 오브젝트 연결 - 인스펙터창 =============================
    // public Tilemap groundTilemap;
    // public Tilemap wallTilemap;
    
    private List<Vector3> groundPositions;    // 땅 Tilemap 위치를 담을 리스트
    private List<Vector3> treePositions;     // 벽 Tilemap 위치를 담을 리스트

//@16-3 미니맵에 그릴 오브젝트 - 플레이어, Foods, Goal 연결
    // private List<Vector3> playerPositions;  //포톤 연결 후에도 추가될 수 있도록 로직 작성 필요  //@ 플레이어 움직임에 따라 이동하는 건 PlayerFollow 클래스에서 해보자
    // private List<Vector3> goalPositions;
    


// 미니맵 상에 표시할 스프라이트 이미지 모음
    public Sprite groundSpriteImg;
    public Sprite treeSpriteImg;
    // public Sprite playerSpriteImg;
    // public Sprite goalSpriteImg;

// 미니맵을 표시할 RawImage 오브젝트 - 나중에 오브젝트에 연결?
    private RawImage minimapRawImage; //여기에 미니맵이 그려질 거야  //테스트용 public 선언

//@16-3 Start 반복문용
Vector3 tilePosition;

List<Vector3> positions;


//@16-3 MakeMinimap 반복문용
Texture2D minimapTexture;

Sprite spriteToUse;

Vector2 positionOnRawImage;
GameObject minimapObject;
Image minimapImage;

//#11-1 플레이어 좌표 기준으로 미니맵 통째로 움직이기
public Transform playerTransform;   //테스트용 public 잠깐만



    void Awake()    
    {
        minimapRawImage = GameObject.Find("miniMapRawImage").GetComponent<RawImage>();   //scPlayUi에 있는 미니맵 연결하기
        playerTransform = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();

    }

    void Start()
    {
        //#11-3 미니맵 박스 위치 맞춰주기
        minimapRawImage.rectTransform.anchoredPosition = new Vector2(-150f, -150f);  //#11-3 미니맵 중심을 좌측 하단으로 맞춰주기
        minimapRawImage.rectTransform.sizeDelta = new Vector2(600*MiniMapConstants.SIZEUP, 600*MiniMapConstants.SIZEUP);    //#11-3 위치 및 크기 맞춰주기 위함 (캔버스 상에서 부모 크기가 300임)
                        //거기에 중심을 좌측 하단으로 했으므로 크기 *2, 더 크게 보기 위해 *10씩 -> 300 * 2 * 10
/*
//@16-1 Ground, Wall 좌표 가져와서 List에 넣기 - 하나하나씩 그리기 위한 용도
        // List<Vector3> groundPositions = GetPositionsOnLayer(GROUND_LAYER_NAME);
        // List<Vector3> wallPositions = GetPositionsOnLayer(WALL_LAYER_NAME);    
//미니맵으로 그릴 좌표들을 List에 추가 - 한번에 그리기 위한 용도
        // List<Vector3> minimapPositions = new List<Vector3>();
        // minimapPositions.AddRange(groundPositions);
        // minimapPositions.AddRange(wallPositions);
// positions 리스트에 저장된 좌표 이용해서 UI이미지 생성
*/

// @16-3 정의
    groundPositions = new List<Vector3>();
    treePositions = new List<Vector3>();
    // playerPositions = new List<Vector3>();
    // goalPositions = new List<Vector3>();

/*
//@16-2 Tilemap 클래스 이용하기 =============================
    //땅 타일맵의 모든 타일 위치를 가져와서 리스트에 추가
    foreach(Vector3Int position in groundTilemap.cellBounds.allPositionsWithin)
    {
        if(groundTilemap.HasTile(position))
        {
            tilePosition = groundTilemap.CellToWorld(position) + new Vector3(0.5f, 0.5f, 0);    
                    //타일 중앙 좌표 계산을 위해서(좌측 상단보다는 중심점 가져와서 계산하는 게 좋대 : (0f, 0f, 0f)
            groundPositions.Add(tilePosition);
        }
    }

    //벽 타일맵의 모든 타일 위치를 가져와서 리스트에 추가
    foreach(Vector3Int position in wallTilemap.cellBounds.allPositionsWithin)
    {
        if(wallTilemap.HasTile(position))
        {
            tilePosition = wallTilemap.CellToWorld(position) + new Vector3(0.5f, 0.5f, 0);
            wallPositions.Add(tilePosition);
        }
    }
*/

//@16-3 플레이어, Foods, Goals 지점들 리스트 가져오기
    groundPositions = GetPositionsOnTag("Block");   //("Player");
    treePositions = GetPositionsOnTag("Tree");  //("Goals");

//@16-3 특정 태그를 통해 위치 가져오기

    // 땅, 벽 타일맵의 위치 리스트를 미니맵에 그려주기
//#11-3    MakeMinimap(groundPositions, MAP_TYPE.GROUND);
    MakeMinimap(treePositions, MAP_TYPE.TREE);
    // MakeMinimap(playerPositions, MAP_TYPE.PLAYER);
    // MakeMinimap(goalPositions, MAP_TYPE.GOAL);
    }
    
    void FixedUpdate()  //#11-1 Update보다 FixedUpdate가 맞으려나? 카메라 위치 가져오는 것도 FixedUpdate에서 했었으니까 같은 맥락일 듯..?
    {
        minimapRawImage.rectTransform.anchoredPosition = new Vector2(/*-70f(미세조정용) */ -1500/64 * playerTransform.position.x,   //(-150f(중점좌표 중 X)+75f(중심에 맞추기 위해)) -((MINIMAP_WIDTH * SIZEUP)/MiniMapConstants.MAP_WIDTH) *  (6000/2 - 300)
                                                                     /*-70f(미세조정용) */ -1500/64 * playerTransform.position.z);  // (-150f(중점좌표 중 Y)+75f(중심에 맞추기 위해)) -((MINIMAP_WIDTH * SIZEUP)/MiniMapConstants.MAP_HEIGHT) * 6000/2 - 300
    }


//태그를 이용해 특정 오브젝트의 위치 가져오기
    private List<Vector3> GetPositionsOnTag(string tagName)
    {
        positions = new List<Vector3>();    //@ 리스트 초기화 및 positions 변수가 객체를 참조하도록 하는 코드
        GameObject[] objects = GameObject.FindGameObjectsWithTag(tagName);
        foreach(GameObject obj in objects)
        {
            positions.Add(obj.transform.position);
        }

        return positions;
    }
/*
    private List<Vector3> GetPositionsOnLayer(string layerName) // Transform 자체로 가져오기 - 특정 레이어의 좌표들을 리스트에 넣어 return
    {
        List<Vector3> positions = new List<Vector3>();
        Transform[] objects = FindObjectsOfType<Transform>();
        foreach(Transform obj in objects)
        {
            if(obj.gameObject.layer == LayerMask.NameToLayer(layerName))
            positions.Add(obj.position);
        }
        return positions;
    }
*/
/*
    private List<Vector3> GetPositionsOnLayer(string layerName) // GameObject로 가져오기 - 특정 레이어의 좌표들을 리스트에 넣어 return
    {
        List<Vector3> positions = new List<Vector3>();
        GameObject[] objects = GameObject.FindObjectsOfType<GameObject>();
        foreach(GameObject obj in objects)
        {
            if(obj.layer == LayerMask.NameToLayer(layerName))
            positions.Add(obj.transform.position);
        }
        return positions;
    }
*/


    // //@ 나중에 팀플에서 사용 - 레이캐스트를 통해 (충돌한 특정 Layer 오브젝트만) 가져오기 때문에 효율적
    // private List<Vector3> GetPositionsOnLayer(string layerName)
    // {
    //     List<Vector3> positions = new List<Vector3>();
    //     RaycastHit[] hits = Physics.RaycastAll(Vector3.zero, Vector3.forward, Mathf.Infinity, LayerMask.GetMask(layerName));
    //     foreach(RaycastHit hit in hits)
    //     {
    //         positions.Add(hit.transform.position);
    //     }
    //     return positions;
    // }




    private void MakeMinimap(List<Vector3> positions, MAP_TYPE _mapType)
    {
        // 미니맵 이미지 생성 - 크기 맞춰서
        minimapTexture = new Texture2D(MiniMapConstants.MINIMAP_WIDTH * MiniMapConstants.SIZEUP, MiniMapConstants.MINIMAP_HEIGHT * MiniMapConstants.SIZEUP);

        //스프라이트 이미지 생성
        spriteToUse = null;  //typeofImage에 따라 사용할 이미지 딱 1개 정해두기 - swtich 문 계속 탈 필요 없이
        switch(_mapType)
            {
                

                case MAP_TYPE.GROUND :
                    spriteToUse = groundSpriteImg;
                    enumObjectName = "GroundMini";
                    break;
                case MAP_TYPE.TREE : 
                    spriteToUse = treeSpriteImg;
                    enumObjectName = "TreeMini";
                    break;
            }

        foreach(Vector3 position in positions)  //하나하나 position 값을 가져와서 자식 만들고, 이미지 넣어주기, 캔버스 내 위치로 잡아주기
        {
            // 월드좌표를 RawImage 상에서의 좌표로 변환 표현 
            positionOnRawImage = new Vector2((position.x / MiniMapConstants.MAP_WIDTH) * MiniMapConstants.MINIMAP_WIDTH * MiniMapConstants.SIZEUP ,    //#11-3 10배 더 멀리   
                                                                                            // +20(플레이어와의 위치 미세 조정 필요 - 플레이어는 0.0으로 표시하는 게 아니라, 캔버스 박스의 중심에 나타내야 하니까)     
                                                (position.z /* position.y*/ / MiniMapConstants.MAP_HEIGHT) * MiniMapConstants.MINIMAP_HEIGHT * MiniMapConstants.SIZEUP);  //#11-3 10배 더 멀리    

            //UI 이미지 생성 - 자식 오브젝트로 넣고 이미지도 각각 갖도록(이름은 Layer의 이름대로 정해서 만들어)
            minimapObject = new GameObject(enumObjectName); // typeofImage라는 string의 이름을 가진 게임오브젝트를 자식들로 생성
            
            minimapObject.transform.SetParent(minimapRawImage.transform, false);
            minimapImage = minimapObject.AddComponent<Image>();   // 하나하나 이미지를 넣어
            //자식들 - 이미지를 올바르게 연결해서 만들어
            minimapImage.sprite = spriteToUse;

            minimapImage.rectTransform.anchoredPosition = positionOnRawImage;   //월드좌표에서 -> 캔버스 위치로 (미니맵 규격에 맞게) 맞추자

//////////////
            minimapRectT = minimapObject.GetComponent<RectTransform>(); //보완 : 크기를 줄여야 좀 정상적으로 보이더라 -> 그래서 밑에서 Vector2(10f, 10f)으로 하고 있는 것
            if(minimapRectT == null)
            {
                Debug.Log("미니맵 RectTransform이 null임");
            }
            minimapRectT.sizeDelta = new Vector2(50f, 50f);   //(10f, 10f);   //#11-1 더 크~게 찍히도록 *10씩 (절대적인 크기는 아님. 원하는 대로 변경해도 돼)

        }
        minimapTexture.Apply(); // 위에서 생성된 미니맵 이미지를 렌더 텍스쳐인 minimapTexture에 적용.
        minimapRawImage.texture = minimapTexture;   //부모인 minimapRawImage에 minimapTexture를 알맞게 지정(연결)
    }

}