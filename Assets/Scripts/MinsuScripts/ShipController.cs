using UnityEngine;

public class ShipController : MonoBehaviour
{
    public enum ShipType { Battleship, Carrier, Destroyer, Cruiser, Submarine, SpeedBoat }
    public ShipType shipType;

    private ShipData shipData;

    void Start()
    {
        switch (shipType)
        {
            case ShipType.Battleship: shipData = new Battleship(); break;
            case ShipType.Carrier: shipData = new Carrier(); break;
            case ShipType.Destroyer: shipData = new Destroyer(); break;
            case ShipType.Cruiser: shipData = new Cruiser(); break;
            case ShipType.Submarine: shipData = new Submarine(); break;
            case ShipType.SpeedBoat: shipData = new SpeedBoat(); break;
        }
        Debug.Log($"{shipData.ShipName} 생성완료! HP: {shipData.CurrentHP}");
    }

    public void TakeDamage(int damage)
    {
        shipData.TakeDamage(damage);
        Debug.Log($"{shipData.ShipName} HP: {shipData.CurrentHP}/{shipData.MaxHP}");

        if (shipData.IsSunk)
        {
            Debug.Log($"{shipData.ShipName} 침몰!");
            gameObject.SetActive(false);
        }
    }

    public ShipData GetData() => shipData;
}
