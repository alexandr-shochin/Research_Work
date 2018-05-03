using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TracProg.Calculation.Algorithms
{
    public class RetraceAlgScheme
    {
        private readonly Configuration _config;
        private readonly int _countIter;

        private readonly Dictionary<string, Tuple<List<int>, List<int>>> _nets;

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

            int curIter = 0;
            bool isRetracePrevStep = true;
            while(isRetracePrevStep)
            {
                isRetracePrevStep = false;
                foreach (var subNet in _nets)
                {
                    string metalId = subNet.Key;
                    List<int> allPins = subNet.Value.Item1;
                    List<int> nonRealizedPins = subNet.Value.Item2;

                    List<int> goodRetracing = new List<int>();
                    foreach (var nonRealizedPin in nonRealizedPins)
                    {
                        if (!goodRetracing.Contains(nonRealizedPin))
                        {
                            RetraceAlgNet alg = new RetraceAlgNet(_config.Grid, metalId, allPins);
                            int finish;
                            bool isFinishPin;
                            if (alg.FindPath(nonRealizedPin, out finish, out isFinishPin))
                            {
                                isRetracePrevStep = true;
                                goodRetracing.Add(nonRealizedPin);

                                _countRealizedPinsAfter++;
                                if (isFinishPin)
                                {
                                    goodRetracing.Add(finish);
                                    _countRealizedPinsAfter++;
                                }
                            }
                        }
                    }

                    foreach (var pin in goodRetracing)
                    {
                        subNet.Value.Item2.RemoveAll(x => x == pin);
                    }             
                    goodRetracing.Clear();
                }

                if (IterFinishEvent != null)
                {
                    IterFinishEvent.Invoke(curIter + 1);
                }

                curIter++;
            }

            sw.Stop();

            countRealizedPinsAfter = _countRealizedPinsAfter;

            return sw.ElapsedMilliseconds;
        }
    }
}
