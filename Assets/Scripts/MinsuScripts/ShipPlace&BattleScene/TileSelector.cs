using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class TileSelector : MonoBehaviour
{
    [Header("타일 색상")]
    public Color hoverColor = new Color(1f, 1f, 0f, 1f);
    public Color selectColor = new Color(1f, 0f, 0f, 1f);

    private GameObject hoveredTile;
    private GameObject selectedTile;

    // 타일별 원래 색상을 저장하는 딕셔너리
    private System.Collections.Generic.Dictionary<GameObject, Color> originalColors
        = new System.Collections.Generic.Dictionary<GameObject, Color>();

    void Update()
    {
        DetectTile();
    }

    void DetectTile()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            GameObject target = hit.collider.gameObject;

            if (target.name.Contains("Tile"))
            {
                // 원래 색상 딕셔너리에 없으면 저장
                if (!originalColors.ContainsKey(target))
                {
                    originalColors[target] = target.GetComponent<Renderer>().material.color;
                }

                // 다른 타일로 이동했을 때 이전 호버 타일 복구
                if (hoveredTile != null && hoveredTile != target && hoveredTile != selectedTile)
                {
                    hoveredTile.GetComponent<Renderer>().material.color = originalColors[hoveredTile];
                }

                // 호버 색상 적용
                if (target != selectedTile)
                {
                    target.GetComponent<Renderer>().material.color = hoverColor;
                }

                hoveredTile = target;

                // 클릭
                if (Input.GetMouseButtonDown(0))
                {
                    SelectTile(target);
                }
            }
        }
        else
        {
            // 타일 밖으로 나갔을 때 복구
            if (hoveredTile != null && hoveredTile != selectedTile)
            {
                hoveredTile.GetComponent<Renderer>().material.color = originalColors[hoveredTile];
                hoveredTile = null;
            }
        }
    }

    void SelectTile(GameObject tile)
    {
        // 이전 선택 타일 복구
        if (selectedTile != null)
        {
            selectedTile.GetComponent<Renderer>().material.color = originalColors[selectedTile];
        }

        selectedTile = tile;
        tile.GetComponent<Renderer>().material.color = selectColor;

        // 좌표 파싱
        string tileName = tile.name;
        tileName = tileName.Replace("Tile (", "").Replace(")", "");
        string[] coords = tileName.Split(',');
        int x = int.Parse(coords[0]);
        int z = int.Parse(coords[1]);

        Debug.Log($"선택한 타일 좌표: ({x}, {z})");
    }

    public Vector2Int GetSelectedCoord()
    {
        if (selectedTile == null) return new Vector2Int(-1, -1);

        string tileName = selectedTile.name;
        tileName = tileName.Replace("Tile (", "").Replace(")", "");
        string[] coords = tileName.Split(',');
        int x = int.Parse(coords[0]);
        int z = int.Parse(coords[1]);

        return new Vector2Int(x, z);
    }
}
