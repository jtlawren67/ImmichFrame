using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using ImmichFrame.Core.Interfaces;
using ImmichFrame.Core.Models;

namespace ImmichFrame.WebApi.Helpers
{
    public static class CalendarExtensionMethods
    {
        public static IAppointment ToAppointment(this Occurrence occurrence)
        {
            var appointment = new Appointment
            {
                StartTime = occurrence.Period.StartTime.AsSystemLocal,
                Duration = occurrence.Period.Duration,
                EndTime = occurrence.Period.EndTime.AsSystemLocal,
                Location = ""
            };

            // A recurring event's source contains the series' original DTSTART.
            // Use the occurrence period above for timing, then copy only metadata
            // from the source event.
            if (occurrence.Source is CalendarEvent calEvent)
            {
                appointment.Summary = calEvent.Summary;
                appointment.Description = calEvent.Description;
                appointment.Location = calEvent.Location;
            }

            return appointment;
        }
        public static IAppointment ToAppointment(this CalendarEvent calEvent)
        {
            return new Appointment
            {
                Summary = calEvent.Summary,
                Description = calEvent.Description,
                StartTime = calEvent.Start.AsSystemLocal,
                Duration = calEvent.Duration,
                EndTime = calEvent.End.AsSystemLocal,
                Location = calEvent.Location
            };
        }
    }
}
