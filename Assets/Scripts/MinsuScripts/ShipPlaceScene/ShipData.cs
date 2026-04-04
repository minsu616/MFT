public abstract class ShipData
{
    public string ShipName { get; protected set; }
    public int Size { get; protected set; }
    public int MaxHP { get; protected set; }
    public int CurrentHP { get; set; }
    public int Attack { get; protected set; }
    public int DetectRange { get; protected set; }
    public int AttackRange { get; protected set; }
    public int MoveRange { get; protected set; }
    public bool IsSunk => CurrentHP <= 0;

    protected ShipData()
    {
        CurrentHP = MaxHP;
    }

    public void TakeDamage(int damage)
    {
        CurrentHP -= damage;
        if (CurrentHP < 0) CurrentHP = 0;
    }

    public override string ToString()
    {
        return $"[{ShipName}] HP:{CurrentHP}/{MaxHP} ATK:{Attack} " +
               $"탐지:{DetectRange} 사거리:{AttackRange} 이동:{MoveRange} 침몰:{IsSunk}";
    }
}

public class Battleship : ShipData
{
    public Battleship() { ShipName = "전함"; Size = 5; MaxHP = 500; CurrentHP = MaxHP; Attack = 120; DetectRange = 4; AttackRange = 6; MoveRange = 2; }
}
public class Carrier : ShipData
{
    public Carrier() { ShipName = "항공모함"; Size = 5; MaxHP = 450; CurrentHP = MaxHP; Attack = 90; DetectRange = 7; AttackRange = 8; MoveRange = 2; }
}
public class Destroyer : ShipData
{
    public Destroyer() { ShipName = "구축함"; Size = 3; MaxHP = 350; CurrentHP = MaxHP; Attack = 80; DetectRange = 6; AttackRange = 4; MoveRange = 4; }
}
public class Cruiser : ShipData
{
    public Cruiser() { ShipName = "순양함"; Size = 4; MaxHP = 400; CurrentHP = MaxHP; Attack = 100; DetectRange = 5; AttackRange = 5; MoveRange = 3; }
}
public class Submarine : ShipData
{
    public Submarine() { ShipName = "잠수함"; Size = 3; MaxHP = 300; CurrentHP = MaxHP; Attack = 130; DetectRange = 3; AttackRange = 5; MoveRange = 3; }
}
public class SpeedBoat : ShipData
{
    public SpeedBoat() { ShipName = "고속정"; Size = 2; MaxHP = 200; CurrentHP = MaxHP; Attack = 60; DetectRange = 6; AttackRange = 3; MoveRange = 6; }
}
