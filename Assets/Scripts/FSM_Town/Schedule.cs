using System.Collections.Generic;
using TMPro;

public class Meeting
{
    public Meeting(Location loc, int day, int time, int dur, int attendees, State<Character> sta) { location = loc; this.day = day; this.time = time; duration = dur; this.attendees = attendees; state = sta; }
    public Location location;
    public int day;
    public int time;
    public int duration;
    public int attendees;
    public State<Character> state;
}

public class CalendarDay
{
    public Dictionary<int, Meeting> schedule = new Dictionary<int, Meeting>();
}

public class Notebook
{
    public Notebook()
    {
        calendar = new Dictionary<int, CalendarDay>();
    }

    //public TextMeshProUGUI[] meetings;
    public List<Character> contacts = new();

    public Dictionary<int, CalendarDay> calendar = new Dictionary<int, CalendarDay>();

    public Meeting CheckCalendar(int minute = -1, int hour = -1, int day = -1)
    {
        if (day < 0)
            day = FSM_Clock.Instance.ElapsedDays();
        if (hour < 0)
            hour = FSM_Clock.Instance.TimeOfDayHours();
        if (minute < 0)
            minute = FSM_Clock.Instance.TimeOfDayMinutes();


        if (calendar.ContainsKey(day))
        {
            return calendar.GetValueOrDefault(day).schedule.GetValueOrDefault(hour);
        }
        return null;
    }


    public Meeting CheckCalendarRange(int duration = 1, int hour = -1, int day = -1)
    {
        if (day < 0)
            day = FSM_Clock.Instance.ElapsedDays();
        if (hour < 0)
            hour = FSM_Clock.Instance.TimeOfDayHours();

        //Debug.Log("BACKWARDS CHECK");
        //backwards check
        //day
        for (int d0 = day; 0 < d0; d0--)
        {
            if (calendar.ContainsKey(d0))
            {
                //hour
                int h0 = 23;
                if (d0 == day)
                    h0 = hour;

                while (0 <= h0)
                {
                    if (calendar.GetValueOrDefault(d0).schedule.ContainsKey(h0))
                    {
                        Meeting meeting = calendar.GetValueOrDefault(d0).schedule.GetValueOrDefault(h0);
                        if (meeting.time + meeting.duration -(day-d0)*24 <= hour)
                        {
                            //end backwards search
                            d0 = 0;
                            h0 = 0;
                        }
                        else
                        {
                            return meeting;
                        }
                    }

                    h0--;
                }
            }
        }

        //Debug.Log("FORWARD CHECK");
        //forward check
        int endHour = hour + duration;
        int endDay = day + (int)(endHour/24);
        endHour = endHour % 24;

        //day
        for (int d1 = day; d1 <= endDay; d1++)
        {
            int h1 = 0;
            if (d1 == day)
                h1 = hour;

            if (calendar.ContainsKey(d1))
            {
                //hour
                while (h1 < 24)
                {
                    if (calendar.GetValueOrDefault(d1).schedule.ContainsKey(h1))
                    {

                        return calendar.GetValueOrDefault(d1).schedule.GetValueOrDefault(h1);

                    }

                    h1++;
                    if (d1 == endDay && h1 == endHour)
                        return null;
                }
            }
        }

        return null;
    }

    public int DecreaseAttendees(Meeting meeting)
    {
        if (calendar.ContainsKey(meeting.day))
        {
            CalendarDay calendarDay = calendar.GetValueOrDefault(meeting.day);
            if (calendarDay.schedule.ContainsKey(meeting.time))
            {
                return --calendarDay.schedule.GetValueOrDefault(meeting.time).attendees;
            }
        }
        return 0;
    }

    public bool TryRemoveDate(Meeting meeting)
    {
        return TryRemoveDate(meeting.day, meeting.time, meeting.duration, meeting.location, meeting.state);
    }

    public bool TryRemoveDate(int day, int time, int duration, Location place, State<Character> state)
    {
        if (calendar.ContainsKey(day))
        {
            CalendarDay calendarDay = calendar.GetValueOrDefault(day);
            if (calendarDay.schedule.ContainsKey(time))
            {
                calendarDay.schedule.Remove(time);
                return true;
            }
        }
        return false;
    }

    public bool TryAddDate(Meeting meeting)
    {
        Location loc = meeting.location;
        State<Character> sta = meeting.state;
        return TryAddDate(meeting.day, meeting.time, meeting.duration, meeting.location, meeting.state, meeting.attendees);
    }

    int maxApoinmentsPerDay = 6;
    public bool TryAddDate(int day, int time, int duration, Location place, State<Character> state, int attendees = 0)
    {
        Meeting occupyingMeeting = CheckCalendarRange(duration, time, day);
        if (occupyingMeeting == null)
        {
            CalendarDay calendarDay = new CalendarDay();
            if (calendar.ContainsKey(day))
            {
                calendarDay = calendar.GetValueOrDefault(day);
            }
            else
            {
                calendar.Add(day, calendarDay);
            }
            if (!calendarDay.schedule.ContainsKey(time) && calendarDay.schedule.Count <= maxApoinmentsPerDay)
            {
                calendarDay.schedule.Add(time, new Meeting(place, day, time, duration, attendees, state));
                return true;
            }
            //if (!calendarDay.schedule.ContainsKey(time) && calendarDay.schedule.Count <= maxApoinmentsPerDay)
            //{
            //    calendarDay.schedule.Add(time, new Meeting(place, day, time, duration, state));
            //    return true;
            //}
        }
        return false;
    }
}
