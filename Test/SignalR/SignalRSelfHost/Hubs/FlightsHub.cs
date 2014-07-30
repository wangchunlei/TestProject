using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace SignalRSelfHost.Hubs
{
    [HubName("flights")]
    public class FlightsHub : Hub
    {
        private static bool Flying;
        private static readonly object Locker = new object();

        public void Go()
        {
            if (Flying) return;
            lock (Locker)
            {
                if (Flying) return;
                Flying = true;
            }

            var towns = new Dictionary<string, PointF>
            {
                {"Turin", new PointF(45.2008018F, 7.6496301F)},
                {"Berlin", new PointF(52.3800011F, 13.5225F)},
                {"Madrid", new PointF(40.4936F, -3.56676F)},
                {"New York", new PointF(40.639801F, -73.7789002F)},
                {"Istanbul", new PointF(40.9768982F, 28.8146F)},
                {"Paris", new PointF(49.0127983F, 2.55F)},
                {"Cape Town", new PointF(-33.9648018F, 18.6016998F)},
            };

            var flights = new[]
            {
                new
                {
                    Code = "LH100",
                    Color = "red",
                    From = "Turin",
                    To = "Berlin",
                    Speed = 10,
                    Period = 500,
                },
                new
                {
                    Code = "AZ150",
                    Color = "blue",
                    From = "Turin",
                    To = "Madrid",
                    Speed = 8,
                    Period = 1500,
                },
                new
                {
                    Code = "XX150",
                    Color = "green",
                    From = "New York",
                    To = "Istanbul",
                    Speed = 30,
                    Period = 400,
                },
                new
                {
                    Code = "AA777",
                    Color = "orange",
                    From = "Paris",
                    To = "Cape Town",
                    Speed = 35,
                    Period = 2000,
                }
            };

            const int simulationFactor = 20; //1 hours = X seconds

            var signals = from flight in flights.ToObservable()
                let f = towns[flight.From]
                let t = towns[flight.To]
                let dx = t.X - f.X
                let dy = t.Y - f.Y
                let d = Math.Sqrt(
                    Math.Pow(dx, 2) + Math.Pow(dy, 2))
                let time = d/flight.Speed
                let sampling = (double) 1000/flight.Period
                let steps = (int) (sampling*simulationFactor*time)
                            + 1
                let sx = dx/steps
                let sy = dy/steps
                let paused = (int) (0.5*simulationFactor*sampling)
                let outbound = from i in Enumerable.Range(0, steps)
                    select new
                    {
                        Index = i,
                        Landed = false,
                        Touched = i == steps - 1,
                        Town = flight.To
                    }
                let delay1 = from i in Enumerable.Repeat(steps, paused)
                    select new
                    {
                        Index = i,
                        Landed = true,
                        Touched = false,
                        Town = flight.To
                    }
                let inbound = from i in Enumerable.Range(0, steps)
                    select new
                    {
                        Index = steps - i,
                        Landed = false,
                        Touched = i == steps - 1,
                        Town = flight.From
                    }
                let delay2 = from i in Enumerable.Repeat(0, paused)
                    select new
                    {
                        Index = i,
                        Landed = true,
                        Touched = false,
                        Town = flight.From
                    }
                let route = outbound.Concat(delay1)
                    .Concat(inbound)
                    .Concat(delay2).ToArray()
                from st in Observable.Interval(
                    TimeSpan.FromMilliseconds(flight.Period))
                let w = route[st%route.Length]
                let s = w.Index
                let x = f.X + sx*s
                let y = f.Y + sy*s
                let landed = w.Landed
                select new
                {
                    flight.Code,
                    w.Touched,
                    w.Town,
                    Landed = landed,
                    X = x,
                    Y = y,
                    Color = landed
                        ? "black"
                        : flight.Color,
                };

            signals.Subscribe(
                flight =>
                {
                    Clients.All.sample(flight.Code,
                        flight.X, flight.Y,
                        flight.Color, flight.Landed);
                    if (!flight.Touched) return;
                    Trace.WriteLine(
                        string.Format("Notifying {0}...",
                            flight.Code));
                    Clients.Group(flight.Code).touched(flight.Code,
                        flight.Town,
                        DateTime.Now.ToLongTimeString());
                });
        }
    }
}