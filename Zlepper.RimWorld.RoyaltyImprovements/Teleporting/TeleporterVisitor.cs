namespace Zlepper.RimWorld.RoyaltyImprovements.Teleporting;

public class TeleporterVisitor
{
    private Grid<VisitedCell?> _grid;
    private readonly MapAdapter _map;
    private readonly PriorityQueue<VisitedCell, double> _validQueue;
    private int _cellCheckedCount = 0;

    public TeleporterVisitor(MapAdapter map)
    {
        _grid = new Grid<VisitedCell?>(map);
        _map = map;
        _validQueue = new PriorityQueue<VisitedCell, double>();
    }

    public VisitedCell? GetCellInfo(IntVec3 position)
    {
        if (_grid.InBounds(position))
        {
            return _grid[position];
        }

        return null;
    }

    public void CalculateFullGrid(IReadOnlyCollection<Teleporter> teleporters)
    {
        if (teleporters.Count == 0)
        {
            return;
        }

        _validQueue.Clear();
        foreach (var teleporter in teleporters)
        {
            var cell = new VisitedCell(0, teleporter, teleporter.Position, teleporter.Position);
            _validQueue.Enqueue(cell, 0);
            _grid[teleporter.Position] = cell;
        }

        RecalculateQueue();
    }

    private void RecalculateQueue()
    {
        _cellCheckedCount = 0;
        while (_validQueue.TryDequeue(out var cell, out var distance))
        {
            if (cell.Distance < distance)
            {
                continue;
            }
            
            for (var i = 0; i < GenAdj.CardinalDirections.Length; i++)
            {
                var adjacentCell = GenAdj.CardinalDirections[i];
                CheckCell(adjacentCell, cell);
            }

            for (var i = 0; i < GenAdj.DiagonalDirections.Length; i++)
            {
                var adjacentCell = GenAdj.DiagonalDirections[i];
                CheckCell(adjacentCell, cell);
            }
        }

        // Log.Message($"Checked {_cellCheckedCount} cells while recalculating teleporter grid");
    }

    private void CheckCell(IntVec3 adjacentCell, VisitedCell cell)
    {
        _cellCheckedCount++;
        var toTest = cell.Position + adjacentCell;
        if (!_grid.InBounds(toTest))
        {
            return;
        }

        if (!CanReach(cell.Position, adjacentCell))
        {
            return;
        }

        var distance = GetNewDistance(adjacentCell, cell, toTest);

        // If it has already been visited, then it should be either the same distance or a shorter distance
        // so we can skip it
        var existing = _grid[toTest];
        if (existing != null)
        {
            if (existing.Distance <= distance)
            {
                return;
            }
        }
        else if (!_map.CanEnter(toTest))
        {
            return;
        }

        if (existing == null)
        {
            existing = new VisitedCell(distance, cell.Teleporter, cell.Position, toTest);
            _grid[toTest] = existing;
        }
        else
        {
            existing.Distance = distance;
            existing.Previous = cell.Position;
            existing.Teleporter = cell.Teleporter;
        }

        _validQueue.Enqueue(existing, existing.Distance);
    }

    private double GetNewDistance(IntVec3 adjacentCell, VisitedCell cell, IntVec3 destination)
    {
        var index = CellIndicesUtility.CellToIndex(destination, _map.Size.x);
        var previousIndex = CellIndicesUtility.CellToIndex(cell.Position, _map.Size.x);
        return cell.Distance + WalkDistance(adjacentCell) * _map.GetPathCost(index, previousIndex);
    }
    
    private double GetNewDistance(VisitedCell cell, IntVec3 destination)
    {
        return GetNewDistance(destination - cell.Position, cell, destination);
    }

    public Grid<Teleporter?> GetGridForClosestTeleporter()
    {
        return _grid.Select(c => c?.Teleporter);
    }

    private static double WalkDistance(IntVec3 adjacentCell)
    {
        return Math.Sqrt(Math.Abs(adjacentCell.x * adjacentCell.x) + Math.Abs(adjacentCell.z * adjacentCell.z));
    }


    public record VisitedCell
    {
        public double Distance;
        public Teleporter Teleporter;
        public IntVec3 Previous;
        public readonly IntVec3 Position;

        public VisitedCell(double Distance, Teleporter Teleporter, IntVec3 Previous, IntVec3 Position)
        {
            this.Distance = Distance;
            this.Teleporter = Teleporter;
            this.Previous = Previous;
            this.Position = Position;
        }
    }

    public void CellUpdated(IntVec3 cell)
    {
        // Log.Message($"Cell updated: {cell}");
        if (_map.CanEnter(cell))
        {
            // Log.Message("Can enter");
            var existingEntry = _grid[cell];
            if (existingEntry != null)
            {
                // Log.Message("Found existing entry");
                _validQueue.Enqueue(existingEntry, existingEntry.Distance);
            }
            else
            {
                // Log.Message("Newly opened, checking for existing paths");
                for (var i = 0; i < GenAdj.AdjacentCells.Length; i++)
                {
                    var adjacentCell = GenAdj.AdjacentCells[i];
                    var toTest = cell + adjacentCell;
                    if (!_grid.InBounds(toTest))
                    {
                        continue;
                    }

                    if (!CanReach(cell, adjacentCell))
                    {
                        continue;
                    }

                    var option = _grid[toTest];
                    if (option != null)
                    {
                        _validQueue.Enqueue(option, option.Distance);
                    }
                }
            }
        }
        else
        {
            var entry = _grid[cell];
            if (entry == null)
            {
                return;
            }

            var queue = new Queue<VisitedCell>();
            var validCells = new HashSet<IntVec3>();
            
            _grid[cell] = null;
            
            foreach (var adjacentCell in GenAdj.AdjacentCells)
            {
                var toTest = cell + adjacentCell;
                if (!_grid.InBounds(toTest))
                {
                    continue;
                }
                
                var option = _grid[toTest];
                if (option == null)
                {
                    continue;
                }

                if (option.Teleporter != entry.Teleporter)
                {
                    _validQueue.Enqueue(option, option.Distance);
                    continue;
                }

                var prevPosition = option.Previous;
                if(!_map.CanEnter(prevPosition) || !CanReach(prevPosition, toTest - prevPosition))
                {
                    _grid[toTest] = null;
                    queue.Enqueue(option);
                }
                else
                {
                    validCells.Add(option.Position);
                }
            }
            
            InvalidateBlockedCells(queue);
            foreach (var validCell in validCells)
            {
                if(_grid[validCell] is {} validEntry)
                {
                    _validQueue.Enqueue(validEntry, validEntry.Distance);
                }
            }
        }
        
        RecalculateQueue();
    }


    private void InvalidateBlockedCells(Queue<VisitedCell> invalidationQueue)
    {
        var alreadyChecked = new HashSet<IntVec3>();
        _cellCheckedCount = 0;
        while (invalidationQueue.TryDequeue(out var cell))
        {
            for (var i = 0; i < GenAdj.AdjacentCells.Length; i++)
            {
                _cellCheckedCount++;
                var adjacentCell = GenAdj.AdjacentCells[i];
                var toTest = cell.Position + adjacentCell;
                if (!alreadyChecked.Add(toTest))
                {
                    continue;
                }
                
                if (!_grid.InBounds(toTest))
                {
                    continue;
                }
                
                var neighbour = _grid[toTest];
                if (neighbour == null)
                {
                    // Already checked or already invalid
                    continue;
                }
                
                if (neighbour.Teleporter != cell.Teleporter)
                {
                    // Log.Message($"Cell {neighbour.Position} and {cell.Position} are not on the same teleporter");
                    // Different teleporter, so it's still valid, which might now be the shortest path
                    _validQueue.Enqueue(neighbour, neighbour.Distance);
                    continue;
                }

                if (neighbour.Previous == cell.Position)
                {
                    // Log.Message($"Neighbour {neighbour.Position} came from {cell.Position}");
                    invalidationQueue.Enqueue(neighbour);
                    _grid[toTest] = null;
                }
            }
        }
        // Log.Message($"Checked {_cellCheckedCount} cells while invalidating blocked cells. Came across {alreadyChecked.Count} unique cells");
    }

    private bool CanReach(IntVec3 previous, IntVec3 adjacentCell)
    {
        // Not a diagonal, so always reachable
        if (adjacentCell.x == 0 || adjacentCell.z == 0)
        {
            return true;
        }

        var xEnter = previous + adjacentCell with
        {
            z = 0
        };
        var zEnter = previous + adjacentCell with
        {
            x = 0
        };

        return _map.CanEnter(xEnter) && _map.CanEnter(zEnter);
    }
}