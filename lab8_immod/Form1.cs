using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace lab8_immod
{
    public partial class Form1 : Form
    {
        private List<double> Statistics;
        private List<double> ProbList;
        private Dictionary<int, double> X;

        private double theory_chi = 11.070; 
        private const double e = 2.7182818284;

        private int N = 1;
        private Random random = new Random();
        private double p;

        public Form1()
        {
            InitializeComponent();
            Statistics = new List<double>();
            ProbList = new List<double>();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach (var series in chart1.Series)
                series.Points.Clear();

            ReadData();
            ModellingStatisticsCount();
            GetResults();
            CountStats();
        }

        void ReadData()
        {
            N = (int)numericUpDownN.Value;
            p = (double)numericUpDownParam.Value;

            double p_sum = 0.0;

            Statistics.Clear();
            ProbList.Clear();

            X = new Dictionary<int, double>(5);

            for (int i = 0; i < 4; i++)
            {
                double pi = Geom(i, p);
                p_sum += pi;
                ProbList.Add(pi);
            }

            double p_new_s = 0;
            for (int i = 0; i < 4; i++)
            {
                ProbList[i] /= p_sum;
                p_new_s += ProbList[i];
            }

            ProbList.Add(1 - p_new_s);

            //double sum = ProbList.Sum();
        }

        public double Geom(int m, double p)
        {
            return p * Math.Pow((1 - p), m);
        }

        void ModellingStatisticsCount()
        {
            X[1] = p;
            int i = 2;
            int last_i = 1;

            while (true)
            {
                double pi = (1 - p) * X[last_i];

                if (i > 6 || (i > 3 && X[last_i] >= pi / p))
                {
                    X[i] = pi / p;
                    last_i = i;
                    i++;
                    break;
                }
                else
                {
                    X[i] = pi;
                    last_i = i;
                    i++;
                }

                i++;
            }

            foreach (KeyValuePair<int, double> keyValue in X)
                Statistics.Add(keyValue.Value);

        }

        void GetResults()
        {
            for (int j = 0; j < Statistics.Count(); j++)
                chart1.Series[0].Points.AddXY(j + 1, Statistics[j]);
        }

        void CountStats()
        {
            double rme = CountRelativeMeanError() * 100;
            double rve = CountRelativeVarError() * 100;
            int rme_i = (int)rme;
            int rve_i = (int)rve;

            double chi_test_res = ChiSquaredTest();
            bool result = !(chi_test_res > theory_chi);

            labelAverage.Text = CountMean().ToString() + "(" + rme_i.ToString() + "%)";
            labelVariance.Text = CountVar().ToString() + "(" + rve_i.ToString() + "%)";
            labelX2.Text = chi_test_res.ToString() + " > " + theory_chi.ToString();
            labelRes.Text = result.ToString();
        }

        double CountAbsoluteMeanError()
        {
            return Math.Abs(CountEmpiricExpectation() - CountMean());
        }        

        double CountEmpiricExpectation()
        {
            double mean = 0.0;

            for (int x = 0; x < Statistics.Count; x++)
                mean += Statistics[x] * (x + 1);

            return mean;
        }

        double CountMean()
        {
            double mean = 0.0;

            for (int x = 0; x < ProbList.Count; x++)
                mean += ProbList[x] * (x + 1);

            return mean;
        }

        double CountAbsoluteVarError()
        {
            return Math.Abs(CountEmpiricVar() - CountVar());
        }

        double CountEmpiricVar()
        {
            double emp_mean_square = CountEmpiricExpectation();
            emp_mean_square *= emp_mean_square;

            double var = 0.0;

            for (int x = 0; x < Statistics.Count; x++)
                var += Statistics[x] * (x + 1) * (x + 1);

            var -= emp_mean_square;

            return var;
        }

        double CountVar()
        {
            double mean_square = CountMean();
            mean_square *= mean_square;

            double var = 0.0;

            for (int x = 0; x < ProbList.Count; x++)
                var += ProbList[x] * (x + 1) * (x + 1);

            var -= mean_square;

            return var;
        }

        double ChiSquaredTest()
        {
            double xi_square = 0;

            for (int x = 0; x < Statistics.Count; x++)
                xi_square += ((Statistics[x] - ProbList[x]) * (Statistics[x] - ProbList[x])) / ProbList[x];

            return xi_square *= N;
        }

        double CountRelativeMeanError()
        {
            return CountAbsoluteMeanError() / Math.Abs(CountMean());
        }

        double CountRelativeVarError()
        {
            return CountAbsoluteVarError() / Math.Abs(CountVar());
        }     
    }
}

