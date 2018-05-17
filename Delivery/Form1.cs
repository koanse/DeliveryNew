using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using NPlot;
using System.IO;

namespace Delivery
{
    public partial class Form1 : Form
    {
        double[] arrT, arrQ;
        double cStSum, cDlSum, QSum;
        public Form1()
        {
            InitializeComponent();
            tbExpr.Text = "t";
            tbN.Text = "3";
            tbTMax.Text = "10";
            tbCSt.Text = "1";
            tbCDl.Text = "1";
            dgvTQ.Columns[0].ValueType = typeof(double);
            dgvTQ.Columns[1].ValueType = typeof(double);
            dgvFn.Columns[0].ValueType = typeof(double);
            dgvFn.Columns[1].ValueType = typeof(double);
        }        
        void doCalcToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string expr = tbExpr.Text;
                double tMax = double.Parse(tbTMax.Text);
                int n = int.Parse(tbN.Text);
                double cSt = double.Parse(tbCSt.Text);
                double cDl = double.Parse(tbCDl.Text);
                double[] arrTFunc = null, arrVFunc = null;
                DeliveryOptimizer opt;
                if (rbAn.Checked)
                    opt = new DeliveryOptimizer(expr, tMax, n, cSt, cDl);
                else
                {
                    arrTFunc = new double[dgvFn.RowCount - 1];
                    arrVFunc = new double[dgvFn.RowCount - 1];
                    for (int i = 0; i < dgvFn.RowCount - 1; i++)
                    {
                        arrTFunc[i] = (double)dgvFn.Rows[i].Cells[0].Value;
                        arrVFunc[i] = (double)dgvFn.Rows[i].Cells[1].Value;
                    }
                    opt = new DeliveryOptimizer(arrTFunc, arrVFunc, n, cSt, cDl);
                }
                double[] arrTPrev = null, arrV = null;
                for (int i = 0; i < 10; i++)
                {
                    opt.Optimize(arrTPrev, out arrT, out arrQ, out arrV,
                        out cStSum, out cDlSum);
                    arrTPrev = arrT;
                }
                opt.GetTheorFunc(out arrTFunc, out arrVFunc);
                ps.Clear();
                LinePlot lp = new LinePlot();
                lp.AbscissaData = arrT;
                lp.OrdinateData = arrV;
                ps.Add(lp);
                LinePlot lpFunc = new LinePlot();
                lpFunc.AbscissaData = arrTFunc;
                lpFunc.OrdinateData = arrVFunc;
                lpFunc.Pen = Pens.Red;
                ps.Add(lpFunc);
                ps.XAxis1.Label = "t";
                ps.YAxis1.Label = "V(t)";
                ps.Refresh();

                dgvTQ.Rows.Clear();
                dgvTQ.RowCount = n;
                for (int i = 0; i < n; i++)
                {
                    dgvTQ.Rows[i].Cells["t"].Value = arrT[i];
                    dgvTQ.Rows[i].Cells["Q"].Value = arrQ[i];
                }
                tbCStSum.Text = cStSum.ToString();
                tbCDlSum.Text = cDlSum.ToString();
                QSum = 0;
                for (int i = 0; i < arrQ.Length; i++)
                    QSum += arrQ[i];
                tbQSum.Text = QSum.ToString();
            }
            catch
            {
                MessageBox.Show("Проверьте исходные данные");
            }
        }
        void saveRepToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (saveFileDialog1.ShowDialog() != DialogResult.OK)
                    return;
                FileStream fs = new FileStream(saveFileDialog1.FileName, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);
                string tAll = "", QAll = "";
                for (int i = 0; i < arrT.Length; i++)
                    tAll += arrT[i].ToString() + " ";
                for (int i = 0; i < arrQ.Length; i++)
                    QAll += arrQ[i].ToString() + " ";
                sw.Write("Однопродуктовая детерминированная задача " +
                    "управления запасами: (n) - политика\r\n" +
                    "Моменты заказов: {0}\r\n" +
                    "Размеры заказов: {1}\r\n" +
                    "Суммарные затраты на хранение: {2}\r\n" +
                    "Суммарные затраты на доставку: {3}\r\n" +
                    "Общий объем поставок: {4}\r\n",
                    tAll, QAll, cStSum, cDlSum, QSum);
                sw.Close();
            }
            catch
            {
                MessageBox.Show("Ошибка сохранения отчета");
            }
        }
        void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}