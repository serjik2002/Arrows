using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 4. DTO (Data Transfer Objects) - чисто для читання JSON
// (Unity JsonUtility не вміє читати складні структури, тому потрібні прості класи-посередники)
[System.Serializable]
public class LevelDataDTO
{
    public int width;
    public int height;
    public List<ArrowDataDTO> arrows;
}

[System.Serializable]
public class ArrowDataDTO
{
    public int id;
    public int[] cells; // Плоский масив [x,y,x,y...]
    public Vector2Int direction; // Поки не використовуємо, бо вираховуємо математично, але хай буде
}

public class ArrowPath
{
    public ArrowPoint startPoint;
    public ArrowPoint endPoint;
}
