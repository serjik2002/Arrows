using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[AddComponentMenu("UI/Effects/Multi Gradient")]
public class UIGradientMulti : BaseMeshEffect
{
    [Tooltip("Нажми на полоску, чтобы открыть редактор градиента")]
    public Gradient gradient = new Gradient(); // Встроенный класс Unity для списка цветов

    [Range(0f, 360f)]
    public float angle = 0f;

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive()) return;

        List<UIVertex> vertexList = new List<UIVertex>();
        vh.GetUIVertexStream(vertexList);
        int count = vertexList.Count;

        if (count == 0) return;

        // --- 1. Математика поворота (как в прошлом примере) ---
        float rad = -angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);

        // --- 2. Находим границы (Min/Max) в повернутой системе ---
        float min = float.MaxValue;
        float max = float.MinValue;

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = vertexList[i].position;
            // Проецируем точку на ось градиента
            float yRotated = pos.x * sin + pos.y * cos;

            if (yRotated > max) max = yRotated;
            if (yRotated < min) min = yRotated;
        }

        float uiHeight = max - min;
        if (uiHeight == 0) return;

        // --- 3. Применяем цвета из Gradient ---
        for (int i = 0; i < count; i++)
        {
            UIVertex uiVertex = vertexList[i];

            // Снова вычисляем повернутую позицию
            float yRotated = uiVertex.position.x * sin + uiVertex.position.y * cos;

            // Нормализуем t от 0 до 1
            float t = (yRotated - min) / uiHeight;

            // ГЛАВНОЕ ИЗМЕНЕНИЕ:
            // Вместо Lerp берем цвет из градиента в точке t.
            // Также умножаем на исходный цвет вершины (чтобы работала прозрачность самого Image)
            uiVertex.color = gradient.Evaluate(t) * uiVertex.color;

            vertexList[i] = uiVertex;
        }

        vh.Clear();
        vh.AddUIVertexTriangleStream(vertexList);
    }
}