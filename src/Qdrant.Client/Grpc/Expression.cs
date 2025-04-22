namespace Qdrant.Client.Grpc
{
    /// <summary>
    /// To construct a formula for score boosting.
    /// </summary>
    public partial class Expression
    {
        /// <summary>
        /// Implicitly converts a float constant to a new instance of <see cref="Expression"/>.
        /// </summary>
        public static implicit operator Expression(float constant) =>
            new() { Constant = constant };

        /// <summary>
        /// Implicitly converts a variable name (string) to a new instance of <see cref="Expression"/>.
        /// Used to reference a payload key or a score variable.
        /// </summary>
        public static implicit operator Expression(string variable) =>
            new() { Variable = variable };

        /// <summary>
        /// Implicitly converts a <see cref="Condition"/> to a new instance of <see cref="Expression"/>.
        /// Evaluates to 1.0 if true, 0.0 if false.
        /// </summary>
        public static implicit operator Expression(Condition condition) =>
            new() { Condition = condition };

        /// <summary>
        /// Implicitly converts a <see cref="GeoDistance"/> to a new instance of <see cref="Expression"/>.
        /// Represents a geographic distance in meters.
        /// </summary>
        public static implicit operator Expression(GeoDistance geoDistance) =>
            new() { GeoDistance = geoDistance };

        /// <summary>
        /// Converts a date-time constant string to a new instance of <see cref="Expression"/>.
        /// </summary>
        public static Expression FromDateTime(string dateTime) =>
            new() { Datetime = dateTime };

        /// <summary>
        /// Converts a date-time key string to a new instance of <see cref="Expression"/>.
        /// Used to reference a payload key with date-time values.
        /// </summary>
        public static Expression FromDateTimeKey(string dateTimeKey) =>
            new() { DatetimeKey = dateTimeKey };

        /// <summary>
        /// Converts a <see cref="MultExpression"/> to a new instance of <see cref="Expression"/>.
        /// </summary>
        public static implicit operator Expression(MultExpression mult) =>
            new() { Mult = mult };

        /// <summary>
        /// Implicitly converts a <see cref="SumExpression"/> to a new instance of <see cref="Expression"/>.
        /// </summary>
        public static implicit operator Expression(SumExpression sum) =>
            new() { Sum = sum };

        /// <summary>
        /// Implicitly converts a <see cref="DivExpression"/> to a new instance of <see cref="Expression"/>.
        /// </summary>
        public static implicit operator Expression(DivExpression div) =>
            new() { Div = div };

        /// <summary>
        /// Converts an <see cref="Expression"/> to a negated expression.
        /// </summary>
        public static Expression FromNeg(Expression expr) =>
            new() { Neg = expr };

        /// <summary>
        /// Converts an <see cref="Expression"/> to its absolute value.
        /// </summary>
        public static Expression FromAbs(Expression expr) =>
            new() { Abs = expr };

        /// <summary>
        /// Converts an <see cref="Expression"/> to its square root.
        /// </summary>
        public static Expression FromSqrt(Expression expr) =>
            new() { Sqrt = expr };

        /// <summary>
        /// Implicitly converts a <see cref="PowExpression"/> to an <see cref="Expression"/>.
        /// </summary>
        public static implicit operator Expression(PowExpression pow) =>
            new() { Pow = pow };

        /// <summary>
        /// Converts an <see cref="Expression"/> to an exponential.
        /// </summary>
        public static Expression FromExp(Expression expr) =>
            new() { Exp = expr };

        /// <summary>
        /// Converts an <see cref="Expression"/> to a base-10 logarithm.
        /// </summary>
        public static Expression FromLog10(Expression expr) =>
            new() { Log10 = expr };

        /// <summary>
        /// Converts an <see cref="Expression"/> to a natural logarithm.
        /// </summary>
        public static Expression FromLn(Expression expr) =>
            new() { Ln = expr };

        /// <summary>
        /// Converts an exponential decay parameter to a new instance of <see cref="Expression"/>.
        /// </summary>
        public static Expression FromExpDecay(DecayParamsExpression decay) =>
            new() { ExpDecay = decay };

        /// <summary>
        /// Converts a Gaussian decay parameter to a new instance of <see cref="Expression"/>.
        /// </summary>
        public static Expression FromGaussDecay(DecayParamsExpression decay) =>
            new() { GaussDecay = decay };

        /// <summary>
        /// Converts a linear decay parameter to a new instance of <see cref="Expression"/>.
        /// </summary>
        public static Expression FromLinDecay(DecayParamsExpression decay) =>
            new() { LinDecay = decay };
    }
}
