using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TradesTracker.Dedust
{
    public static class MathUtils
    {
        // simplified cubic equation solver where b = 0
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static double SolveCubicEquation(double a, double c, double d)
        {
            c /= a;
            d /= a;
            double h = Math.Sqrt(d * d / 4d + c * c * c / 27d);

            double r = (-d / 2) + h;
            double s = r >= 0 ? Math.Pow(r, 1 / 3d) : -Math.Pow(-r, 1d / 3d);

            double t = (-d / 2) - h;
            double u = t >= 0 ? Math.Pow(t, 1d / 3d) : -Math.Pow(-t, 1 / 3d);

            return s + u;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static (double? X1, double? X2) SolveQuadraticEquation(double a, double b, double c)
        {
            double discriminant = b * b - 4d * a * c;
            if (discriminant > 0d)
                return ((-b + Math.Sqrt(discriminant)) / (2d * a),
                        (-b - Math.Sqrt(discriminant)) / (2d * a));

            if (discriminant == 0d)
                return (-b / (2d * a), null);

            return (null, null);
        }
    }
}
