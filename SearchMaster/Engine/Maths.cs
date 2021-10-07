using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchMaster.Engine
{
    public class Maths
    {

        public static double Dot(double[] v1, double[] v2)
        {
            if (v1.Length != v2.Length)
            {
                throw new ArgumentException("The two vectors size should be equal.");
            }
            double acc = 0;
            for (int i = 0; i < v1.Length; i++)
            {
                acc += v1[i] * v2[i];
            }
            return acc;
        }

        public static double Magnitude(double[] vec)
        {
            double mag = 0;
            for (int i = 0; i < vec.Length; i++)
            {
                mag += vec[i] * vec[i];
            }
            return Math.Sqrt(mag);
        }
    }
}
