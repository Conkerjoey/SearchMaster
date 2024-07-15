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

        public static int[] NonZero(double[] vec)
        {
            int[] nz = new int[vec.Length];
            for (int i = 0; i < vec.Length; i++)
            {
                nz[i] = vec[i] != 0 ? 1 : 0;
            }
            return nz;
        }

        public static double[] Add(double[] v1, double[] v2)
        {
            double[] res = new double[v1.Length];
            for (int i = 0; i < v1.Length; i++)
            {
                res[i] = v1[i] + v2[i];
            }
            return res;
        }

        public static int[] Add(int[] v1, int[] v2)
        {
            int[] res = new int[v1.Length];
            for (int i = 0; i < v1.Length; i++)
            {
                res[i] = v1[i] + v2[i];
            }
            return res;
        }

        public static double[,] Add(double[,] mat, double scalar)
        {
            double[,] res = new double[mat.GetLength(0), mat.GetLength(1)];
            for (int i = 0; i < mat.GetLength(0); i++)
            {
                for (int j = 0; j < mat.GetLength(1); j++)
                {
                    res[i, j] = mat[i, j] + scalar;
                }
            }
            return res;
        }

        public static double[,] Add(double[,] mat1, double[,] mat2)
        {
            double[,] res = new double[mat1.GetLength(0), mat1.GetLength(1)];
            for (int i = 0; i < mat1.GetLength(0); i++)
            {
                for (int j = 0; j < mat1.GetLength(1); j++)
                {
                    res[i, j] = mat1[i, j] + mat2[i, j];
                }
            }
            return res;
        }

        public static double[] Ones(int length)
        {
            double[] res = new double[length];
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = 1;
            }
            return res;
        }

        public static double[] Times(double[] v, double scalar)
        {
            double[] res = new double[v.Length];
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = v[i] * scalar;
            }
            return res;
        }

        public static double[] Times(double[] v1, double[] v2)
        {
            double[] res = new double[v1.Length];
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = v1[i] * v2[i];
            }
            return res;
        }

        public static double[,] Times(double[,] mat, double[] v, int axis)
        {
            double[,] res = new double[mat.GetLength(0), mat.GetLength(1)];
            for (int i = 0; i < v.Length; i++)
            {
                if (axis == 0)
                {
                    for (int j = 0; j < mat.GetLength(1); j++)
                    {
                        res[i, j] = mat[i, j] * v[i];
                    }
                }
                if (axis == 1)
                {
                    for (int j = 0; j < mat.GetLength(0); j++)
                    {
                        res[j, i] = mat[j, i] * v[i];
                    }
                }
            }
            return res;
        }

        public static double[,] Times(double[,] mat, double scalar)
        {
            double[,] res = new double[mat.GetLength(0), mat.GetLength(1)];
            for (int i = 0; i < mat.GetLength(0); i++)
            {
                for (int j = 0; j < mat.GetLength(1); j++)
                {
                    res[i, j] = mat[i, j] * scalar;
                }
            }
            return res;
        }

        public static double[,] Divide(double[,] mat, double scalar)
        {
            double[,] res = new double[mat.GetLength(0), mat.GetLength(1)];
            for (int i = 0; i < mat.GetLength(0); i++)
            {
                for (int j = 0; j < mat.GetLength(1); j++)
                {
                    res[i, j] = mat[i, j] / scalar;
                }
            }
            return res;
        }

        public static double[,] Divide(double[,] mat1, double[,] mat2)
        {
            double[,] res = new double[mat1.GetLength(0), mat1.GetLength(1)];
            for (int i = 0; i < mat1.GetLength(0); i++)
            {
                for (int j = 0; j < mat1.GetLength(1); j++)
                {
                    res[i, j] = mat1[i, j] / mat2[i, j];
                }
            }
            return res;
        }

        public static double[] Divide(double[] v1, double[] v2)
        {
            double[] res = new double[v1.Length];
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = v1[i] / v2[i];
            }
            return res;
        }

        public static double[] Divide(double[] v1, int[] v2)
        {
            double[] res = new double[v1.Length];
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = v1[i] / v2[i];
            }
            return res;
        }

        public static double[] Divide(double[] v, double scalar)
        {
            double[] res = new double[v.Length];
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = v[i] / scalar;
            }
            return res;
        }

        public static double[] Apply(Func<double, double> func, double[] v)
        {
            double[] res = new double[v.Length];
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = func(v[i]);
            }
            return res;
        }

        public static double[] Apply(Func<double[], double> func, double[,] mat, int axis)
        {
            double[] res = new double[mat.GetLength(1 - axis)];

            for (int i = 0; i < res.Length; i++)
            {
                double[] temp = new double[mat.GetLength(axis)];
                for (int j = 0; j < temp.Length; j++)
                {
                    if (axis == 0)
                        temp[j] = mat[j, i];
                    if (axis == 1)
                        temp[j] = mat[i, j];
                }
                res[i] = func(temp);
            }
            return res;
        }

        public static double Max(double[] v)
        {
            double max = double.MinValue;
            for (int i = 0; i < v.Length; i++)
            {
                max = Math.Max(max, v[i]);
            }
            return max;
        }

        public static double Min(double[] v)
        {
            double max = double.MaxValue;
            for (int i = 0; i < v.Length; i++)
            {
                max = Math.Min(max, v[i]);
            }
            return max;
        }

        public static double Sum(double[] v)
        {
            double sum = 0;
            for (int i = 0; i < v.Length; i++)
            {
                sum += v[i];
            }
            return sum;
        }
    }
}
