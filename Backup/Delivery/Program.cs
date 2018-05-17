using System;
using System.Collections.Generic;
using System.Windows.Forms;
using muWrapper;
using NPlot;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics;

namespace Delivery
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
            //DeliveryOptimizer opt = new DeliveryOptimizer(new double[] { 2, 3, 5, 2, 1 }, 5);
            DeliveryOptimizer opt = new DeliveryOptimizer("ln(x + 10)", 10, 3);
            double[] arrT, arrQ, arrV;
            opt.Optimize(out arrT, out arrQ, out arrV);
        }
    }
    class DeliveryOptimizer
    {
        double[] arrW;
        double tMax;
        int n;
        public DeliveryOptimizer(double[] arrW, double tMax)
        {
            this.arrW = arrW;
            this.tMax = tMax;
            this.n = arrW.Length;
        }
        public DeliveryOptimizer(string expr, double tMax, int n)
        {
            this.tMax = tMax;
            this.n = n;
            Parser prs = new Parser();
            ParserVariable pvX = new ParserVariable();
            prs.DefineVar("x", pvX);
            prs.SetExpr(expr);
            double tStep = tMax / n;
            arrW = new double[n];
            for (int i = 0; i < arrW.Length; i++)
            {
                pvX.Value = tStep * i;
                double f1 = prs.Eval();
                pvX.Value = tStep * (i + 1);
                double f2 = prs.Eval();
                arrW[i] = (f2 - f1) / tStep;                
            }
        }
        public void Optimize(out double[] arrT, out double[] arrQ, out double[] arrV)
        {
            double[,] matrA = new double[n + 1, n + 1];
            // первые n - 2 уравнений
            for (int i = 0; i < n - 2; i++)
            {
                matrA[i, i] += -arrW[i];
                matrA[i, i + 1] += arrW[i] + arrW[i + 1];
                matrA[i, i + 2] += -arrW[i + 1];
            }
            // (n - 1)-e уравнение
            matrA[n - 2, n - 2] += -arrW[n - 2];
            matrA[n - 2, n - 1] += arrW[n - 1];
            matrA[n - 2, n] = 1;
            // ур. для t0
            matrA[n - 1, 0] = 1;
            // ур. для V(tn)
            matrA[n, n - 1] = -arrW[n - 1];
            matrA[n, n] = -1;            
            for (int i = 0; i < n - 1; i++)
            {
                matrA[n, i] += -arrW[i];
                matrA[n, i + 1] += arrW[i];                
            }
            Matrix mA = Matrix.Create(matrA);

            double[,] matrB = new double[n + 1, 1];
            matrB[0, 0] = arrW[0];
            matrB[n, 0] = -tMax * arrW[n - 1];
            Matrix mB = Matrix.Create(matrB);
            Matrix mT = mA.Inverse() * mB;
            arrT = new double[n];
            for (int i = 0; i < n; i++)
                arrT[i] = mT[i, 0];

            arrQ = new double[n];
            for (int i = 0; i < arrQ.Length - 1; i++)
                arrQ[i] = arrW[i] * (arrT[i + 1] - arrT[i]);
            arrQ[arrQ.Length - 1] = arrW[arrW.Length - 1] *
                (tMax - arrT[arrT.Length - 1]);

            arrV = new double[n + 1];
            for (int i = 0; i < arrV.Length - 1; i++)
                arrV[i + 1] = arrV[i] + arrQ[i];
        }
    }
}