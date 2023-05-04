using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
// using UnityEngine.Tilemaps;

public enum MAP_TYPE {ROCK = 1, TREE, PLAYER, /*FOOD,*/ GOAL};

public static class MiniMapConstants
{
    // 맵의 폭, 길이 
    public const int MAP_WIDTH = 64;    //갈 수 있는 거리는 0~63.9까지니까..? //100; //40;    //#11-3 더 멀~리 찍히도록 *10씩
    public const int MAP_HEIGHT = 64;   //  100;  //20;
    public const int MINIMAP_WIDTH = 300;   //64 *3;  //1600;  //280;   //그림 약간 튀어나와서 300-20
    public const int MINIMAP_HEIGHT = 300;  //64 *3;  //900;  //180; //그림이 약간 튀어나오길래.. 200-20
}

public class MiniMap : MonoBehaviour    //@16 미니맵
{
    private int mapSize;    // = 5;(Start에서 지정)      //const 빼 - 확대 축소 기능 위해서 (5이면 더 멀리서 보는 효과, 10이면 더 확대하는 효과)
    private int zoomInSize = 10;    //줌인 했을 때의 크기
    private int zoomOutSize = 3;    //줌아웃 했을 때의 크기
    
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
    
    private List<Vector3> rockPositions;    
    private List<Vector3> treePositions;     

// 미니맵 상에 표시할 스프라이트 이미지 모음
    public Sprite rockSpriteImg;
    public Sprite treeSpriteImg;
    // public Sprite playerSpriteImg;
    // public Sprite goalSpriteImg;
//#11-4 플레이어 바라보는 방향(이정표)
    public RectTransform playerArrow;   //플레이어가 바라보는 방향(rotation Z 값 조정)(그냥 실제 플레이어의 y rotation값과 같게 하면 될 듯)

// 미니맵을 표시할 RawImage 오브젝트 - 나중에 오브젝트에 연결?
    [SerializeField]
    private RawImage zoomInRawImage; //여기에 미니맵이 그려질 거야  //테스트용 public 선언
    [SerializeField]
    private RawImage zoomOutRawImage; //여기에 미니맵이 그려질 거야  //테스트용 public 선언
//#11-4 //껐다 키기 위한 목적
    public GameObject zoomInRawObj;     //줌인 부모
    public GameObject zoomOutRawObj;    //줌아웃 부모

//@16-3 Start 반복문용
Vector3 tilePosition;
List<Vector3> positions;

//#11-4 FixedUpdate 반복문용(회전값 받아오기)
float rotationAngle;
Vector3 newRotation;

//@16-3 MakeMinimap 반복문용
Texture2D minimapTexture;
Sprite spriteToUse;

Vector2 positionOnRawImage;
GameObject minimapObject;
Image minimapImage;

//#11-3 맵에서 삭제하면 미니맵 오브젝트도 삭제되도록
List<MiniMapObjData> zoomOutMiniMap= new List<MiniMapObjData>();    //#11-3 용훈님 추가
List<MiniMapObjData> zoomInMiniMap= new List<MiniMapObjData>();

//#11-1 플레이어 좌표 기준으로 미니맵 통째로 움직이기
Transform playerTransform;   //테스트용 public 잠깐만
public GameObject[] btnSizeChange;  //[0] : btnSizeDown, btnSizeUp 버튼 연결

    //#11-5 플레이어 회전에 따라 맵도 돌도록
    // RectTransform zoomInRectT;
    // RectTransform zoomOutRectT;

    //###
    public csPhotonGame csPG;

    void Awake()    
    {
        //zoomInRawImage = GameObject.Find("zoomInRawImage").GetComponent<RawImage>();   //scPlayUi에 있는 미니맵 연결하기
        //zoomOutRawImage = GameObject.Find("zoomOutRawImage").GetComponent<RawImage>();   //scPlayUi에 있는 미니맵 연결하기
        //playerTransform = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
    
        //#11-5 플레이어 회전에 따라 맵도 돌도록
        // zoomInRectT = zoomInRawObj.GetComponent<RectTransform>();
        // zoomOutRectT = zoomOutRawObj.GetComponent<RectTransform>();

    }

    void Start()
    {
        btnSizeChange[0].SetActive(false);   // btnSizeDown 켜두고
        btnSizeChange[1].SetActive(true);  // btnSizeUp 꺼두기

        setMinimapPos();    //미니맵 박스 위치 맞춰주기
//#11-4 디폴트는 줌아웃 사이즈로    
        zoomInRawObj.SetActive(false);
        zoomOutRawObj.SetActive(true);

// @16-3 List 정의
    rockPositions = new List<Vector3>();
    treePositions = new List<Vector3>();

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
    rockPositions = GetPositionsOnTag("Rock");   //("Player");
    treePositions = GetPositionsOnTag("Tree");  //("Goals");

//@16-3 특정 태그를 통해 위치 가져오기
    // 땅, 벽 타일맵의 위치 리스트를 미니맵에 그려주기
//#11-3    MakeMinimap(groundPositions, MAP_TYPE.GROUND);

    MakeMinimap(treePositions, MAP_TYPE.TREE, true);    //zoomIn으로 한번 그려놓고
    MakeMinimap(treePositions, MAP_TYPE.TREE, false);   //zoomOut으로 한번 그려놓기
    
    MakeMinimap(rockPositions, MAP_TYPE.ROCK, true);    //zoomIn으로 한번 그려놓고
    MakeMinimap(rockPositions, MAP_TYPE.ROCK, false);   //zoomOut으로 한번 그려놓기

    mapSize = zoomOutSize;  //디폴트는 줌아웃으로 가자

    }
    
    void FixedUpdate()  //#11-1 Update보다 FixedUpdate가 맞으려나? 카메라 위치 가져오는 것도 FixedUpdate에서 했었으니까 같은 맥락일 듯..?
    {
        //###
        if (csPG.myPlyerCtrl==null)
        {
            return;
        }
        else if(csPG.myPlyerCtrl!=null && playerTransform == null)
        {
            playerTransform = csPG.myPlyerCtrl.transform;
        }

        //#11-4 플레이어 바라보는 방향으로 맞추기
        // Quaternion arrowrotation = Quaternion.Euler(0, 0, playerTransform.rotation.y);
        // playerArrow.localRotation = arrowrotation;
        rotationAngle = -playerTransform.rotation.eulerAngles.y;
        newRotation = new Vector3(0, 0, rotationAngle);
        playerArrow.eulerAngles = newRotation;

        //#11-5 플레이어 회전에 따라 지도도 회전하도록
        // zoomInRectT.eulerAngles = newRotation;
        // zoomOutRectT.eulerAngles = newRotation;
        

        zoomInRawImage.rectTransform.anchoredPosition = new Vector2(/*-70f(미세조정용) */ -(MiniMapConstants.MINIMAP_WIDTH * zoomInSize)/MiniMapConstants.MAP_WIDTH * playerTransform.position.x,   //(-150f(중점좌표 중 X)+75f(중심에 맞추기 위해)) -((MINIMAP_WIDTH * SIZEUP)/MiniMapConstants.MAP_WIDTH) *  (6000/2 - 300)
                                                                     /*-70f(미세조정용) */ -(MiniMapConstants.MINIMAP_HEIGHT * zoomInSize)/MiniMapConstants.MAP_HEIGHT * playerTransform.position.z);  // (-150f(중점좌표 중 Y)+75f(중심에 맞추기 위해)) -((MINIMAP_WIDTH * SIZEUP)/MiniMapConstants.MAP_HEIGHT) * 6000/2 - 300
        //#11-4
        zoomOutRawImage.rectTransform.anchoredPosition = new Vector2(/*-70f(미세조정용) */ -(MiniMapConstants.MINIMAP_WIDTH * zoomOutSize)/MiniMapConstants.MAP_WIDTH * playerTransform.position.x,   //(-150f(중점좌표 중 X)+75f(중심에 맞추기 위해)) -((MINIMAP_WIDTH * SIZEUP)/MiniMapConstants.MAP_WIDTH) *  (6000/2 - 300)
                                                                /*-70f(미세조정용) */ -(MiniMapConstants.MINIMAP_HEIGHT * zoomOutSize)/MiniMapConstants.MAP_HEIGHT * playerTransform.position.z);  // (-150f(중점좌표 중 Y)+75f(중심에 맞추기 위해)) -((MINIMAP_WIDTH * SIZEUP)/MiniMapConstants.MAP_HEIGHT) * 6000/2 - 300

    }

    void setMinimapPos()
    {
        //#11-3 미니맵 박스 위치 맞춰주기
        zoomInRawImage.rectTransform.anchoredPosition = new Vector2(-150f, -150f);  //#11-3 미니맵 중심을 좌측 하단으로 맞춰주기
        zoomInRawImage.rectTransform.sizeDelta = new Vector2(600*zoomInSize, 600*zoomInSize);    //#11-3 위치 및 크기 맞춰주기 위함 (캔버스 상에서 부모 크기가 300임)
                        //거기에 중심을 좌측 하단으로 했으므로 크기 *2, 더 크게 보기 위해 *10씩 -> 300 * 2 * 10
        //#11-4
        zoomOutRawImage.rectTransform.anchoredPosition = new Vector2(-150f, -150f);  //#11-3 미니맵 중심을 좌측 하단으로 맞춰주기
        zoomOutRawImage.rectTransform.sizeDelta = new Vector2(600*zoomOutSize, 600*zoomOutSize);    //#11-3 위치 및 크기 맞춰주기 위함 (캔버스 상에서 부모 크기가 300임)
                        //거기에 중심을 좌측 하단으로 했으므로 크기 *2, 더 크게 보기 위해 *10씩 -> 300 * 2 * 10
    }

    private List<Vector3> GetPositionsOnTag(string tagName) //태그를 이용해 특정 오브젝트의 위치 가져오기
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


    public void RemoveObj(int x, int z)
    {
        for(int i=0;i<zoomOutMiniMap.Count;i++)
        {
            if(zoomOutMiniMap[i].GetX == x)
            {
                if(zoomOutMiniMap[i].GetZ==z){
                    Destroy( zoomOutMiniMap[i].gameObject);
                    Destroy( zoomInMiniMap[i].gameObject);

                    zoomOutMiniMap.RemoveAt(i);
                    zoomInMiniMap.RemoveAt(i);  //오브젝트 아예 지워
                    break;
                }
            }
        }
    }

    private void MakeMinimap(List<Vector3> positions, MAP_TYPE _mapType, bool _zoomIn)
    {
        int _mapSize;

        if(_zoomIn)         // 줌인 상태의 화면을 원하면 10으로 설정하고 만들기
            _mapSize = zoomInSize;
        else                // 아니면 5로 설정하고 만들기
            _mapSize = zoomOutSize;

        // 미니맵 이미지 생성 - 크기 맞춰서
        minimapTexture = new Texture2D(MiniMapConstants.MINIMAP_WIDTH * _mapSize, MiniMapConstants.MINIMAP_HEIGHT * _mapSize);

        //스프라이트 이미지 생성
        spriteToUse = null;  //typeofImage에 따라 사용할 이미지 딱 1개 정해두기 - swtich 문 계속 탈 필요 없이
        switch(_mapType)
            {
                case MAP_TYPE.ROCK :
                    spriteToUse = rockSpriteImg;
                    enumObjectName = "RockMini";
                    break;
                case MAP_TYPE.TREE : 
                    spriteToUse = treeSpriteImg;
                    enumObjectName = "TreeMini";
                    break;
            }

        foreach(Vector3 position in positions)  //하나하나 position 값을 가져와서 자식 만들고, 이미지 넣어주기, 캔버스 내 위치로 잡아주기
        {
            // 월드좌표를 RawImage 상에서의 좌표로 변환 표현 
            positionOnRawImage = new Vector2((position.x / MiniMapConstants.MAP_WIDTH) * MiniMapConstants.MINIMAP_WIDTH * _mapSize ,    //#11-3 10배 더 멀리   
                                                                                            // +20(플레이어와의 위치 미세 조정 필요 - 플레이어는 0.0으로 표시하는 게 아니라, 캔버스 박스의 중심에 나타내야 하니까)     
                                                (position.z /* position.y*/ / MiniMapConstants.MAP_HEIGHT) * MiniMapConstants.MINIMAP_HEIGHT * _mapSize);  //#11-3 10배 더 멀리    

            //UI 이미지 생성 - 자식 오브젝트로 넣고 이미지도 각각 갖도록(이름은 Layer의 이름대로 정해서 만들어)
            minimapObject = new GameObject(enumObjectName); // typeofImage라는 string의 이름을 가진 게임오브젝트를 자식들로 생성

            if(_zoomIn)
                minimapObject.transform.SetParent(zoomInRawImage.transform, false);
            else
                minimapObject.transform.SetParent(zoomOutRawImage.transform, false);


            minimapImage = minimapObject.AddComponent<Image>();   // 하나하나 이미지를 넣어

            minimapObject.AddComponent<MiniMapObjData>().SetData((int)position.x, (int)position.z);

            if(_zoomIn)
                zoomInMiniMap.Add(minimapObject.GetComponent<MiniMapObjData>());
            else    
                zoomOutMiniMap.Add(minimapObject.GetComponent<MiniMapObjData>());

            //자식들 - 이미지를 올바르게 연결해서 만들어
            minimapImage.sprite = spriteToUse;

            minimapImage.rectTransform.anchoredPosition = positionOnRawImage;   //월드좌표에서 -> 캔버스 위치로 (미니맵 규격에 맞게) 맞추자

//////////////
            minimapRectT = minimapObject.GetComponent<RectTransform>(); //보완 : 크기를 줄여야 좀 정상적으로 보이더라 -> 그래서 밑에서 Vector2(10f, 10f)으로 하고 있는 것
            if(minimapRectT == null)
            {
                Debug.Log("미니맵 RectTransform이 null임");
            }
            minimapRectT.sizeDelta = new Vector2(_mapSize * 10f, _mapSize * 10f);   //(10f, 10f);   //#11-1 더 크~게 찍히도록 *10씩 (절대적인 크기는 아님. 원하는 대로 변경해도 돼)

        }
        minimapTexture.Apply(); // 위에서 생성된 미니맵 이미지를 렌더 텍스쳐인 minimapTexture에 적용.

        if(_zoomIn)
            zoomInRawImage.texture = minimapTexture;   //부모인 zoomInRawImage에 minimapTexture를 알맞게 지정(연결)
        else
            zoomOutRawImage.texture = minimapTexture;   //#11-3 부모 따로 생기도록

    }

    public void ChangeMinimapZoom(bool zoomIn)    //미니맵 확대(true)/ 축소(false)
    {

        if(zoomIn && (mapSize != zoomInSize))       //확대하고자 한다면
        {
            Debug.Log("확대하기");
            btnSizeChange[1].SetActive(false);
            btnSizeChange[0].SetActive(true);

            mapSize = zoomInSize;
            setMinimapPos();
        //    MakeMinimap(treePositions, MAP_TYPE.TREE);
        // 부모 오브젝트를 활성화, 비활성화 하기
            zoomOutRawObj.SetActive(false);
            zoomInRawObj.SetActive(true);
        }
        if(!zoomIn && (mapSize != zoomOutSize))       //축소하고자 한다면
        {
            Debug.Log("축소하기");
            btnSizeChange[0].SetActive(false);
            btnSizeChange[1].SetActive(true);

            mapSize = zoomOutSize;
            setMinimapPos();
        //    MakeMinimap(treePositions, MAP_TYPE.TREE);
        // 부모 오브젝트를 활성화, 비활성화 하기
            zoomInRawObj.SetActive(false);
            zoomOutRawObj.SetActive(true);

        }
        
    }

}
