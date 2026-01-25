using UnityEngine;

public class ArrowPoint
{
    public GridCoordinate GridPosition; // Координата на сітці [x,y]
    public ArrowPoint Prev;         // Посилання на попередню точку
    public ArrowPoint Next;         // Посилання на наступну точку
}
