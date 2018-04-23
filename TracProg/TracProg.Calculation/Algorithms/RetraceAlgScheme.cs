using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TracProg.Calculation.Algoriths
{
    public class RetraceAlgScheme
    {
        private Configuration _config;
        private int _countIter;

        private Dictionary<string, Tuple<List<int>, List<int>>> _nets;

        public event Action<int> IterFinishEvent;

        private int _countRealizedPinsBefore = 0;
        private int _countRealizedPinsAfter = 0;

        public RetraceAlgScheme(Configuration config, int countIter, int countRealizedPinsBefore, Dictionary<string, Tuple<List<int>, List<int>>> nets)
        {
            _config = config;
            _countIter = countIter;
            _nets = nets;
            _countRealizedPinsBefore = countRealizedPinsBefore;
            _countRealizedPinsAfter = countRealizedPinsBefore;
        }

        public long Calculate(out int countRealizedPinsAfter)
        {
            countRealizedPinsAfter = 0;

            Stopwatch sw = new Stopwatch();
            sw.Reset();
            sw.Start();

            List<int> allRealizedPins = new List<int>();
            for (int curIter = 0; curIter < _countIter; curIter++)
            {
                foreach (var subNet in _nets)
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
                                _goodRetracing.Add(nonRealizedPin);

                                _countRealizedPinsAfter++;
                                if (isFinishPin)
                                {
                                    _goodRetracing.Add(finish);
                                    _countRealizedPinsAfter++;
                                }
                            }
                        }
                    }

                    foreach (var pin in _goodRetracing)
                    {
                        subNet.Value.Item2.RemoveAll(x => x == pin);
                    }
                    allRealizedPins.AddRange(_goodRetracing);                    

                    _goodRetracing.Clear();
                }

                if (IterFinishEvent != null)
                {
                    IterFinishEvent.Invoke(curIter + 1);
                }
            }

            sw.Stop();

            countRealizedPinsAfter = _countRealizedPinsAfter;

            return sw.ElapsedMilliseconds;
        }
    }
}
