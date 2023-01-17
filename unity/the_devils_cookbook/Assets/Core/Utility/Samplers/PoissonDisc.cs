using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TDC.Core.Extension;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TDC.Core.Utility.Samplers
{
    [Serializable]
    public class PoissonDisc
    {
        [SerializeField]
        private int[] _Cells;
        [SerializeField]
        private Vector2Int _CellCount;
        [SerializeField]
        private List<Vector2> _Points;
        public ReadOnlyCollection<Vector2> Points => _Points.AsReadOnly();
        [SerializeField] private Vector2 _Origin;
        [SerializeField]
        private Vector2 _DomainSize;
        [SerializeField]
        private Vector2 _DomainExtents;
        [SerializeField]
        private float _Spacing;
        [SerializeField]
        private float _SqrSpacing;
        [SerializeField]
        private float _CellSize;

        private static readonly float _Sqrt2 = Mathf.Sqrt(2); 

        private int IndexFrom2D(int x, int y)
        {
            return _CellCount.y * y + x;
        }
        
        private Vector2Int PositionToIndex(Vector2 position)
        {
            Vector2 positionFromZero = position - _Origin + _DomainExtents;
            return new Vector2Int(
                Mathf.FloorToInt(positionFromZero.x / _CellSize),
                Mathf.FloorToInt(positionFromZero.y / _CellSize));
        }
        
        public bool TryGetCellIndexAtPosition(Vector2 position, out Vector2Int index)
        {
            index = PositionToIndex(position);
            return Math.Range(index.x, 0, _CellCount.x - 1) && Math.Range(index.y, 0, _CellCount.y - 1);
        }

        public bool TryGetCellValueAtPosition(Vector2Int cellIndex, out int index)
        {
            return _Cells.TryGetValue(out index, IndexFrom2D(cellIndex.x, cellIndex.y));
        }

        public List<Vector2> GetPointsInRadius(Vector2 position, float radius)
        {
            float sqrRadius = Mathf.Pow(radius, 2);
            if (!TryGetCellIndexAtPosition(position, out Vector2Int centreIndex))
                throw new ArgumentException($"{position} lies outside of Poisson grid.");

            int cellRadius = Mathf.CeilToInt(radius / _CellSize);
            var points = new List<Vector2>();

            for (int x = centreIndex.x - cellRadius; x < centreIndex.x + cellRadius; x++)
            {
                for (int y = centreIndex.y - cellRadius; y < centreIndex.y + cellRadius; y++)
                {
                    if (!TryGetCellValueAtPosition(new Vector2Int(x, y), out int testCellValue)) continue;
                    if (testCellValue == -1) continue;
                    Vector2 point = _Points[testCellValue];
                    if ((point - position).sqrMagnitude > sqrRadius) continue;
                    points.Add(point);
                }
            }

            return points;
        }

        private bool IsPointValid(Vector2 position, out Vector2Int cellIndex, params Func<Vector2, bool>[] validators)
        {
            cellIndex = default;
            if (Mathf.Abs(position.x - _Origin.x) > _DomainExtents.x 
                || Mathf.Abs(position.y - _Origin.y) > _DomainExtents.y) return false;
            if (!TryGetCellIndexAtPosition(position, out cellIndex)) return false;

            if (validators?.Length > 0 && validators.Any(v => !v(position))) return false;
            
            for (int x = -2; x <= 2; x++)
            {
                for (int y = -2; y <= 2; y++)
                {
                    var testCellIndex = new Vector2Int(cellIndex.x + x, cellIndex.y + y);
                    if (!_Cells.InRange(IndexFrom2D(testCellIndex.x, testCellIndex.y))) continue;
                    int testPointIndex = _Cells[IndexFrom2D(testCellIndex.x, testCellIndex.y)];
                    if (testPointIndex == -1) continue;
                    Vector2 testPoint = _Points[testPointIndex];
                    if ((testPoint - position).sqrMagnitude < _SqrSpacing) return false;
                }
            }

            return true;
        }
        
        public static PoissonDisc Generate(Vector2 origin, Vector2 domainSize, float spacing, int sampleAttempts = 30, params Func<Vector2, bool>[] sampleValidators)
        {
            var disc = new PoissonDisc(origin, domainSize, spacing);
            var activePoints = new Queue<int>(disc._Cells.GetLength(0));

            Vector2 initialPoint;
            var attempts = 0;
            do
            {
                initialPoint = origin + new Vector2(Random.Range(-disc._DomainExtents.x, disc._DomainExtents.y),
                    Random.Range(-disc._DomainExtents.y / 2, disc._DomainExtents.y / 2));
                attempts++;
                if (attempts > 100)
                    throw new OverflowException($"Failed to find valid starting point within 100 iterations.");
            } while (sampleValidators?.Length > 0 && sampleValidators.Any(v => !v(initialPoint)));
            
            disc._Points.Add(initialPoint);
            int initialPointIndex = disc._Points.Count - 1;
            
            Vector2Int initialCellIndex = disc.PositionToIndex(initialPoint);
            disc._Cells[disc.IndexFrom2D(initialCellIndex.x, initialCellIndex.y)] = initialPointIndex;
            activePoints.Enqueue(initialPointIndex);
            
            while (activePoints.Count > 0)
            {
                int pointIndex = activePoints.Peek();
                Vector2 point = disc._Points[pointIndex];

                var isPointExhausted = true;
                foreach (Vector2 sample in Annulus.Points(sampleAttempts, spacing, spacing * 2, point))
                {
                    if (!disc.IsPointValid(sample, out Vector2Int sampleCellIndex, sampleValidators)) continue;
                    
                    disc._Points.Add(sample);
                    int samplePointIndex = disc._Points.Count - 1;
                    disc._Cells[disc.IndexFrom2D(sampleCellIndex.x, sampleCellIndex.y)] = samplePointIndex;
                    activePoints.Enqueue(samplePointIndex);
                    isPointExhausted = false;
                }

                if (isPointExhausted) activePoints.Dequeue();
            }

            return disc;
        }
        
        private PoissonDisc(Vector2 origin, Vector2 domainSize, float spacing)
        {
            _Origin = origin;
            _DomainSize = domainSize;
            _DomainExtents = domainSize / 2;
            _Spacing = spacing;
            _SqrSpacing = Mathf.Pow(_Spacing, 2);
            _CellSize = spacing / _Sqrt2;
            _CellCount = new Vector2Int(
                Mathf.CeilToInt(domainSize.x / _CellSize),
                Mathf.CeilToInt(domainSize.y / _CellSize));

            _Cells = new int[_CellCount.x * _CellCount.y];
            for (var i = 0; i < _Cells.Length; i++)
            {
                _Cells[i] = -1;
            }

            _Points = new List<Vector2>(_Cells.Length);
        }
    }
}