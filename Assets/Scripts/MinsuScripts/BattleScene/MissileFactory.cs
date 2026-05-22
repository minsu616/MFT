using UnityEngine;

public class MissileFactory : MonoBehaviour
{
    // 함선 타입별 발사체 설정
    public static GameObject CreateMissile(ShipController.ShipType shipType)
    {
        GameObject missileObj = new GameObject("Missile");
        Missile missile = missileObj.AddComponent<Missile>();

        switch (shipType)
        {
            case ShipController.ShipType.Battleship:
                // 전함 - 크고 느린 빨간 포탄
                SetupMissileVisual(missileObj,
                    color: Color.red,
                    scale: 0.5f,
                    isPrimitive: PrimitiveType.Sphere);
                missile.arcHeight = 15f;
                missile.flightDuration = 2.5f;
                break;

            case ShipController.ShipType.Carrier:
                // 항공모함 - 비행기 모양 (가로 캡슐)
                SetupMissileVisual(missileObj,
                    color: new Color(0.8f, 0.8f, 0.8f),
                    scale: 0.3f,
                    isPrimitive: PrimitiveType.Capsule);
                missile.arcHeight = 3f;
                missile.flightDuration = 1.5f;
                // 가로로 눕히기
                missileObj.transform.GetChild(0).localRotation =
                    Quaternion.Euler(0, 0, 90f);
                break;

            case ShipController.ShipType.Destroyer:
                // 구축함 - 작고 빠른 파란 포탄
                SetupMissileVisual(missileObj,
                    color: Color.blue,
                    scale: 0.2f,
                    isPrimitive: PrimitiveType.Sphere);
                missile.arcHeight = 5f;
                missile.flightDuration = 0.5f;
                break;

            case ShipController.ShipType.Cruiser:
                // 순양함 - 중간 노란 포탄
                SetupMissileVisual(missileObj,
                    color: Color.yellow,
                    scale: 0.3f,
                    isPrimitive: PrimitiveType.Sphere);
                missile.arcHeight = 8f;
                missile.flightDuration = 1.0f;
                break;

            case ShipController.ShipType.Submarine:
                // 잠수함 - 긴 어뢰 (캡슐)
                SetupMissileVisual(missileObj,
                    color: new Color(0.2f, 0.8f, 0.8f),
                    scale: 0.3f,
                    isPrimitive: PrimitiveType.Capsule);
                missile.arcHeight = 1f;  // 낮은 포물선 (수면 따라)
                missile.flightDuration = 1.8f;
                break;

            case ShipController.ShipType.SpeedBoat:
                // 고속정 - 아주 작고 빠른 노란 총알
                SetupMissileVisual(missileObj,
                    color: Color.yellow,
                    scale: 0.1f,
                    isPrimitive: PrimitiveType.Sphere);
                missile.arcHeight = 2f;
                missile.flightDuration = 0.3f;
                break;
        }

        return missileObj;
    }

    static void SetupMissileVisual(
        GameObject obj,
        Color color,
        float scale,
        PrimitiveType isPrimitive)
    {
        GameObject visual = GameObject.CreatePrimitive(isPrimitive);
        visual.transform.parent = obj.transform;
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = Vector3.one * scale;
        visual.GetComponent<Renderer>().material.color = color;
        Object.Destroy(visual.GetComponent<Collider>());
    }
}