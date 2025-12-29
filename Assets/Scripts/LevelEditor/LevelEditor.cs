using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Простір імен для організації коду
namespace ArrowPuzzle
{
    // Структура для зберігання даних змійки
    [System.Serializable]
    public class SnakeData
    {
        public int id;
        public List<Vector2Int> cells; // Координати (x = col, y = row)
        public Vector2Int direction;   // Напрямок руху
        public bool removed = false;

        public SnakeData(int id, List<Vector2Int> cells)
        {
            this.id = id;
            this.cells = new List<Vector2Int>(cells);
            UpdateDirection();
        }

        public Vector2Int Head => cells[cells.Count - 1];
        public Vector2Int Tail => cells[0];

        public void UpdateDirection()
        {
            if (cells.Count < 2)
            {
                direction = Vector2Int.right; // Fallback
                return;
            }
            Vector2Int head = cells[cells.Count - 1];
            Vector2Int neck = cells[cells.Count - 2];
            direction = head - neck;
        }

        public void Flip()
        {
            cells.Reverse();
            UpdateDirection();
        }
    }

    public class LevelGenerator
    {
        private int width;  // Стовпці (j)
        private int height; // Рядки (i)
        private int[,] grid; // [Rows, Cols] -> [Height, Width]
        private List<GenSnake> snakes;
        private System.Random rng;

        public LevelGenerator(int seed = -1)
        {
            rng = seed == -1 ? new System.Random() : new System.Random(seed);
        }

        public LevelModel GenerateLevelModel(int w, int h, int minLen, int maxLen, float turnChance)
        {
            this.width = w;
            this.height = h;
            // Матриця [Row, Col]
            this.grid = new int[h, w];
            this.snakes = new List<GenSnake>();

            BuildLevelConstructive(minLen, maxLen, turnChance);
            FillGapsClean();
            OptimizeNoDelete();

            LevelModel level = new LevelModel();
            level.Width = w;
            level.Height = h;
            level.OccupiedGrid = (int[,])grid.Clone();

            foreach (var snake in snakes)
            {
                ArrowModel arrow = new ArrowModel();
                arrow.Id = snake.id;

                ArrowPoint prevPoint = null;
                for (int k = 0; k < snake.cells.Count; k++)
                {
                    Vector2Int pos = snake.cells[k];
                    ArrowPoint p = new ArrowPoint { GridPosition = pos };

                    if (prevPoint != null)
                    {
                        prevPoint.Next = p;
                        p.Prev = prevPoint;
                    }

                    if (k == 0) arrow.StartPoint = p;
                    if (k == snake.cells.Count - 1) arrow.EndPoint = p;

                    prevPoint = p;
                }
                level.AddArrow(arrow);
            }

            return level;
        }

        private void BuildLevelConstructive(int minLen, int maxLen, float turnChance)
        {
            int snakeId = 1;
            int failures = 0;
            int maxFailures = 200;

            while (failures < maxFailures)
            {
                var emptyCells = new List<Vector2Int>();

                // --- СТАНДАРТНИЙ ЦИКЛ: Рядок (i), Стовпець (j) ---
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        if (grid[i, j] == 0)
                        {
                            // Зберігаємо як Vector2Int(x, y) -> (j, i)
                            emptyCells.Add(new Vector2Int(j, i));
                        }
                    }
                }

                if (emptyCells.Count == 0) break;

                Vector2Int start = emptyCells[rng.Next(emptyCells.Count)];
                GenSnake newSnake = CreateRandomSnake(start, minLen, maxLen, turnChance, snakeId);

                if (newSnake == null) { failures++; continue; }

                snakes.Add(newSnake);
                foreach (var c in newSnake.cells) grid[c.y, c.x] = newSnake.id; // Access [row, col] -> [y, x]

                if (IsLevelSolvable())
                {
                    snakeId++;
                    failures = 0;
                }
                else
                {
                    foreach (var c in newSnake.cells) grid[c.y, c.x] = 0;
                    snakes.RemoveAt(snakes.Count - 1);
                    failures++;
                }
            }
        }

        private void FillGapsClean()
        {
            UpdateGridMap();
            bool changed = true;
            int iter = 0;
            while (changed && iter++ < 5)
            {
                changed = false;
                UpdateGridMap();
                var gaps = new List<Vector2Int>();

                // --- СТАНДАРТНИЙ ЦИКЛ ---
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        // Перевіряємо [Row, Col] -> [i, j]
                        if (grid[i, j] == 0) gaps.Add(new Vector2Int(j, i)); // Store (x, y)
                    }
                }

                gaps = gaps.OrderBy(a => rng.Next()).ToList();

                foreach (var gap in gaps)
                {
                    var neighbors = GetNeighbors(gap);
                    var candidates = new List<(GenSnake s, bool isHead)>();

                    foreach (var n in neighbors)
                    {
                        // Check [y, x]
                        if (grid[n.y, n.x] != 0)
                        {
                            var s = snakes.Find(x => x.id == grid[n.y, n.x]);
                            if (s == null) continue;

                            bool isHead = (Mathf.Abs(s.Head.x - gap.x) + Mathf.Abs(s.Head.y - gap.y) == 1);
                            bool isTail = (Mathf.Abs(s.cells[0].x - gap.x) + Mathf.Abs(s.cells[0].y - gap.y) == 1);

                            if (isHead) candidates.Add((s, true));
                            else if (isTail) candidates.Add((s, false));
                        }
                    }

                    if (candidates.Count > 0)
                    {
                        var choice = candidates[rng.Next(candidates.Count)];
                        if (choice.isHead)
                        {
                            choice.s.cells.Add(gap);
                            choice.s.UpdateDirection();
                        }
                        else
                        {
                            choice.s.cells.Insert(0, gap);
                            choice.s.UpdateDirection();
                        }
                        grid[gap.y, gap.x] = choice.s.id;
                        changed = true;
                    }
                }
            }
        }

        private GenSnake CreateRandomSnake(Vector2Int start, int minLen, int maxLen, float turnChance, int id)
        {
            var cells = new List<Vector2Int> { start };
            Vector2Int curr = start;
            int targetLen = rng.Next(minLen, maxLen + 1);
            Vector2Int? lastDir = null;
            var used = new HashSet<Vector2Int> { start };

            for (int k = 0; k < targetLen - 1; k++)
            {
                var neighbors = GetNeighbors(curr)
                    .Where(n => grid[n.y, n.x] == 0 && !used.Contains(n)) // Access [y, x]
                    .ToList();

                if (neighbors.Count == 0) break;

                Vector2Int next;
                Vector2Int? straight = null;
                if (lastDir.HasValue)
                {
                    Vector2Int s = curr + lastDir.Value;
                    if (neighbors.Contains(s)) straight = s;
                }

                if (rng.NextDouble() < turnChance || straight == null) next = neighbors[rng.Next(neighbors.Count)];
                else next = straight.Value;

                cells.Add(next);
                used.Add(next);
                lastDir = next - curr;
                curr = next;
            }

            if (cells.Count < minLen) return null;

            Vector2Int head = cells[cells.Count - 1];
            Vector2Int tail = cells[0];
            int dHead = Mathf.Min(Mathf.Min(head.x, width - 1 - head.x), Mathf.Min(head.y, height - 1 - head.y));
            int dTail = Mathf.Min(Mathf.Min(tail.x, width - 1 - tail.x), Mathf.Min(tail.y, height - 1 - tail.y));
            if (dTail < dHead && rng.NextDouble() > 0.2) cells.Reverse();

            return new GenSnake(id, cells);
        }

        private bool IsLevelSolvable()
        {
            int[,] simGrid = (int[,])grid.Clone();
            var active = new List<GenSnake>(snakes);
            bool progress = true;

            while (progress && active.Count > 0)
            {
                progress = false;
                var next = new List<GenSnake>();
                foreach (var s in active)
                {
                    if (CanSnakeFly(s, simGrid))
                    {
                        foreach (var c in s.cells) simGrid[c.y, c.x] = 0; // [y, x]
                        progress = true;
                    }
                    else next.Add(s);
                }
                active = next;
            }
            return active.Count == 0;
        }

        private bool CanSnakeFly(GenSnake snake, int[,] currentGrid)
        {
            Vector2Int check = snake.Head + snake.direction;
            while (check.x >= 0 && check.x < width && check.y >= 0 && check.y < height)
            {
                if (currentGrid[check.y, check.x] != 0) return false; // [y, x]
                check += snake.direction;
            }
            return true;
        }

        private bool OptimizeNoDelete()
        {
            for (int k = 0; k < 500; k++)
            {
                if (IsLevelSolvable()) return true;
                var bad = snakes[rng.Next(snakes.Count)];
                bad.Flip();
                UpdateGridMap();
            }
            return IsLevelSolvable();
        }

        private void UpdateGridMap()
        {
            grid = new int[height, width]; // [Rows, Cols]
            foreach (var s in snakes)
                foreach (var c in s.cells)
                    grid[c.y, c.x] = s.id; // [y, x]
        }

        private List<Vector2Int> GetNeighbors(Vector2Int p)
        {
            var res = new List<Vector2Int>();
            if (p.x > 0) res.Add(new Vector2Int(p.x - 1, p.y));
            if (p.x < width - 1) res.Add(new Vector2Int(p.x + 1, p.y));
            if (p.y > 0) res.Add(new Vector2Int(p.x, p.y - 1));
            if (p.y < height - 1) res.Add(new Vector2Int(p.x, p.y + 1));
            return res;
        }
    }

    // Допоміжний внутрішній клас
    public class GenSnake
    {
        public int id;
        public List<Vector2Int> cells;
        public Vector2Int direction;

        public GenSnake(int id, List<Vector2Int> cells)
        {
            this.id = id; this.cells = new List<Vector2Int>(cells); UpdateDirection();
        }
        public Vector2Int Head => cells[cells.Count - 1];
        public void UpdateDirection()
        {
            if (cells.Count < 2) { direction = Vector2Int.right; return; }
            direction = cells[cells.Count - 1] - cells[cells.Count - 2];
        }
        public void Flip() { cells.Reverse(); UpdateDirection(); }
    }
}
