namespace THYLoggerAPI_POSTGRESQL.Model
{
    public class NemCalibrationOptions
    {
        public double InLow { get; set; } = 4.0;
        public double InHigh { get; set; } = 20.0;
        public double OutLow { get; set; } = 0.0;
        public double OutHigh { get; set; } = 100.0;
        public int Precision { get; set; } = 4;
    }
}
