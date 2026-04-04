using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("맵 설정")]
    public int width = 30;//가로 칸 수
    public int height = 30;//세로 칸 수
    public float cellSize = 1f;//칸 하나 크기

    [Header("타일 머티리얼")]
    public Material tileA; //밝은 타일
    public Material timeB; //어두운 타일

    void Start()
    {
        GenerateMap();
    }


/*플레이 버튼 누를시
 *30*30=900개 타일 자동 생성
 * 체크무늬 파란색 마다 맵
 * Hierachy에 MapGenerator 하위로 정리됨
 */
    void GenerateMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                //타일 오브젝트 생성
                GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);

                //위치 설정
                tile.transform.position = new Vector3(x * cellSize,0,z * cellSize);

                //납작하게(바닥 타일처럼)
                tile.transform.localScale = new Vector3(cellSize,0.1f,cellSize);

                //체크무늬 색상
                Renderer rend = tile.GetComponent<Renderer>();
                if ((x + z) % 2 == 0)
                {
                    rend.material.color = new Color(0.2f, 0.5f, 0.8f);//파란색
                }
                else
                {
                    rend.material.color = new Color(0.1f, 0.3f, 0.6f);//진파란색
                }

                //MapGenerator 오브젝트 하위로 정리
                tile.transform.parent=this.transform;

                //이름 설정
                tile.name = $"Tile ({x},{z})";
            }
        }
    }


}
