using System.Collections.Generic;
using UnityEngine;

namespace Alteracia.Web
{
    public static class Log
    {
        public static string log = "";
        
        public delegate void UpdateLog();
        public static event UpdateLog OnUpdateLog;

        private class Line
        {
            public float time;
            public string text;
        }

        private static List<List<Line>> _log = new List<List<Line>>();
        
        public static int Start(string line)
        {
            _log.Add(new List<Line>
            {
                new Line
                {
                    time = Time.realtimeSinceStartup,
                    text = line
                }
            });
            return _log.Count - 1;
        }

        public static void AddLine(int id, string line)
        {
            if (id < 0 || id >= _log.Count) return;
            
            _log[id].Add(new Line
            {
                time = Time.realtimeSinceStartup,
                text = line
            });
        }

        public static void Finish(int id, string line = null)
        {
            if (id < 0 || id >= _log.Count) return;
            
            if (!string.IsNullOrEmpty(line))
            {
                _log[id].Add(new Line
                {
                    time = Time.realtimeSinceStartup,
                    text = line
                });
            }
            log += $"\n{_log[id][0].text}\tstart";
            for (int i = 1; i < _log[id].Count - 1; i++)
                log += $"\n{_log[id][i].text}\t{_log[id][i].time - _log[id][i - 1].time} sec.";
            log +=  $"\n{_log[id][_log[id].Count - 1].text}\t{_log[id][_log[id].Count - 1].time - _log[id][0].time} sec.\n";
            OnUpdateLog?.Invoke();
        }
    }
}
