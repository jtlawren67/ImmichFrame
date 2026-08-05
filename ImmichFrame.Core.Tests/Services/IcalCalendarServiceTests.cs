using System.Net;
using System.Net.Http;
using ImmichFrame.Core.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;

namespace ImmichFrame.Core.Tests.Services;

[TestFixture]
public class IcalCalendarServiceTests
{
    [Test]
    public async Task GetAppointments_ExcludesAppointmentsThatHaveAlreadyEnded()
    {
        var now = DateTime.Now;
        var calendarData = $"""
            BEGIN:VCALENDAR
            VERSION:2.0
            BEGIN:VEVENT
            UID:past
            DTSTART:{now.AddMinutes(-2):yyyyMMddTHHmmss}
            DTEND:{now.AddMinutes(-1):yyyyMMddTHHmmss}
            SUMMARY:Past appointment
            END:VEVENT
            BEGIN:VEVENT
            UID:future
            DTSTART:{now.AddMinutes(1):yyyyMMddTHHmmss}
            DTEND:{now.AddMinutes(2):yyyyMMddTHHmmss}
            SUMMARY:Future appointment
            END:VEVENT
            END:VCALENDAR
            """;
        var settings = new Mock<IGeneralSettings>();
        settings.SetupGet(x => x.Webcalendars).Returns(["https://calendar.example.test/calendar.ics"]);
        settings.SetupGet(x => x.WebcalendarLookaheadDays).Returns(2);

        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory
            .Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(new HttpClient(new CalendarResponseHandler(calendarData)));

        var service = new IcalCalendarService(
            settings.Object,
            NullLogger<IcalCalendarService>.Instance,
            httpClientFactory.Object);

        var appointments = await service.GetAppointments();

        Assert.That(appointments.Select(x => x.Summary), Is.EqualTo(["Future appointment"]));
    }

    private sealed class CalendarResponseHandler(string calendarData) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(calendarData)
            });
    }
}
