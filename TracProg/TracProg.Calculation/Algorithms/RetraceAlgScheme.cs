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

        private Dictionary<string, Tuple<List<int>, List<int>>> _nonRealizedNet;

        public event Action<int> IterFinishEvent;

        public RetraceAlgScheme(Configuration config, int countIter, Dictionary<string, Tuple<List<int>, List<int>>> nonRealizedNet)
        {
            _config = config;
            _countIter = countIter;
            _nonRealizedNet = nonRealizedNet;
        }

        public long Calculate()
        {
            Stopwatch sw = new Stopwatch();
            sw.Reset();
            sw.Start();

            for (int curIter = 0; curIter < _countIter; curIter++)
            {
                foreach (var subNet in _nonRealizedNet)
                {
                    string metalID = subNet.Key;
                    List<int> allPins = subNet.Value.Item1;
                    List<int> nonRealizedPins = subNet.Value.Item2;

                    List<int> _goodRetracing = new List<int>();
                    foreach (var nonRealizedPin in nonRealizedPins)
                    {
                        if (!_goodRetracing.Contains(nonRealizedPin))
                        {
                            RetraceAlgNet alg = new RetraceAlgNet(_config.Grid, metalID, allPins);
                            int finish;
                            bool isFinishPin;
                            if (alg.FindPath(_config.Nets, nonRealizedPin, out finish, out isFinishPin))
                            {
                                GridElement gridEl1 = _config.Grid[nonRealizedPin];
                                IElement el1 = _config.Grid[nonRealizedPin].ViewElement;
                                gridEl1.ViewElement = new Pin(el1.X, el1.Y, el1.Width, el1.Height);
                                _config.Grid[nonRealizedPin] = gridEl1;

                                _goodRetracing.Add(nonRealizedPin);

                                if (isFinishPin)
                                {
                                    GridElement gridEl2 = _config.Grid[finish];
                                    IElement el2 = _config.Grid[finish].ViewElement;
                                    gridEl2.ViewElement = new Pin(el2.X, el2.Y, el2.Width, el2.Height);
                                    _config.Grid[nonRealizedPin] = gridEl2;

                                    _goodRetracing.Add(finish);
                                }
                            }
                        }
                    }

                    foreach (var pin in _goodRetracing)
                    {
                        subNet.Value.Item2.RemoveAll(x => x == pin);
                    }
                    if (subNet.Value.Item2.Count == 0)
                    {
                        
                    }

                    _goodRetracing.Clear();
                }

                if (IterFinishEvent != null) IterFinishEvent.Invoke(curIter + 1);
            }

            sw.Stop();

            return sw.ElapsedMilliseconds;
        }
    }
}
