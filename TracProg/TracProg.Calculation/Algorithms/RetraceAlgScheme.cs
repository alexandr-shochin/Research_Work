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

        int countRealizedBefore = 0;
        int countRealizedAFter = 0;

        public RetraceAlgScheme(Configuration config, int countIter, Dictionary<string, Tuple<List<int>, List<int>>> nonRealizedNet)
        {
            _config = config;
            _countIter = countIter;
            _nonRealizedNet = nonRealizedNet;

            foreach (var kv in nonRealizedNet)
            {
                countRealizedBefore += (kv.Value.Item1.Count - kv.Value.Item2.Count);
            }
            countRealizedAFter = countRealizedBefore;
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
                                //GridElement gridEl1 = _config.Grid[nonRealizedPin];
                                //Point p1 = _config.Grid.GetCoordCell(nonRealizedPin);
                                //gridEl1.ViewElement = new Pin(p1.x, p1.y, 1 * _config.Grid.Koeff, 1 * _config.Grid.Koeff);
                                //_config.Grid[nonRealizedPin] = gridEl1;

                                _goodRetracing.Add(nonRealizedPin);

                                if (isFinishPin)
                                {
                                    //GridElement gridEl2 = _config.Grid[finish];
                                    //Point p2 = _config.Grid.GetCoordCell(finish);
                                    //gridEl2.ViewElement = new Pin(p2.x, p2.y, 1 * _config.Grid.Koeff, 1 * _config.Grid.Koeff);
                                    //_config.Grid[nonRealizedPin] = gridEl2;

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
