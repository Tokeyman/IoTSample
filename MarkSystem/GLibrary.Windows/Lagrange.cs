using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLibrary.Windows
{
    public class Lagrange
    {

        public double[] x { get; set; }
        public double[] y { get; set; }

        public int itemNum { get; set; }



        public Lagrange(double[] x, double[] y)
        {
            this.x = x;
            this.y = y;
            this.itemNum = x.Length;
        }

        public double GetValue(double xValue)
        {
            int start, end;
            double value = 0.0;

            if (itemNum < 1) { return value; }
            if (itemNum == 1) { value = y[0]; return value; }
            if (itemNum == 2)
            {
                value = (y[0] * (xValue - x[1]) - y[1] * (xValue - x[0])) / (x[0] - x[1]);
                return value;
            }

            if (xValue < x[1]) { start = 0; end = 2; }
            else if (xValue > x[itemNum - 2]) { start = itemNum - 3; end = itemNum - 1; }
            else
            {
                start = 1; end = itemNum;
                int temp;

                while ((end - start) != 1)
                {
                    temp = (start + end) / 2;
                    if (xValue < x[temp - 1])
                        end = temp;
                    else
                        start = temp;
                }
                start--;
                end--;

                if (Math.Abs(xValue - x[start]) < Math.Abs(xValue - x[end]))
                    start--;
                else
                    end++;
            }

            double valueTemp;

            for (int i = start; i <= end; i++)
            {
                valueTemp = 1.0;
                for (int j = start; j <= end; j++)
                    if (j != i)

                        valueTemp *= (double)(xValue - x[j]) / (double)(x[i] - x[j]);
                value += valueTemp * y[i];

            }
            return value;
        }


    }
}
