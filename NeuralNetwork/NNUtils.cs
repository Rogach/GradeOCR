using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeuralNetwork {
    public static class NNUtils {
        public static double[] ToNetworkInput(bool[] bits) {
            double[] res = new double[bits.Length];
            for (int q = 0; q < bits.Length; q++) {
                res[q] = bits[q] ? 1.0 : 0.0;
            }
            return res;
        }

        public static int Answer(double[] result) {
            int i = 0;
            double max = 0;
            for (int w = 0; w < result.Length; w++) {
                if (result[w] > max) {
                    max = result[w];
                    i = w;
                }
            }
            return i;
        }

        public static double AnswerConfidence(double[] result) {
            int i = Answer(result);
            double sum = 0;
            for (int q = 0; q < result.Length; q++) {
                if (i == q) {
                    sum += (1 - result[q]) * (1 - result[q]);
                } else {
                    sum += result[q] * result[q];
                }
            }
            return sum;
        }
    }
}
