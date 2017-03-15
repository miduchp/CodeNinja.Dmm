using System;

namespace CodeNinja.Dmm.Drivers
{
    public class Measurement
    {
        public Measurement()
        {
            TimeStamp = DateTime.UtcNow;
        }

        public DateTime TimeStamp { get; set; }
        public int Value { get; set; }
        public float Factor { get; set; }
        public bool IsNegativeSign { get; set; }
        public bool UnderLimit { get; set; }
        public bool OverLimit { get; set; }

        public float GetValue()
        {
            if (!UnderLimit && !OverLimit)
            {
                return (float)((IsNegativeSign ? -1 : 1) * Value) * Factor;
            }

            throw new Exception();
        }

        public bool IsVoltage { get; set; }
        public bool IsAc { get; set; }
        public bool IsDc { get; set; }
        public bool IsResistance { get; set; }
        public bool IsCapacitance { get; set; }
        public bool IsTemperature { get; set; }
        public bool IsCelcius { get; set; }
        public bool IsFahrenheit { get; set; }
        public bool IsCurrent { get; set; }
        public bool IsContinuity { get; set; }
        public bool IsDiode { get; set; }
        public bool IsFrequency { get; set; }
        public bool IsPower { get; set; }
        public bool IsLoopCurrent { get; set; }
        public bool IsDutyCycle { get; set; }
        public bool IsAuto { get; set; }
        public bool IsManual { get; set; }
    }
}

