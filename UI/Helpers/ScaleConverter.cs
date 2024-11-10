using System.Globalization;

namespace UI.Helpers
{
    public class ScaleConverterParameter
    {
        public double A1 { get; set; } = 1;
        public double B1 { get; set; } = 1;
        public double A2 { get; set; } = 1;
        public double B2 { get; set; } = 1;
    }
    public class ScaleConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double d)
            {
                var p = GetParameter(parameter);
                return (d - p.A1) / (p.B1 - p.A1) * (p.B2 - p.A2) + p.A2;
            }
            return value;
        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double d)
            {
                var p = GetParameter(parameter);
                return (d - p.A2) / (p.B2 - p.A2) * (p.B1 - p.A1) + p.A1;
            }
            return value;
        }
        private static ScaleConverterParameter GetParameter(object? parameter)
        {
            if (parameter is ScaleConverterParameter p)
            {
                return p;
            }
            else if (parameter is string s)
            {
                var parts = s.Split(',');
                if (parts.Length == 4)
                {
                    return new ScaleConverterParameter
                    {
                        A1 = double.Parse(parts[0]),
                        B1 = double.Parse(parts[1]),
                        A2 = double.Parse(parts[2]),
                        B2 = double.Parse(parts[3])
                    };
                }
            }
            return new ScaleConverterParameter();
        }
    }
}
