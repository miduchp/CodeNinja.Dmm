using NLog;
using System;
using System.Linq;
using System.Text;

namespace CodeNinja.Dmm.Drivers.Ut71b
{
    public class PacketParser
    {
        private readonly ILogger _logger;

        public PacketParser(ILogger logger)
        {
            _logger = logger;
        }

        private readonly float[,] _factors = new float[,] {
                                                            {1e-5F, 0,     0,     0,     0,    0,    0,    0   }, /* AC mV */
	                                                        {0,    1e-4F,  1e-3F,  1e-2F,  1e-1F, 0,    0,    0   }, /* DC V */
	                                                        {0,    1e-4F,  1e-3F,  1e-2F,  1e-1F, 0,    0,    0   }, /* AC V */
	                                                        {1e-5F, 0,     0,     0,     0,    0,    0,    0   }, /* DC mV */
	                                                        {0,    1e-1F,  1,     1e1F,   1e2F,  1e3F,  1e4F,  0   }, /* Resistance */
	                                                        {0,    1e-12F, 1e-11F, 1e-10F, 1e-9F, 1e-8F, 1e-7F, 1e-6F}, /* Capacitance */
	                                                        {1e-1F, 0,     0,     0,     0,    0,    0,    0   }, /* Temp (C) */
	                                                        {1e-8F, 1e-7F,  0,     0,     0,    0,    0,    0   }, /* uA */
	                                                        {1e-6F, 1e-5F,  0,     0,     0,    0,    0,    0   }, /* mA */
	                                                        {0,    1e-3F,  0,     0,     0,    0,    0,    0   }, /* 10A */
	                                                        {1e-1F, 0,     0,     0,     0,    0,    0,    0   }, /* Continuity */
	                                                        {1e-4F, 0,     0,     0,     0,    0,    0,    0   }, /* Diode */
	                                                        {1e-3F, 1e-2F,  1e-1F,  1F,     1e1F,  1e2F,  1e3F,  1e4F }, /* Frequency */
	                                                        {1e-1F, 0,     0,     0,     0,    0,    0,    0   }, /* Temp (F) */
	                                                        {0,    0,     0,     1,     0,    0,    0,    0   }, /* Power */
	                                                        {1e-2F, 0,     0,     0,     0,    0,    0,    0   }, /* Loop current */
                                                        };

        public void ParseValue(byte[] buffer, Measurement measurement)
        {
            string result = Encoding.ASCII.GetString(buffer.Take(5).ToArray());
            var numDigits = 5;
            var intval = 0;

            if (result == "::0<:")
            {
                measurement.OverLimit = true;
            }
            else if (result == ":<0::")
            {
                measurement.UnderLimit = true;
            }
            else if (buffer[4] == ':')
            {
                numDigits = 4;
            }

            for (int k = 0; k < numDigits; k++)
            {
                intval = 10 * intval + (buffer[k] - '0');
            }

            measurement.Value = intval;
        }

        public bool ParseRange(byte[] buffer, Measurement measurement)
        {

            int idx, mode;
            float factor = 0;

            idx = buffer[5] - '0';

            if (idx < 0 || idx > 7)
            {
                _logger.Error("Invalid range byte {0:X2} (idx {1:X2}).", buffer[5], idx);
                return false;
            }

            mode = buffer[6] - '0';
            if (mode < 0 || mode > 15)
            {

                _logger.Error("Invalid mode byte {0:X2} (idx {1:X2}).", buffer[6], mode);
                return false;
            }

            factor = _factors[mode, idx];

            if (factor == 0)
            {

                _logger.Error("Invalid factor for range byte: {0:X2}.", buffer[5]);
                return false;
            }

            measurement.Factor = factor;

            return true;
        }

        public void ParseFlags(byte[] buffer, Measurement measurement)
        {
            switch (buffer[6] - '0')
            {
                case 0: /* AC mV */
                    {
                        measurement.IsVoltage = true;
                        measurement.IsAc = true;
                        break;
                    }
                case 1: /* DC V */
                    {
                        measurement.IsVoltage = true;
                        measurement.IsDc = true;
                        break;
                    }
                case 2: /* AC V */
                    {
                        measurement.IsVoltage = true;
                        measurement.IsAc = true;
                        break;
                    }
                case 3: /* DC mV */
                    {
                        measurement.IsVoltage = true;
                        measurement.IsDc = true;
                        break;
                    }
                case 4: /* Resistance */
                    {
                        measurement.IsResistance = true;
                        break;
                    }
                case 5: /* Capacitance */
                    {
                        measurement.IsCapacitance = true;
                        break;
                    }
                case 6: /* Temperature (Celsius) */
                    {
                        measurement.IsTemperature = true;
                        measurement.IsCelcius = true;
                        break;
                    }
                case 7: /* uA */
                    {
                        measurement.IsCurrent = true;
                        measurement.IsDc = true;
                        break;
                    }

                case 8: /* mA */
                    {
                        measurement.IsCurrent = true;
                        measurement.IsDc = true;
                        break;
                    }
                case 9: /* 10A */
                    {
                        measurement.IsCurrent = true;
                        measurement.IsDc = true;
                        break;
                    }
                case 10: /* Continuity */
                    {
                        measurement.IsContinuity = true;
                        break;
                    }
                case 11: /* Diode */
                    {
                        measurement.IsDiode = true;
                        break;
                    }
                case 12: /* Frequency */
                    {
                        measurement.IsFrequency = true;
                        break;
                    }
                case 13: /* Temperature (F) */
                    {
                        measurement.IsTemperature = true;
                        measurement.IsFahrenheit = true;
                        break;
                    }
                case 14: /* Power */
                         /* Note: Only available on UT71E (range 0-2500W). */
                    {
                        measurement.IsPower = true;
                        break;
                    }
                case 15: /* DC loop current, percentage display (range 4-20mA) */
                    {
                        measurement.IsLoopCurrent = true;
                        break;
                    }
                default:
                    _logger.Debug("Invalid function byte: {0:X2}", buffer[6]);
                    break;
            }

            /*
             * State 1 byte: bit 0 = AC, bit 1 = DC
             * Either AC or DC or both or none can be set at the same time.
             */
            measurement.IsAc = (buffer[7] & (1 << 0)) != 0;
            measurement.IsDc = (buffer[7] & (1 << 1)) != 0;

            /*
             * State 2 byte: bit 0 = auto, bit 1 = manual, bit 2 = sign
             *
             * The Conrad/Voltcraft protocol descriptions have a typo
             * (they suggest bit 3 as sign bit, which is incorrect).
             *
             * For modes where there's only one possible range (e.g. AC mV)
             * neither the "auto" nor the "manual" bits will be set.
             */
            measurement.IsAuto = (buffer[8] & (1 << 0)) != 0;
            measurement.IsManual = (buffer[8] & (1 << 1)) != 0;
            measurement.IsNegativeSign = (buffer[8] & (1 << 2)) != 0;

            /* Note: "Frequency mode + sign bit" means "duty cycle mode". */
            if (measurement.IsFrequency && measurement.IsNegativeSign)
            {
                measurement.IsDutyCycle = true;
                measurement.IsFrequency = false;
                measurement.IsNegativeSign = false;
            }
        }

        private bool FlagsValid(Measurement measurement)
        {
	        int count;

            /* Does the packet "measure" more than one type of value? */
            count  = (measurement.IsVoltage) ? 1 : 0;
	        count += (measurement.IsCurrent) ? 1 : 0;
	        count += (measurement.IsResistance) ? 1 : 0;
	        count += (measurement.IsCapacitance) ? 1 : 0;
	        count += (measurement.IsFrequency) ? 1 : 0;
	        count += (measurement.IsTemperature) ? 1 : 0;
	        count += (measurement.IsContinuity) ? 1 : 0;
	        count += (measurement.IsDiode) ? 1 : 0;
	        count += (measurement.IsPower) ? 1 : 0;
	        count += (measurement.IsLoopCurrent) ? 1 : 0;
	        if (count > 1)
            {

		        return false;
	        }

	        /* Auto and manual can't be active at the same time. */
	        if (measurement.IsAuto && measurement.IsManual) {

		        return false;
	        }

	        return true;
        }

        public Measurement Parse(byte[] packet)
        {
            Measurement measurement = new Measurement();

            ParseFlags(packet, measurement);

            ParseValue(packet, measurement);

            ParseRange(packet, measurement);

            return measurement;
        }

        public bool IsValid(byte[] packet)
        {
            Measurement measurement = new Measurement();

            if (packet[9] != '\r' || packet[10] != '\n')
            {
                return false;
            }

            ParseFlags(packet, measurement);
           
            return FlagsValid(measurement);
        }

        public void Print(byte[] packet)
        {
            StringBuilder sb = new StringBuilder();

            for (int j = 0; j < packet.Length; j++)
            {
                sb.AppendFormat("{0:X2} ", packet[j]);
            }

            _logger.Debug(sb.ToString());
        }
    }
}
