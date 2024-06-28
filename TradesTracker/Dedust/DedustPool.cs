using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using TonSharp.Core;

namespace TradesTracker.Dedust
{
    public sealed class DedustPool
    {
        public Address Address { get; init; }

        public DedustAsset Left { get; init; }

        public DedustAsset Right { get; init; }

        public PoolType Type { get; init; } = PoolType.Volatile;

        public UInt128 ReserveLeft { get; set; }

        public UInt128 ReserveRight { get; set; }

        public double PricePerLeft => Type == PoolType.Stable ? CalculateStablePriceLeft(1d) : CalculateVolatilePriceLeft(1d);

        public double PricePerRight => Type == PoolType.Stable ? CalculateStablePriceRight(1d) : CalculateVolatilePriceRight(1d);

        public DedustPool(Address address, DedustAsset left, DedustAsset right)
        {
            Address = address;
            Left = left;
            Right = right;
        }

        public double CalculateLeftToRight(double leftAmount)
        {
            return Type == PoolType.Stable ? CalculateStablePriceLeft(leftAmount) : CalculateVolatilePriceLeft(leftAmount);
        }

        public double CalculateRightToLeft(double rightAmount)
        {
            return Type == PoolType.Stable ? CalculateStablePriceRight(rightAmount) : CalculateVolatilePriceRight(rightAmount);
        }

        public double AmountToBuyLeftUntilPrice(double priceInRight)
        {
            double leftReserve = (double)ReserveLeft / Math.Pow(10, Left.Decimals);
            double rightReserve = (double)ReserveRight / Math.Pow(10, Right.Decimals);
            double constantProduct = leftReserve * rightReserve;
            (double? X1, double? X2) = MathUtils.SolveQuadraticEquation(1d, -priceInRight, -priceInRight * constantProduct);
            if (X1 == null)
                return double.NaN;
            double targetRReserve = X2 != null && X2.Value > X1.Value ? X2.Value : X1.Value;
            double targetLReserve = constantProduct / targetRReserve;
            return leftReserve - targetLReserve;
        }

        public double AmountToBuyRightUntilPrice(double priceInLeft)
        {
            double leftReserve = (double)ReserveLeft / Math.Pow(10, Left.Decimals);
            double rightReserve = (double)ReserveRight / Math.Pow(10, Right.Decimals);
            double constantProduct = leftReserve * rightReserve;
            (double? x1, double? x2) = MathUtils.SolveQuadraticEquation(1d, -priceInLeft, -priceInLeft * constantProduct);
            if (x1 == null)
                return double.NaN;
            double targetLReserve = x2 != null && x2.Value > x1.Value ? x2.Value : x1.Value;
            double targetRReserve = constantProduct / targetLReserve;
            return rightReserve - targetRReserve;
        }

        // Volatile constant product is x • y = k
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double CalculateVolatilePriceLeft(double leftAmount)
        {
            if (ReserveLeft == UInt128.Zero || ReserveRight == UInt128.Zero)
                return 0d;
            double leftReserve = (double)ReserveLeft / Math.Pow(10, Left.Decimals);
            double rightReserve = (double)ReserveRight / Math.Pow(10, Right.Decimals);
            double constantProduct = leftReserve * rightReserve;
            return rightReserve - constantProduct / (leftReserve + leftAmount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double CalculateVolatilePriceRight(double rightAmount)
        {
            if (ReserveLeft == UInt128.Zero || ReserveRight == UInt128.Zero)
                return 0d;
            double leftReserve = (double)ReserveLeft / Math.Pow(10, Left.Decimals);
            double rightReserve = (double)ReserveRight / Math.Pow(10, Right.Decimals);
            double constantProduct = leftReserve * rightReserve;
            return leftReserve - constantProduct / (rightReserve + rightAmount);
        }

        // Stable constant product is x^3 • y + y^3 • x = k
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double CalculateStablePriceLeft(double leftAmount)
        {
            if (ReserveLeft == UInt128.Zero || ReserveRight == UInt128.Zero)
                return 0d;
            double leftReserve = (double)ReserveLeft / Math.Pow(10, Left.Decimals);
            double rightReserve = (double)ReserveRight / Math.Pow(10, Right.Decimals);
            double constantProduct = leftReserve * leftReserve * leftReserve * rightReserve + rightReserve * rightReserve * rightReserve * leftReserve;
            // represent this as cubic equation to find new y. x = leftReserve + amount
            // so formula look like `x • y^3 + x^3 • y - constantProduct = 0` and now solve it
            double newLReserve = leftReserve + leftAmount;
            double newRReserve = MathUtils.SolveCubicEquation(newLReserve, newLReserve * newLReserve * newLReserve, -constantProduct);
            return rightReserve - newRReserve;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double CalculateStablePriceRight(double rightAmount)
        {
            if (ReserveLeft == UInt128.Zero || ReserveRight == UInt128.Zero)
                return 0d;
            double leftReserve = (double)ReserveLeft / Math.Pow(10, Left.Decimals);
            double rightReserve = (double)ReserveRight / Math.Pow(10, Right.Decimals);
            double constantProduct = leftReserve * leftReserve * leftReserve * rightReserve + rightReserve * rightReserve * rightReserve * leftReserve;
            // represent this as cubic equation to find new y. x = leftReserve + amount
            // so formula look like `x • y^3 + x^3 • y - constantProduct = 0` and now solve it
            double newRReserve = rightReserve + rightAmount;
            double newLReserve = MathUtils.SolveCubicEquation(newRReserve, newRReserve * newRReserve * newRReserve, -constantProduct);
            return leftReserve - newLReserve;
        }
    }
}
