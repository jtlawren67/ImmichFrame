using Ical.Net;
using ImmichFrame.WebApi.Helpers;
using NUnit.Framework;

namespace ImmichFrame.Core.Tests.Helpers;

[TestFixture]
public class CalendarExtensionMethodsTests
{
    [Test]
    public void ToAppointment_RecurringOccurrence_UsesOccurrenceTimesInsteadOfSeriesStart()
    {
        const string calendarData = """
            BEGIN:VCALENDAR
            VERSION:2.0
            BEGIN:VEVENT
            UID:weekly-meeting
            DTSTART:20200106T090000
            DTEND:20200106T100000
            RRULE:FREQ=WEEKLY;COUNT=400
            SUMMARY:Weekly meeting
            LOCATION:Conference room
            END:VEVENT
            END:VCALENDAR
            """;
        var calendar = Calendar.Load(calendarData);
        var occurrence = calendar.GetOccurrences(
                new DateTime(2026, 7, 6),
                new DateTime(2026, 7, 7))
            .Single();

        var appointment = occurrence.ToAppointment();

        Assert.Multiple(() =>
        {
            Assert.That(appointment.StartTime, Is.EqualTo(new DateTime(2026, 7, 6, 9, 0, 0)));
            Assert.That(appointment.EndTime, Is.EqualTo(new DateTime(2026, 7, 6, 10, 0, 0)));
            Assert.That(appointment.Summary, Is.EqualTo("Weekly meeting"));
            Assert.That(appointment.Location, Is.EqualTo("Conference room"));
        });
    }
}
