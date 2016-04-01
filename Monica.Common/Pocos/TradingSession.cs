using System;
using System.Collections.Generic;
using System.Linq;
using Platinum.Common.Utils;

namespace Platinum.Common.Pocos
{
    public class SubSession
    {
        public DateTime Start { get;  set; }
        public DateTime End { get;  set; }
        public int TimeZoneIndex { get; set; }

        public SubSession(DateTime start, DateTime end, int timeZoneIndex)
        {
            Start = new DateTime(start.Year,start.Month,start.Day,start.Hour,start.Minute,start.Second);
            End = new DateTime(end.Year, end.Month, end.Day, end.Hour, end.Minute, end.Second);
            TimeZoneIndex = timeZoneIndex;
        }

        public SubSession()
        {
            
        }

        public int GetMinutesCount()
        {
            return (int)(End - Start).TotalMinutes + 1;
        }

        public bool IsInSubSession(TimeSpan time)
        {
            if (Start > End)
            {
                if (time >= Start.TimeOfDay || time <= End.TimeOfDay)
                    return true;
            }
            if (time >= Start.TimeOfDay && time <= End.TimeOfDay)
                return true;
            return false;
        }

        public bool IsInSubSession(DateTime time)
        {
            if (Start > End)
            {
                if (time >= Start || time <= End)
                    return true;
            }
            if (time >= Start && time <= End)
                return true;
            return false;
        }

        public int Check(TimeSpan time)
        {
            if (time >= Start.TimeOfDay && time <= End.TimeOfDay)
                return 0;
            if (time < Start.TimeOfDay)
                return -1;
            return 1;
        }

        public int Check(DateTime time)
        {
            if (time >= Start && time <= End)
                return 0;
            if (time < Start)
                return -1;
            return 1;
        }

        public void Shift(int destTimeZoneIndex)
        {
            Start = DateTimeHelper.ConvertTime(Start, TimeZoneIndex, destTimeZoneIndex);
            End = DateTimeHelper.ConvertTime(End, TimeZoneIndex, destTimeZoneIndex);
            TimeZoneIndex = destTimeZoneIndex;
        }

        public bool HasTimeSpan(IEnumerable<DateTime> times)
        {
            return times.Any(IsInSubSession);
        }

        public override string ToString()
        {
            return $"{Start.ToString(@"HHmmss")}-{End.ToString(@"HHmmss")}";
        }
    }
    public class TradingSession
    {
        public List<SubSession> SubSessions { get;  set; }
        public int TimeZoneIndex { get; set; }

        public TradingSession()
        {
            
        }
        public TradingSession(string tradingSessionString, int timeZoneIndex,DateTime date)
        {
            SubSessions = new List<SubSession>();
            TimeZoneIndex = timeZoneIndex;
            var subSessionStrings = tradingSessionString.Split('&');
            foreach (var subSessionString in subSessionStrings)
            {
                var start = date.Add(DateTime.ParseExact(subSessionString.Split('-')[0], "HHmmss", null).TimeOfDay);
                var end = date.Add(DateTime.ParseExact(subSessionString.Split('-')[1], "HHmmss", null).TimeOfDay);
                if (end < start)
                {
                    SubSessions.Add(new SubSession(start, start.Date.AddDays(1), timeZoneIndex));
                    SubSessions.Add(new SubSession(end.Date,end,timeZoneIndex));
                }
                else
                {
                    SubSessions.Add(new SubSession(start, end, timeZoneIndex));
                }
            }
        }

        public double GetMinuteIndex(TimeSpan time)
        {
            var index = -1.0;

            foreach (var subSession in SubSessions)
            {
                if (subSession.Check(time) == 0)
                {
                    index += (time - subSession.Start.TimeOfDay).TotalMinutes + 1;
                }
                else if (subSession.Check(time) > 0)
                    index += subSession.GetMinutesCount();
            }
            return index;
        }

        public int GetMinutesCount()
        {
            return SubSessions.Sum(s => s.GetMinutesCount());
        }

        public void Shift(int timeZoneIndex)
        {
            SubSessions.ForEach(s =>
            {
                s.Shift(timeZoneIndex);
            });
            TimeZoneIndex = timeZoneIndex;
        }

        public bool IsInTimeSession(DateTime time)
        {
            return SubSessions.Any(subSession => subSession.IsInSubSession(time));
        }

        public DateTime GetLast()
        {
            return SubSessions.OrderBy(s => s.End).Last().End;
        }

        public DateTime GetFirst()
        {
            return SubSessions.OrderBy(s => s.Start).First().Start;
        }

        public SubSession GetFirstSubSession()
        {
            return SubSessions.OrderBy(s => s.Start).First();
        }

        public override string ToString()
        {
            return string.Join("&", SubSessions.Select(sub => sub.ToString()));
        }

    }
}
