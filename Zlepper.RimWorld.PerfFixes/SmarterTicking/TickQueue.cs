namespace Zlepper.RimWorld.PerfFixes.SmarterTicking;

public sealed class TickQueue
{
    private int _pointer;
    private readonly List<int>[] _queue;

    public TickQueue(int size)
    {
        _queue = new List<int>[size];
        for (var i = 0; i < size; i++)
        {
            _queue[i] = new List<int>();
        }
    }

    public void Add(int id)
    {
        var position = id % _queue.Length;
        _queue[position].Add(id);
    }


    public int[] Next()
    {
        _pointer = (_pointer + 1) % _queue.Length;

        var list = _queue[_pointer];
        var items = list.ToArray();

        list.Clear();
        return items;
    }

    public void Clear()
    {
        foreach (var list in _queue)
        {
            list.Clear();
        }
    }
}