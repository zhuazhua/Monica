using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Monica.Common.Pocos;

namespace Monica.Common.Utils
{
    public class DateTimeHelper
    {
        private static readonly object SyncRoot = new Object();

        private static bool _isInit;

        private static Dictionary<int, TimeZoneInfo> _timeZoneIndexDict;

        public const int DefaultTimezoneIndex = 210;

        private static Dictionary<Regex, string> _dateTimeFormatDict;
        private static Dictionary<Regex, string> _dateFormatDict;

        public static DateTime Parse(string dateTimeString)
        {
            if (_isInit == false)
                Init();
            foreach (var pair in _dateTimeFormatDict.Where(pair => pair.Key.IsMatch(dateTimeString)))
            {
                return DateTime.ParseExact(dateTimeString, pair.Value, null);
            }
            foreach (var pair in _dateFormatDict.Where(pair => pair.Key.IsMatch(dateTimeString)))
            {
                return DateTime.ParseExact(dateTimeString, pair.Value, null);
            }
            throw new Exception($"Invalid DateTime format, dateTimeString={dateTimeString}");
        }

        public static DateTime ParseDateTime(string dateTimeString)
        {
            if (_isInit == false)
                Init();
            foreach (var pair in _dateTimeFormatDict.Where(pair => pair.Key.IsMatch(dateTimeString)))
            {
                return DateTime.ParseExact(dateTimeString, pair.Value, null);
            }
            throw new Exception($"Invalid DateTime format, dateTimeString={dateTimeString}");
        }

        public static DateTime ParseDate(string dateString)
        {
            if (_isInit == false)
                Init();
            foreach (var pair in _dateFormatDict.Where(pair => pair.Key.IsMatch(dateString)))
            {
                return DateTime.ParseExact(dateString, pair.Value, null);
            }
            throw new Exception($"Invalid DateTime format, dateTimeString={dateString}");
        }

        public static TimeSpan Abs(DateTime d1, DateTime d2)
        {
            return d1 > d2 ? d1 - d2 : d2 - d1;
        }


        private static void Init()
        {
            lock (SyncRoot)
            {
                if (_isInit) return;
                lock (SyncRoot)
                {
                    if (_isInit == false)
                    {

                        _dateTimeFormatDict = new Dictionary<Regex, string>
                        {
                            {new Regex(@"^\d{8}\s\d{6}$", RegexOptions.Compiled), "yyyyMMdd HHmmss"},
                            {new Regex(@"^\d{8}\s[\d:\.]{5,12}$", RegexOptions.Compiled), "yyyyMMdd H:m:s.FFF"},
                            {new Regex(@"^[\d-]{8,10}\s[\d:\.]{5,12}$", RegexOptions.Compiled), "yyyy-M-d H:m:s.FFF"},
                            {new Regex(@"^[\d/]{8,10}\s[\d:\.]{5,12}$", RegexOptions.Compiled), "yyyy/M/d H:m:s.FFF"},

                        };

                        _dateFormatDict = new Dictionary<Regex, string>
                        {
                            {new Regex(@"^\d{8}$", RegexOptions.Compiled), "yyyyMMdd"},
                            {new Regex(@"^[\d-]{8,10}$", RegexOptions.Compiled), "yyyy-M-d"},
                            {new Regex(@"^[\d/]{8,10}$",RegexOptions.Compiled), "yyyy/M/d"}
                        };


                        Holidays = new Dictionary<string, List<string>>();
                        if (File.Exists(GeneralConstants.HolidayPath) == false)
                            throw new Exception(GeneralConstants.HolidayPath +
                                                        " does not exists. Current Dir is:" +
                                                        Directory.GetCurrentDirectory());
                        var lines = File.ReadAllLines(GeneralConstants.HolidayPath);
                        foreach (var line in lines)
                        {
                            var datas = line.Split(',');
                            if (datas.Length < 2)
                                continue;
                            var exchange = datas[0];
                            var holiday = datas[1];
                            if (Holidays.ContainsKey(exchange) == false)
                                Holidays.Add(exchange, new List<string>());
                            Holidays[exchange].Add(holiday);
                        }

                        _timeZoneIndexDict = new Dictionary<int, TimeZoneInfo>();
                        if (File.Exists(GeneralConstants.TimeZoneIndexPath) == false)
                            throw new Exception(GeneralConstants.TimeZoneIndexPath + " does not exists.");
                        lines = File.ReadAllLines(GeneralConstants.TimeZoneIndexPath);
                        foreach (var line in lines)
                        {
                            var datas = line.Split(',');
                            if (datas.Length < 2)
                                continue;
                            var tzi = int.Parse(datas[0]);
                            var  timeZoneId =datas[1];
                            _timeZoneIndexDict.Add(tzi, TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));
                        }
                        _isInit = true;
                    }
                }
            }
        }

        public static DateTime ConvertTime(DateTime dateTime, int sourceTimeZoneIndex, int destTimeZoneIndex)
        {

            var destTimeZoneInfo = GetTimeZoneInfo(destTimeZoneIndex);
            if (destTimeZoneIndex == DefaultTimezoneIndex)
                destTimeZoneInfo = TimeZoneInfo.Local;
            var sourceTimeZoneInfo = GetTimeZoneInfo(sourceTimeZoneIndex);
            if (sourceTimeZoneIndex == DefaultTimezoneIndex)
                sourceTimeZoneInfo = TimeZoneInfo.Local;
            return TimeZoneInfo.ConvertTime(dateTime, sourceTimeZoneInfo, destTimeZoneInfo);
        }

        public static DateTime ConvertLocalTime(DateTime dateTime, int destTimeZoneIndex)
        {
            var destTimeZoneInfo = GetTimeZoneInfo(destTimeZoneIndex);

            return TimeZoneInfo.ConvertTime(dateTime, TimeZoneInfo.Local, destTimeZoneInfo);
        }

        public static DateTime ConvertToLocalTime(DateTime dateTime, int sourceTimeZoneIndex)
        {
            var sourceTimeZoneInfo = GetTimeZoneInfo(sourceTimeZoneIndex);
            return TimeZoneInfo.ConvertTime(dateTime, sourceTimeZoneInfo, TimeZoneInfo.Local);
        }

        public static TimeZoneInfo GetTimeZoneInfo(int timeZoneIndex)
        {
            if (_isInit == false)
                Init();
            if (_timeZoneIndexDict.ContainsKey(timeZoneIndex) == false)
                throw new Exception($"Invalid timeZoneIndex={timeZoneIndex}");
            return _timeZoneIndexDict[timeZoneIndex];
        }

        /// <summary>
        /// Pack AmiBroker DateTime object into UInt64
        /// </summary>
        public static ulong PackDate(DateTime date, bool isFeaturePad)
        {
            var isEOD = date.Hour == 0 && date.Minute == 0 && date.Second == 0;

            // lower 32 bits
            var ft = BitVector32.CreateSection(1);
            var rs = BitVector32.CreateSection(23, ft);
            var ms = BitVector32.CreateSection(999, rs);
            var ml = BitVector32.CreateSection(999, ms);
            var sc = BitVector32.CreateSection(59, ml);

            var bv1 = new BitVector32(0);
            bv1[ft] = isFeaturePad ? 1 : 0;         // bit marking "future data"
            bv1[rs] = 0;                            // reserved set to zero
            bv1[ms] = 0;                            // microseconds 0..999
            bv1[ml] = date.Millisecond;             // milliseconds 0..999
            bv1[sc] = date.Second;                  // 0..59

            // higher 32 bits
            var mi = BitVector32.CreateSection(59);
            var hr = BitVector32.CreateSection(23, mi);
            var dy = BitVector32.CreateSection(31, hr);
            var mn = BitVector32.CreateSection(12, dy);
            var yr = BitVector32.CreateSection(4095, mn);

            var bv2 = new BitVector32(0);
            bv2[mi] = isEOD ? 63 : date.Minute;     // 0..59        63 is reserved as EOD marker
            bv2[hr] = isEOD ? 31 : date.Hour;       // 0..23        31 is reserved as EOD marker
            bv2[dy] = date.Day;                     // 1..31
            bv2[mn] = date.Month;                   // 1..12
            bv2[yr] = date.Year;                    // 0..4095

            return ((ulong)(uint)bv2.Data << 32) ^ (uint)bv1.Data;
        }

        public static Dictionary<string, List<string>> Holidays;

        public static DateTime GetLastDay(DateTime date)
        {
            var last = date - new TimeSpan(1, 0, 0, 0);
            return last;
        }

        public static DateTime GetLastTradingDay(DateTime date, string exchange = "SH", bool isNight = false)
        {
            var last = GetLastDay(date);

            if (IsTradingDay(last, exchange, isNight) == false)
                return GetLastTradingDay(last, exchange, isNight);

            return last;
        }

        public static DateTime GetLastTradingDay(string date, string dateFormat = "yyyy-MM-dd", string exchange = "SH", bool isNight = false)
        {
            return GetLastTradingDay(DateTime.ParseExact(date, dateFormat, null), exchange, isNight);
        }

        public static DateTime GetNextDay(DateTime date)
        {
            var next = date + new TimeSpan(1, 0, 0, 0);
            return next;
        }

        public static DateTime GetNextTradingDay(DateTime date, string exchange = "SH", bool isNight = false)
        {
            var next = GetNextDay(date);

            if (IsTradingDay(next, exchange, isNight) == false)
                return GetNextTradingDay(next, exchange, isNight);

            return next;
        }

        public static DateTime GetNextTradingDay(string date, string dateFormat = "yyyy-MM-dd", string exchange = "SH", bool isNight = false)
        {
            return GetNextTradingDay(DateTime.ParseExact(date, dateFormat, null), exchange, isNight);
        }

        public static bool IsTradingDay(string date, string dateFormat = "yyyy-MM-dd", string exchange = "SH", bool isNight = false)
        {
            return IsTradingDay(DateTime.ParseExact(date, dateFormat, null), exchange, isNight);
        }

        public static bool IsTradingDay(DateTime date, string exchange = "SH", bool isNight = false)
        {
            if (_isInit == false)
                Init();
            if (date.DayOfWeek == DayOfWeek.Sunday)
                return false;
            if (date.DayOfWeek == DayOfWeek.Saturday && isNight == false)
                return false;
            if (Holidays.ContainsKey(exchange) && Holidays[exchange].Contains(date.ToString("yyyy-MM-dd")))
                return false;
            if (date.DayOfWeek == DayOfWeek.Saturday && exchange != "SHFE" && exchange != "DCE")
                return false;
            if (date.DayOfWeek == DayOfWeek.Saturday && Holidays.ContainsKey(exchange) &&
                Holidays[exchange].Contains(GetLastDay(date).ToString("yyyy-MM-dd")))
                return false;
            return true;
        }

        public static DateTime Trim(DateTime date, long roundTicks)
        {
            return new DateTime(date.Ticks - date.Ticks % roundTicks);
        }

        public static DateTime ConvertToLocalTime(DateTime time)
        {
            return time.Kind == DateTimeKind.Utc ? time.ToLocalTime() : time;
        }

        public static DateTime ForwardRoundMinute(DateTime time)
        {
            if (time.Ticks % TimeSpan.TicksPerMinute > 0)
                return new DateTime(time.Year, time.Month, time.Day, time.Hour,
                    time.Minute,
                    0) + new TimeSpan(0, 0, 1, 0);
            return new DateTime(time.Year, time.Month, time.Day, time.Hour,
                    time.Minute,
                    0);
        }

        public static string DisplayTime(double miliseconds)
        {
            var timespan = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(miliseconds));
            
            if (miliseconds < 1000)
            {
                return string.Format("{0:N3}ms", miliseconds);
            }
            if (miliseconds >= 1000 && miliseconds < 60 * 1000)
            {
                return string.Format("{0:N1}s", miliseconds / 1000);
            }
            if (miliseconds >= 60 * 1000 && miliseconds < 60 * 60 * 1000)
            {
                return string.Format("{0:N0}m{1:N0}s", timespan.Minutes, timespan.Seconds);
            }
            if (miliseconds >= 60 * 60 * 1000 && miliseconds < 60 * 60 * 60 * 1000)
            {
                return string.Format("{0:N0}h{1:N0}m{2:N0}s", timespan.Hours, timespan.Minutes, timespan.Seconds);
            }
            return string.Format("{0:N0}ms", miliseconds);
        }

        public static ulong GetAmiDate(DateTime dateTime)
        {
            var dateNum = (ulong)(10000 * DateTime.Now.Year + 100 * DateTime.Now.Month + DateTime.Now.Day);
            var timeNum = (ulong)(10000 * dateTime.Hour + 100 * dateTime.Minute + dateTime.Second);
            return dateNum * 100000000 + timeNum;
        }

        public static int CalcTradingDaysInterval(string dateFrom, string dateTo = null,
                string exchange = "SH", bool isNight = false, string dateFormat = "yyyy-MM-dd")
        {
            if (dateTo == null)
            {
                dateTo = DateTime.Today.ToString(dateFormat);
            }

            var days = 0;
            var curDate = dateFrom;
            while (string.CompareOrdinal(curDate, dateTo)<0)
            {
                curDate = GetNextTradingDay(curDate,dateFormat,exchange,isNight).ToString(dateFormat);
                ++days;
            }
            return days;
        }


    }
}