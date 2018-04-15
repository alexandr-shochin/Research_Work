using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TracProg.Calculation.Algoriths
{
    public class RetraceAlgScheme
    {
        private Configuration _config;
        private int _countIter;

        private Dictionary<int, Net> _nonRealized = new Dictionary<int, Net>();

        public event Action<int> IterFinishEvent;

        public RetraceAlgScheme(Configuration config, int countIter, Dictionary<int, Net> nonRealized)
        {
            _config = config;
            _countIter = countIter;
            _nonRealized = nonRealized;
        }

        public long Calculate()
        {
            Dictionary<int, Net> _goodRetracing = new Dictionary<int, Net>();

            long time = 0;
            Stopwatch sw = new Stopwatch();
            for (int curIter = 0; curIter < _countIter; curIter++)
            {
                sw.Reset();
                sw.Start();
                foreach (var net in _nonRealized)
                {
                    RetraceAlgNet alg = new RetraceAlgNet(_config.Grid, _config.Net.Length, net.Key);
                    if (alg.FindPath(net.Value[0], net.Value[1]))
                    {
                        _config.Grid[net.Value[0]].ViewElement._Color = System.Drawing.Color.FromArgb(0, 100, 0);
                        _config.Grid[net.Value[1]].ViewElement._Color = System.Drawing.Color.FromArgb(0, 100, 0);

                        _goodRetracing.Add(net.Key, net.Value);
                    }
                }
                sw.Stop();
                time += sw.ElapsedMilliseconds;

                foreach (var item in _goodRetracing)
                {
                    if (_nonRealized.ContainsKey(item.Key))
                    {
                        _nonRealized.Remove(item.Key);
                    }
                }

                if (IterFinishEvent != null) IterFinishEvent.Invoke(curIter + 1);
            }

            return time;
        }
    }
}
