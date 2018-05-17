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
            Application.Run(new Form1());
            //DeliveryOptimizer opt = new DeliveryOptimizer(new double[] { 2, 3, 5, 2, 1 }, 5);
            //DeliveryOptimizer opt = new DeliveryOptimizer("ln(x + 10)", 10, 3);
            //double[] arrT, arrQ, arrV;
            //opt.Optimize(out arrT, out arrQ, out arrV);
        }
    }
    class DeliveryOptimizer
    {
        double[] arrW, arrTFunc, arrVFunc;
        double tMax, cSt, cDl;
        int n;
        Parser prs;
        ParserVariable pvT;
        public DeliveryOptimizer(double[] arrTFunc, double[] arrVFunc,
            int n, double cSt, double cDl)
        {
            this.arrTFunc = arrTFunc;
            this.arrVFunc = arrVFunc;
            this.tMax = arrTFunc[arrTFunc.Length - 1];
            this.n = n;
            this.cSt = cSt;
            this.cDl = cDl;
            for (int i = 0; i < arrVFunc.Length; i++)
                arrVFunc[i] -= arrVFunc[0];
        }
        public DeliveryOptimizer(string expr, double tMax,
            int n, double cSt, double cDl)
        {
            this.tMax = tMax;
            this.n = n;
            this.cSt = cSt;
            this.cDl = cDl;
            prs = new Parser();
            pvT = new ParserVariable();
            prs.DefineVar("t", pvT);
            prs.SetExpr(expr);

            int nGrid = 100;
            double tStep = tMax / nGrid;
            pvT.Value = 0;
            double f0 = prs.Eval();
            arrTFunc = new double[nGrid + 1];
            arrVFunc = new double[nGrid + 1];
            for (int i = 0; i < nGrid + 1; i++)
            {
                arrTFunc[i] = i * tStep;
                pvT.Value = arrTFunc[i];
                arrVFunc[i] = prs.Eval() - f0;
            }
        }
        public void Optimize(double[] arrTPrev,
            out double[] arrT, out double[] arrQ, out double[] arrV,
            out double cStSum, out double cDlSum)
        {
            CalcArrW(arrTPrev);
            double[,] matrA = new double[n + 1, n + 1];
            for (int i = 0; i < n - 1; i++)
            {
                matrA[i, i] += -arrW[i];
                matrA[i, i + 1] += arrW[i] + arrW[i + 1];
                matrA[i, i + 2] += -arrW[i + 1];
            }
            // ур. для t0
            matrA[n - 1, 0] = 1;
            // ур. для tn
            matrA[n, n] = 1;
            Matrix mA = Matrix.Create(matrA);

            double[,] matrB = new double[n + 1, 1];
            matrB[n, 0] = tMax;
            Matrix mB = Matrix.Create(matrB);
            Matrix mT = mA.Inverse() * mB;
            arrT = new double[n + 1];
            for (int i = 0; i < n + 1; i++)
                arrT[i] = mT[i, 0];

            arrQ = new double[n];
            for (int i = 0; i < n; i++)
                arrQ[i] = arrW[i] * (arrT[i + 1] - arrT[i]);

            arrV = new double[n + 1];
            for (int i = 0; i < n; i++)
                arrV[i + 1] = arrV[i] + arrQ[i];

            // расчет затрат
            cStSum = 0;
            for (int i = 0; i < n; i++)
                cStSum += 0.5 * arrQ[i] *
                    (arrT[i + 1] - arrT[i]);
            cStSum *= cSt;
            cDlSum = n * cDl;
        }
        public void GetTheorFunc(out double[] arrTFunc, out double[] arrVFunc)
        {
            arrTFunc = this.arrTFunc;
            arrVFunc = this.arrVFunc;
        }
        void CalcArrW(double[] arrT)
        {
            arrW = new double[n];
            if (arrT == null)
            {
                arrT = new double[n + 1];
                double tStep = tMax / n;
                for (int i = 0; i < n + 1; i++)
                    arrT[i] = i * tStep;
            }
            for (int i = 0; i < n; i++)
                arrW[i] = (GetFuncV(arrT[i + 1]) - GetFuncV(arrT[i])) /
                    (arrT[i + 1] - arrT[i]);
        }
        double GetFuncV(double t)
        {
            if (prs != null)
            {
                pvT.Value = t;
                return prs.Eval();
            }
            int i;
            for (i = 1; i < arrTFunc.Length - 1; i++)
                if (t < arrTFunc[i])
                    break;
            return (t - arrTFunc[i - 1]) * (arrVFunc[i] - arrVFunc[i - 1]) /
                    (arrTFunc[i] - arrTFunc[i - 1]) + arrVFunc[i - 1];
        }        
    }
}