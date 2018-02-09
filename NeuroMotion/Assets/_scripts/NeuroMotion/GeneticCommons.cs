using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeuroMotion {

    public static class GeneticCrossover {

        public static void SinglePoint(List<float> p0, List<float> p1, ref List<float> c0, ref List<float> c1) {
            int point = Random.Range(0, p0.Count);

            for (int i = 0; i < point; i++) {
                c0.Add(p0[i]);
                c1.Add(p1[i]);
            }
            for (int i = point; i < p0.Count; i++) {
                c0.Add(p1[i]);
                c1.Add(p0[i]);
            }
        }

        public static void TwoPoint(List<float> p0, List<float> p1, ref List<float> c0, ref List<float> c1) {
            int point0 = Random.Range(0, p0.Count);

            int point1 = point0;
            while (point1 == point0)
                point1 = Random.Range(0, p0.Count);

            if (point1 < point0) {
                int temp = point1;
                point1 = point0;
                point0 = temp;
            }

            for (int i = 0; i < point0; i++) {
                c0.Add(p0[i]);
                c1.Add(p1[i]);
            }
            for (int i = point0; i < point1; i++) {
                c0.Add(p1[i]);
                c1.Add(p0[i]);
            }
            for (int i = point1; i < p0.Count; i++) {
                c0.Add(p0[i]);
                c1.Add(p1[i]);
            }
        }

        public static void Uniform(List<float> p0, List<float> p1, ref List<float> c0, ref List<float> c1) {
            for (int i = 0; i < p0.Count; i++) {
                if (Random.value > 0.5f) {
                    c0.Add(p0[i]);
                    c1.Add(p1[i]);
                }
                else {
                    c0.Add(p1[i]);
                    c1.Add(p0[i]);
                }
            }
        }
    }

    public static class GeneticMutation {

        public static void Perturb(ref List<float> genome, float rate) {
            for (int i = 0; i < genome.Count; i++) {
                genome[i] += Random.Range(-1.0f, 1.0f) * rate;
            }
        }

        public static void Invert(ref List<float> genome, float rate) {
            for (int i = 0; i < genome.Count; i++) {
                if (Random.value <= rate) {
                    genome[i] = -genome[i];
                }
            }
        }

        public static void Randomize(ref List<float> genome, float rate) {
            for (int i = 0; i < genome.Count; i++) {
                if (Random.value <= rate) {
                    genome[i] = Random.Range(-1.0f, 1.0f);
                }
            }
        }
    }

}
