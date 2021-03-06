﻿using System;
using System.Globalization;
using System.Diagnostics;

namespace PhaseOpt
{
    public class PhaseOpt
    {
        private const double Bara_To_Barg = 1.01325;
        private const double Kelvin_To_Celcius = 273.15;

        public double[] Composition_Values;
        public Int32[]  Composition_IDs;

        public PhaseOpt()
        { }
        public PhaseOpt(Int32[] IDs, double[] Values)
        {
            Composition_IDs = IDs;
            Composition_Values = Values;
        }

        /// <summary>
        /// Calculates the density and the compressibility factor, at given pressure and temperature, of the composition.
        /// </summary>
        /// <param name="P">Pressure [bara]</param>
        /// <param name="T">Temperature [K]</param>
        /// <returns>An array containing {Vapour density, Liquid density, Vapour compressibility factor, Liquid compressibility factor}.
        /// If there is only one phase the values for the non existing phase will be -1.</returns>
        public double[] Calculate_Density_And_Compressibility(double P = 1.01325, double T = 288.15)
        {
            double[] Results = new double[4];
            string Arguments = "-dens -id";
            string output;
            Process umrol = new Process();

            foreach (int i in Composition_IDs)
            {
                Arguments += " " + i.ToString();
            }
            Arguments += " -z";
            foreach (double v in Composition_Values)
            {
                // Make the value string into a Fortran style double presicion literal
                string val = " " + v.ToString("G", CultureInfo.InvariantCulture).Replace('E', 'D');
                if (!val.Contains("D"))
                    val += "D0";

                Arguments += val;
            }
            Arguments += " -p " + P.ToString("G", CultureInfo.InvariantCulture) + "D0";
            Arguments += " -t " + T.ToString("G", CultureInfo.InvariantCulture) + "D0";

            umrol.StartInfo.UseShellExecute = false;
            umrol.StartInfo.RedirectStandardOutput = true;
            umrol.StartInfo.FileName = "umrol.exe";
            umrol.StartInfo.Arguments = Arguments;
            umrol.Start();
            umrol.PriorityClass = ProcessPriorityClass.Idle;
            output = umrol.StandardOutput.ReadToEnd();
            umrol.WaitForExit();

            Results[0] = Convert.ToDouble(output.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0], CultureInfo.InvariantCulture);
            Results[1] = Convert.ToDouble(output.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1], CultureInfo.InvariantCulture);
            Results[2] = Convert.ToDouble(output.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[2], CultureInfo.InvariantCulture);
            Results[3] = Convert.ToDouble(output.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[3], CultureInfo.InvariantCulture);

            return Results;
        }

        /// <summary>
        /// Calculates the cricondenbar point of the composition.
        /// </summary>
        /// <param name="Units">Sets the engineering units of the outputs. 0: use bara and Kelvin. 1: use barg and ­°C.</param>
        /// <returns>An array containing the cricondenbar pressure and temperature.</returns>
        public double[] Cricondenbar(uint Units = 1)
        {
            double[] Results = new double[2];
            double CCBT = -1.0;
            double CCBP = -1.0;
            string Arguments = "-ccd -ind 2 -id";
            string output = "";
            Process umrol = new Process();

            if (Units > 1) Units = 1;

            foreach (int i in Composition_IDs)
            {
                Arguments += " " + i.ToString();
            }
            Arguments += " -z";
            foreach (double v in Composition_Values)
            {
                // Make the value string into a Fortran style double presicion literal
                string val = " " + v.ToString("G", CultureInfo.InvariantCulture).Replace('E', 'D');
                if (!val.Contains("D"))
                    val += "D0";

                Arguments += val;
            }

            umrol.StartInfo.UseShellExecute = false;
            umrol.StartInfo.RedirectStandardOutput = true;
            umrol.StartInfo.FileName = "umrol.exe";
            umrol.StartInfo.Arguments = Arguments;
            try
            {
                umrol.Start();
                umrol.PriorityClass = ProcessPriorityClass.Idle;
                output = umrol.StandardOutput.ReadToEnd();
                umrol.WaitForExit();
            }
            catch
            {
                CCBP = -1.0;
                CCBT = -1.0;
            }

            if (output.Length > 0)
            {
                CCBP = Convert.ToDouble(output.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0], CultureInfo.InvariantCulture);
                CCBT = Convert.ToDouble(output.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1], CultureInfo.InvariantCulture);
            }

            if (CCBP < 0.0) CCBP = double.NaN;
            if (CCBT < 0.0) CCBT = double.NaN;

            Results[0] = (CCBP - (Units * Bara_To_Barg));
            Results[1] = (CCBT - (Units * Kelvin_To_Celcius));

            return Results;
        }

        /// <summary>
        /// Calculates the cricondentherm point of the composition.
        /// </summary>
        /// <param name="Units">Sets the engineering units of the outputs. 0: use bara and Kelvin. 1: use barg and ­°C.</param>
        /// <returns>An array containing the cricondentherm pressure and temperature.</returns>
        public double[] Cricondentherm(uint Units = 1)
        {
            // Calculate the cricondentherm point
            double CCTT = -1.0;
            double CCTP = -1.0;
            double[] Results = new double[2];
            string Arguments = "-ccd -ind 1 -id";
            string output = "";
            Process umrol = new Process();

            if (Units > 1) Units = 1;

            foreach (int i in Composition_IDs)
            {
                Arguments += " " + i.ToString();
            }
            Arguments += " -z";
            foreach (double v in Composition_Values)
            {
                // Make the value string into a Fortran style double presicion literal
                string val = " " + v.ToString("G", CultureInfo.InvariantCulture).Replace('E', 'D');
                if (!val.Contains("D"))
                    val += "D0";

                Arguments += val;
            }

            umrol.StartInfo.UseShellExecute = false;
            umrol.StartInfo.RedirectStandardOutput = true;
            umrol.StartInfo.FileName = "umrol.exe";
            umrol.StartInfo.Arguments = Arguments;
            umrol.Start();
            umrol.PriorityClass = ProcessPriorityClass.Idle;
            output = umrol.StandardOutput.ReadToEnd();
            umrol.WaitForExit();

            if (output.Length > 0)
            {
                CCTP = Convert.ToDouble(output.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0], CultureInfo.InvariantCulture);
                CCTT = Convert.ToDouble(output.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1], CultureInfo.InvariantCulture);
            }

            if (CCTP < 0.0) CCTP = double.NaN;
            if (CCTT < 0.0) CCTT = double.NaN;

            Results[0] = (CCTP - (Units * Bara_To_Barg));
            Results[1] = (CCTT - (Units * Kelvin_To_Celcius));

            return Results;
        }


        /// <summary>
        /// Calculates the liquid dropout of the composition at given pressure and temperature.
        /// </summary>
        /// <param name="P">Pressure [bara]</param>
        /// <param name="T">Temperature [K]</param>
        /// <returns>An array containing the liquid dropout mass and volume fractions.</returns>
        public double[] Dropout(double P, double T)
        {
            double[] Results = new double[2] { double.NaN, double.NaN };
            string Arguments = "-vpl -id";
            string output = "";
            Process umrol = new Process();

            foreach (int i in Composition_IDs)
            {
                Arguments += " " + i.ToString();
            }
            Arguments += " -z";
            foreach (double v in Composition_Values)
            {
                // Make the value string into a Fortran style double presicion literal
                string val = " " + v.ToString("G", CultureInfo.InvariantCulture).Replace('E', 'D');
                if (!val.Contains("D"))
                    val += "D0";

                Arguments += val;
            }
            Arguments += " -p " + P.ToString("G", CultureInfo.InvariantCulture) + "D0";
            Arguments += " -t " + T.ToString("G", CultureInfo.InvariantCulture) + "D0";

            umrol.StartInfo.UseShellExecute = false;
            umrol.StartInfo.RedirectStandardOutput = true;
            umrol.StartInfo.FileName = "umrol.exe";
            umrol.StartInfo.Arguments = Arguments;
            umrol.Start();
            umrol.PriorityClass = ProcessPriorityClass.Idle;
            output = umrol.StandardOutput.ReadToEnd();
            umrol.WaitForExit();

            if (output.Length > 0)
            {
                Results[0] = Convert.ToDouble(output.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0], CultureInfo.InvariantCulture);
                Results[1] = Convert.ToDouble(output.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1], CultureInfo.InvariantCulture);
            }

            return Results;
        }

        public double Dropout_Search(double wd, double T, double P_Max, double limit = 0.01, int max_iterations = 25)
        {
            double P = double.NaN;
            string Arguments = "-ds -id";
            string output = "";
            Process umrol = new Process();

            foreach (int i in Composition_IDs)
            {
                Arguments += " " + i.ToString();
            }
            Arguments += " -z";
            foreach (double v in Composition_Values)
            {
                // Make the value string into a Fortran style double presicion literal
                string val = " " + v.ToString("G", CultureInfo.InvariantCulture).Replace('E', 'D');
                if (!val.Contains("D"))
                    val += "D0";

                Arguments += val;
            }
            Arguments += " -wd "      + wd.ToString("G", CultureInfo.InvariantCulture)    + "D0";
            Arguments += " -p "       + P_Max.ToString("G", CultureInfo.InvariantCulture) + "D0";
            Arguments += " -t "       + T.ToString("G", CultureInfo.InvariantCulture)     + "D0";
            Arguments += " -limit "   + limit.ToString("G", CultureInfo.InvariantCulture) + "D0";
            Arguments += " -max-itr " + max_iterations.ToString();

            umrol.StartInfo.UseShellExecute = false;
            umrol.StartInfo.RedirectStandardOutput = true;
            umrol.StartInfo.FileName = "umrol.exe";
            umrol.StartInfo.Arguments = Arguments;
            try
            {
                umrol.Start();
                umrol.PriorityClass = ProcessPriorityClass.Idle;
                output = umrol.StandardOutput.ReadToEnd();
                umrol.WaitForExit();
            }
            catch
            {
                P = double.NaN;
            }

            if (output.Length > 0)
            {
                P = Convert.ToDouble(output.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0], CultureInfo.InvariantCulture);
            }

            return P;
        }


        /// <summary>
        /// Calculates the dew point pressure of the composition.
        /// </summary>
        /// <param name="T">Temperature [K]</param>
        /// <returns>The dew point pressure [bara].</returns>
        public double DewP(double T)
        {
            double P1 = 0.0;
            double P2 = 0.0;
            double[] XY1 = new double[100];
            double[] XY2 = new double[100];
            string Arguments = "-dewp -id";
            string output = "";
            Process umrol = new Process();

            foreach (int i in Composition_IDs)
            {
                Arguments += " " + i.ToString();
            }
            Arguments += " -z";
            foreach (double v in Composition_Values)
            {
                // Make the value string into a Fortran style double presicion literal
                string val = " " + v.ToString("G", CultureInfo.InvariantCulture).Replace('E', 'D');
                if (!val.Contains("D"))
                    val += "D0";

                Arguments += val;
            }
            Arguments += " -t " + T.ToString("G", CultureInfo.InvariantCulture) + "D0";

            umrol.StartInfo.UseShellExecute = false;
            umrol.StartInfo.RedirectStandardOutput = true;
            umrol.StartInfo.FileName = "umrol.exe";
            umrol.StartInfo.Arguments = Arguments;
            umrol.Start();
            umrol.PriorityClass = ProcessPriorityClass.Idle;
            output = umrol.StandardOutput.ReadToEnd();
            umrol.WaitForExit();

            if (output.Length > 0)
            {
                P1 = Convert.ToDouble(output.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0], CultureInfo.InvariantCulture);
                P2 = Convert.ToDouble(output.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1], CultureInfo.InvariantCulture);
            }

            if (P1 > 900.0) P1 = 0.0;
            if (P2 > 900.0) P2 = 0.0;

            return Math.Max(P1, P2);
        }

        public int Fluid_Tune()
        {
            string Arguments = "-tf -id";
            string output = "";
            Process umrol = new Process();

            foreach (int i in Composition_IDs)
            {
                Arguments += " " + i.ToString();
            }
            Arguments += " -z";
            foreach (double v in Composition_Values)
            {
                // Make the value string into a Fortran style double presicion literal
                string val = " " + v.ToString("G", CultureInfo.InvariantCulture).Replace('E', 'D');
                if (!val.Contains("D"))
                    val += "D0";

                Arguments += val;
            }

            umrol.StartInfo.UseShellExecute = false;
            umrol.StartInfo.RedirectStandardOutput = true;
            umrol.StartInfo.FileName = "umrol.exe";
            umrol.StartInfo.Arguments = Arguments;
            umrol.Start();
            umrol.PriorityClass = ProcessPriorityClass.Idle;
            output = umrol.StandardOutput.ReadToEnd();
            umrol.WaitForExit();

            if (output.Length > 0)
            {
                for (int i = 0; i < Composition_Values.Length; i++)
                {
                    Composition_Values[i] = Convert.ToDouble(output.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[i], CultureInfo.InvariantCulture);
                }
            }

            return 0;
        }
    }
}
