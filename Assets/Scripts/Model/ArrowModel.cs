using UnityEngine;
// 2. Клас, що описує одну стрілку (Логічна сутність)
public class ArrowModel
{
    public int Id;
    public ArrowPoint StartPoint; // Голова списку
    public ArrowPoint EndPoint;   // Хвіст списку (там, де малюється стрілка)
    public Color Color = Color.white; // Можна додати властивості, які впливають на геймплей
}
