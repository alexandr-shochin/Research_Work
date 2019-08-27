using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TracProg.Calculation.Algorithms
{
    public class RetraceAlgScheme
    {
        private readonly Configuration _config;

        private readonly Dictionary<string, Tuple<List<int>, List<int>>> _nets;

        private int _countRealizedPinsAfter = 0;

        public RetraceAlgScheme(Configuration config, int countRealizedPinsBefore, Dictionary<string, Tuple<List<int>, List<int>>> nets)
        {
            _config = config;
            _nets = nets;
            _countRealizedPinsAfter = countRealizedPinsBefore;
        }

        public long Calculate(out int countRealizedPinsAfter, out int countIter)
        {
            countRealizedPinsAfter = 0;

            Stopwatch sw = new Stopwatch();
            sw.Reset();
            sw.Start();

            countIter = -1;
            bool isRetracePrevStep = true;
            while(isRetracePrevStep)
            {
                countIter++;
                isRetracePrevStep = false;
                foreach (var subNet in _nets)
                {
                    string metalId = subNet.Key;
                    List<int> allPins = subNet.Value.Item1;
                    List<int> nonRealizedPins = subNet.Value.Item2;

                    List<int> goodRetracing = new List<int>();
                    foreach (int nonRealizedPin in nonRealizedPins)
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

                    foreach (int pin in goodRetracing)
                    {
                        subNet.Value.Item2.RemoveAll(x => x == pin);
                    }             
                    goodRetracing.Clear();
                }
            }

            sw.Stop();

            countRealizedPinsAfter = _countRealizedPinsAfter;

            return sw.ElapsedMilliseconds;
        }
    }
}
