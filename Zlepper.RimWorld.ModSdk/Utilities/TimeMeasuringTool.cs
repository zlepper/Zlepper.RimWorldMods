using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Zlepper.RimWorld.ModSdk.Utilities;

public class TimeMeasuringTool
{
    private readonly List<OngoingTask> _runningTasks = new();

    private readonly Dictionary<string, long> _taskTicks = new();

    [Obsolete("ONLY FOR TESTING")]
    public IDisposable MeasureTask(string title)
    {
        if (_runningTasks.Count > 0)
        {
            _runningTasks[_runningTasks.Count - 1].Pause();
        }
        
        if (!_taskTicks.ContainsKey(title))
        {
            _taskTicks[title] = 0;
        }
        
        var task = new OngoingTask(this, title);
        _runningTasks.Add(task);
        return task;
    }

    public void Dump(TaskLoggingHelper logger)
    {
        foreach (var pair in _taskTicks.OrderBy(p => p.Value))
        {
            logger.LogMessage(MessageImportance.High, "{0}: {1}ms", pair.Key, pair.Value / TimeSpan.TicksPerMillisecond);
        }
    }

    private void Pop()
    {
        if (_runningTasks.Count > 0)
        {
            _runningTasks[_runningTasks.Count - 1].Resume();
        }
    }

    private class OngoingTask : IDisposable
    {
        private readonly TimeMeasuringTool _parent;
        private readonly string _title;
        private readonly Stopwatch _stopwatch;

        public OngoingTask(TimeMeasuringTool parent, string title)
        {
            _parent = parent;
            _title = title;
            _stopwatch = Stopwatch.StartNew();
        }

        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _stopwatch.Stop();
            _parent._taskTicks[_title] += _stopwatch.ElapsedTicks;
            
            _parent._runningTasks.Remove(this);
            _parent.Pop();
        }

        public void Pause()
        {
            _stopwatch.Stop();
        }

        public void Resume()
        {
            _stopwatch.Start();
        }
    }
}