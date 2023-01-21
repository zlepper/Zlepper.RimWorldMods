using System.Globalization;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Zlepper.RimWorld.RoyaltyImprovements.Teleporting;

public class TeleporterDistanceTracker : MapComponent
{
    private readonly HashSet<Teleporter> _teleporters = new();

    private Grid<Teleporter?> _teleporterGrid;
    private readonly int _mapSizeX;
    private Grid<Teleporter?> _closestTeleporterDistanceGrid;
    private Grid<Teleporter?> _teleporter8WayGrid;
    private bool _closestTeleporterGridDirty = true;
    private readonly CellBoolDrawer _closestTeleporterDistanceGridDebugDrawer;
    private TeleporterVisitor? _teleporterVisitor;
    private readonly HashSet<IntVec3> _dirtyCells = new();
    private int _updatedCellCount = 0;


    public TeleporterDistanceTracker(Map map) : base(map)
    {
        _mapSizeX = map.Size.x;
        _teleporterGrid = new Grid<Teleporter?>(map);
        _closestTeleporterDistanceGrid = new Grid<Teleporter?>(map);
        _closestTeleporterDistanceGridDebugDrawer =
            new CellBoolDrawer(new TeleporterCellDrawer(this), map.Size.x, map.Size.z);
        _teleporter8WayGrid = new Grid<Teleporter?>(map);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Teleporter? GetTeleporter(IntVec3 cell)
    {
        return GetTeleporter(CellIndicesUtility.CellToIndex(cell, _mapSizeX));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Teleporter? GetSkipdoorInArea(IntVec3 cell)
    {
        return _teleporter8WayGrid[cell];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Teleporter? GetTeleporter(int index)
    {
        return _teleporterGrid[index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Teleporter? GetClosestTeleporter(IntVec3 cell)
    {
        return GetClosestTeleporter(CellIndicesUtility.CellToIndex(cell, _mapSizeX));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Teleporter? GetClosestTeleporter(int x, int z)
    {
        return GetClosestTeleporter(CellIndicesUtility.CellToIndex(x, z, _mapSizeX));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Teleporter? GetClosestTeleporter(int index)
    {
        RecalculateClosestSkipdoor();
        return _closestTeleporterDistanceGrid[index];
    }

    private void RecalculateClosestSkipdoor()
    {
        if (_updatedCellCount > 150)
        {
            _closestTeleporterGridDirty = true;
        }
        
        if (_closestTeleporterGridDirty)
        {
            _closestTeleporterGridDirty = false;
            _updatedCellCount = 0;
            _teleporterVisitor = new TeleporterVisitor(map);
            _teleporterVisitor.CalculateFullGrid(_teleporters);
        }
        else
        {
            if (_dirtyCells.Count == 0 || _teleporterVisitor == null)
            {
                return;
            }

            foreach (var dirtyCell in _dirtyCells)
            {
                _teleporterVisitor.CellUpdated(dirtyCell);
            }
        }

        _closestTeleporterDistanceGrid = _teleporterVisitor.GetGridForClosestTeleporter();
        _dirtyCells.Clear();
        _closestTeleporterDistanceGridDebugDrawer.SetDirty();
    }

    public void RegisterTeleporter(Thing teleporterThing)
    {
        var position = teleporterThing.Position;
        if (position.IsValid && teleporterThing.Map == map)
        {
            var teleporter = new Teleporter(teleporterThing);
            _teleporterGrid[CellIndicesUtility.CellToIndex(position, _mapSizeX)] = teleporter;
            _teleporters.Add(teleporter);
            MarkAsDirty();

            foreach (var adjPos in GenAdj.CellsAdjacent8Way(teleporterThing))
            {
                if (_teleporter8WayGrid.InBounds(adjPos))
                {
                    _teleporter8WayGrid[adjPos] = teleporter;
                }
            }
        }
    }

    public void MarkAsDirty()
    {
        _closestTeleporterGridDirty = true;
        _closestTeleporterDistanceGridDebugDrawer.SetDirty();
    }

    public void RemoveTeleporter(Thing teleporter)
    {
        var position = teleporter.Position;
        if (position.IsValid)
        {
            var existing = _teleporterGrid[position];
            if (existing != null)
            {
                _teleporters.Remove(existing);
                _teleporterGrid[position] = null;
                _closestTeleporterGridDirty = true;
            }

            foreach (var adjPos in GenAdj.CellsAdjacent8Way(teleporter))
            {
                if (_teleporter8WayGrid.InBounds(adjPos))
                {
                    _teleporter8WayGrid[adjPos] = null;
                }
            }
        }
    }

    public int GetTeleporterDistance(int fromCellX, int fromCellZ, int toCellX, int toCellZ, int cardinalSpeed,
        int ordinalSpeed)
    {
        var selfSkipdoor = GetClosestTeleporter(fromCellX, fromCellZ);
        if (selfSkipdoor == null)
        {
            return int.MaxValue;
        }

        var targetSkipdoor = GetClosestTeleporter(toCellX, toCellZ);
        if (targetSkipdoor == null)
        {
            return int.MaxValue;
        }

        var distanceToSelfSkipdoor = GenMath.OctileDistance(Math.Abs(selfSkipdoor.Position.x - fromCellX),
            Math.Abs(selfSkipdoor.Position.y - fromCellZ), cardinalSpeed, ordinalSpeed);
        var distanceToTargetSkipdoor = GenMath.OctileDistance(Math.Abs(targetSkipdoor.Position.x - toCellX),
            Math.Abs(targetSkipdoor.Position.y - toCellZ), cardinalSpeed, ordinalSpeed);

        return distanceToSelfSkipdoor + distanceToTargetSkipdoor;
    }

    public void BuildingDirty(Building building)
    {
        _dirtyCells.Add(building.Position);
        _closestTeleporterDistanceGridDebugDrawer.SetDirty();
        _updatedCellCount++;
    }
    
    public void DebugDrawClosestTeleporterGrid()
    {
        _closestTeleporterDistanceGridDebugDrawer.MarkForDraw();
        _closestTeleporterDistanceGridDebugDrawer.CellBoolDrawerUpdate();
    }

    public void DebugDrawTeleporterDistances()
    {
        RecalculateClosestSkipdoor();
        if (_teleporterVisitor != null)
        {
            foreach (var pos in GenRadial.RadialCellsAround(UI.MouseCell(), 8f, true))
            {
                var info = _teleporterVisitor.GetCellInfo(pos);
                if (info != null)
                {
                    var labelPos = GenMapUI.LabelDrawPosFor(pos);
                    if (TeleporterDebugSettings.DrawBestTravelDirectionForTeleporter)
                    {
                        labelPos.y -= 6;
                    }
                    
                    GenMapUI.DrawThingLabel(labelPos,
                        Math.Round(info.Distance, 2).ToString(CultureInfo.InvariantCulture), Color.white);
                }
            }
        }
    }

    public void DebugDrawBestTravelDirection()
    {
        RecalculateClosestSkipdoor();
        if (_teleporterVisitor != null)
        {
            foreach (var pos in GenRadial.RadialCellsAround(UI.MouseCell(), 8f, true))
            {
                var info = _teleporterVisitor.GetCellInfo(pos);
                if (info != null)
                {
                    var direction = info.Previous - info.Position;
                    var arrow = AdjacencyArrows.GetArrow(direction);

                    var labelPos = GenMapUI.LabelDrawPosFor(pos);
                    if (TeleporterDebugSettings.DrawTeleporterDistances)
                    {
                        labelPos.y += 6;
                    }
                    GenMapUI.DrawThingLabel(labelPos, arrow, Color.white);
                }
            }
        }
    }

}

public class TeleporterCellDrawer : ICellBoolGiver
{
    private readonly TeleporterDistanceTracker _tracker;

    public TeleporterCellDrawer(TeleporterDistanceTracker tracker)
    {
        _tracker = tracker;
    }

    public bool GetCellBool(int index)
    {
        return _tracker.GetClosestTeleporter(index) != null;
    }

    public Color GetCellExtraColor(int index)
    {
        var teleporter = _tracker.GetClosestTeleporter(index);
        if (teleporter == null)
        {
            return Color.white;
        }

        return teleporter.Color;
    }

    public Color Color => Color.white;
}