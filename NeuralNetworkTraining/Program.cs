using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GradeOCR;
using LibUtil;
using NeuralNetwork;

namespace NeuralNetworkTraining {
    public static class Program {
        public static List<int> inputCodes = new List<int> { 2, 3, 4, 5 };

        static void Main(string[] args) {
            MultiLayerNetwork nw = new MultiLayerNetwork(GradeDigest.dataSize, new int[] { 100, 4 });

            List<GradeDigest> trainDigests = GradeDigestSet.staticInstance.GetDigestList();
            List<GradeDigest> testDigests = GradeDigestSet.Read("e:/Pronko/prj/Grader/ocr-data/test-data/grade-digests.db").GetDigestList();

            for (int trainingRun = 1; trainingRun <= 100; trainingRun++) {
                Util.Timed(String.Format("training run #{0}", trainingRun), () => {
                    int c = 0;
                    foreach (var gd in trainDigests.Shuffle()) {
                        double[] desiredOutput = new double[4];
                        desiredOutput[inputCodes.IndexOf(gd.grade)] = 1;
                        nw.LearnNW(NNUtils.ToNetworkInput(GradeDigest.UnpackBits(gd.data)), desiredOutput, 0.1);
                        c++;
                    }
                });

                TestNN(nw, testDigests, trainingRun);
            }
        }

        static void TestNN(MultiLayerNetwork nw, List<GradeDigest> testDigests, int run) {
            List<Tuple<bool, double>> results = new List<Tuple<bool, double>>();

            foreach (var gd in testDigests) {
                double[] output = new double[4];
                nw.NetOUT(NNUtils.ToNetworkInput(GradeDigest.UnpackBits(gd.data)), out output);

                int ans = inputCodes[NNUtils.Answer(output)];
                double certainity = NNUtils.AnswerConfidence(output);
                results.Add(new Tuple<bool, double>(ans == gd.grade, certainity));
            }

            double confidenceThreshold = results.Select(t => t.Item2).OrderBy(x => x).ElementAt((int) Math.Floor(results.Count * 0.95));
            confidenceThreshold = 0.0001;

            int testSuccess = 0;
            int testFailure = 0;

            int sureTestSuccess = 0;
            int sureTestFailure = 0;
            int unsure = 0;

            foreach (var res in results) {
                if (res.Item1) {
                    testSuccess++;
                } else {
                    testFailure++;
                }

                if (res.Item2 > confidenceThreshold) {
                    unsure++;
                } else if (res.Item1) {
                    sureTestSuccess++;
                } else {
                    sureTestFailure++;
                }
            }


            Func<int, double> perc = x => ((double) x / testDigests.Count * 100);

            Console.WriteLine("Test results (r/w%): {0:F2}/{1:F2}", perc(testSuccess), perc(testFailure));
            Console.WriteLine("Test results (r/u/w%): {0:F2}/{1:F2}/{2:F2} (confidence threshold = {3})",
                perc(sureTestSuccess), perc(unsure), perc(sureTestFailure), confidenceThreshold);

            nw.SaveNW(String.Format("e:/Pronko/prj/Grader/ocr-data/grade-recognition_{0}_{1:F2}_{2:F2}.nn",
                run, perc(testSuccess), perc(sureTestFailure)));
        }

        
    }
}
