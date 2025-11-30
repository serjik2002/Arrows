using UnityEngine;
// 1. Клас, що описує одну точку в ланцюжку (Linked List)
public class ArrowPoint
{
    public Vector2Int GridPosition; // Координата на сітці [x,y]
    public ArrowPoint Prev;         // Посилання на попередню точку
    public ArrowPoint Next;         // Посилання на наступну точку
}
