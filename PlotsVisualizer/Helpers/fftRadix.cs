//using System;
//using System.Numerics;

//namespace PlotsVisualizer.Helpers
//{
//    class fftRadix
//    {
//        public void CalcSubFFT(Complex[] a, int n, int lo)
//        {
//            int i, m;
//            Complex w;
//            Complex v;
//            Complex h;
//            if (n > 1)
//            {
//                Shuffle(a, n, lo);  // separate odd from even elements
//                m = n / 2;
//                CalcSubFFT(a, m, lo);  //recursive call for u(k)
//                CalcSubFFT(a, m, lo + m); //recursive call for v(k)
//                for (i = lo; i < lo + m; i++)
//                {
//                    w.Real = Math.Cos(2.0 * Math.PI * (double)(i - lo) / (double)(n));
//                    w.Imaginary = Math.Sin(2.0 * Math.PI * (double)(i - lo) / (double)(n));
//                    h = a[i + m] * w;
//                    v = a[i];
//                    a[i] = a[i] + h);
//                a[i + m] = v - h;
//            }
//        }

//        public void Shuffle(Complex[] a, int n, int lo)
//        {
//            if (n > 2)
//            {
//                int i, m = n / 2;
//                Complex[] b = new Complex[m];
//                for (i = 0; i < m; i++)
//                    b[i] = a[i * 2 + lo + 1];
//                for (i = 0; i < m; i++)
//                    a[i + lo] = a[i * 2 + lo];
//                for (i = 0; i < m; i++)
//                    a[i + lo + m] = b[i];
//            }
//        }

//        public void CalcFFT()
//        {
//            int i;
//            CalcSubFFT(y, N, 0);
//            for (i = 0; i < N; i++)
//            {
//                y[i].imag = y[i].imag / (double)N * 2.0;
//                y[i].real = y[i].real / (double)N * 2.0;
//            }
//            y[0].imag = y[0].imag / 2.0;
//            y[0].real = y[0].real / 2.0;
//        }
//    }
//}
