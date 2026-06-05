namespace Skyline.DataMiner.Utils.Protocol.Extension
{
    using System;

    /// <summary>
    /// Defines extension methods to change a type of an object value.
    /// </summary>
    internal static class TypeExtensions
    {
        private static readonly Type DateTimeType = typeof(DateTime);

        /// <summary>
        /// Converts an object to the desired type.
        /// </summary>
        /// <typeparam name="T">Type of the result.</typeparam>
        /// <param name="obj">Object to convert.</param>
        /// <returns>The converted object.</returns>
        /// <exception cref="InvalidCastException">This conversion is not supported. Or <paramref name="obj"/> does not implement the <see cref="IConvertible"/> interface.</exception>
        /// <exception cref="FormatException"><paramref name="obj"/> is not in a format unrecognized by conversionType.</exception>
        /// <exception cref="OverflowException"><paramref name="obj"/> represents a number that is out of the range of conversionType.</exception>
        public static T ChangeType<T>(this object obj)
            where T : IConvertible
        {
            if (obj == null)
            {
                return default(T);
            }

            var type = typeof(T);

            if (type.IsEnum)
            {
                return (T)Enum.ToObject(type, obj.ChangeType<int>());
            }

            if (type == DateTimeType)
            {
                var oadate = Convert.ToDouble(obj);

                if (!oadate.InRange(-657435.0, 2958465.99999999))
                {
                    throw new OverflowException($"{obj} is not a valid OA Date, supported range -657435.0 to 2958465.99999999");
                }

                object date = DateTime.FromOADate(oadate);
                return (T)date;
            }

            return (T)Convert.ChangeType(obj, type);
        }

        /// <summary>
        /// Checks if a values is inside an interval.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="value">Value to check.</param>
        /// <param name="fromInclusive">Lower Range, inclusive value.</param>
        /// <param name="toInclusive">High Range, inclusive value.</param>
        /// <returns>True if the value is between the given interval; otherwise false.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="value"/> is null.</exception>
        private static bool InRange<T>(this T value, T fromInclusive, T toInclusive)
            where T : IComparable
        {
            if (Object.Equals(value, default(T)))
            {
                throw new ArgumentNullException(nameof(value));
            }

            return value.CompareTo(fromInclusive) >= 0 && value.CompareTo(toInclusive) <= 0;
        }
    }
}
