using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Newtonsoft.Json;

public class Flight
{
    public string FlightNumber { get; set; }
    public string Airline { get; set; }
    public string Destination { get; set; }
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public FlightStatus Status { get; set; }
    public TimeSpan Duration { get; set; }
    public string AircraftType { get; set; }
    public string Terminal { get; set; }
}

public enum FlightStatus
{
    OnTime,
    Delayed,
    Cancelled,
    Boarding,
    InFlight
}

// Info for flifght

public class FlightInformationSystem
{
    private List<Flight> flights;

    public FlightInformationSystem()
    {
        flights = new List<Flight>();
    }

    public void AddFlight(Flight flight)
    {
        flights.Add(flight);
    }
    public class FlightData
    {
        [JsonProperty("flights")]
        public List<Flight> Flights { get; set; }
    }

    public void RemoveFlight(string flightNumber)
    {
        var flightToRemove = flights.FirstOrDefault(f => f.FlightNumber == flightNumber);
        if (flightToRemove != null)
            flights.Remove(flightToRemove);
    }

    public List<Flight> SearchFlightsByAirline(string airline)
    {
        return flights.Where(f => f.Airline == airline)
                      .OrderBy(f => f.DepartureTime)
                      .ToList();
    }

    public List<Flight> SearchDelayedFlights()
    {
        return flights.Where(f => f.Status == FlightStatus.Delayed)
                      .OrderBy(f => f.DepartureTime)
                      .ToList();
    }

    public List<Flight> SearchFlightsByDepartureDate(DateTime departureDate)
    {
        return flights.Where(f => f.DepartureTime.Date == departureDate.Date)
                      .OrderBy(f => f.DepartureTime)
                      .ToList();
    }

    public List<Flight> SearchFlightsByTimeAndDestination(DateTime startTime, DateTime endTime, string destination)
    {
        return flights.Where(f => f.DepartureTime >= startTime && f.DepartureTime <= endTime
                                && f.Destination == destination)
                      .OrderBy(f => f.DepartureTime)
                      .ToList();
    }

    public List<Flight> SearchRecentArrivals(DateTime endTime, DateTime endTimes)
    {
        var startTime = endTime.AddHours(-1); // One hour ago
        return flights.Where(f => f.ArrivalTime >= startTime && f.ArrivalTime <= endTime)
                      .OrderBy(f => f.ArrivalTime)
                      .ToList();
    }


    public void LoadFlightsFromJson(string filePath)
    {
        try
        {
            string jsonData = File.ReadAllText(filePath);
            var flightData = JsonConvert.DeserializeObject<FlightData>(jsonData);

            if (flightData != null)
            {
                // Валідує окремі рейси
                foreach (var flight in flightData.Flights)
                {
                    if (ValidateFlight(flight))
                    {
                        flights.Add(flight);
                    }
                    else
                    {
                        Console.WriteLine("Недійсні дані рейсу знайдено: {0}", flight);
                        // 0 рейс
                    }
                }

                Console.WriteLine("Рейси завантажено успішно.");
            }
            else
            {
                Console.WriteLine("Помилка: не знайдено дійсних даних рейсу в файлі JSON.");
            }
        }
        catch (JsonSerializationException ex)
        {
            Console.WriteLine($"Помилка завантаження рейсів з JSON: {ex.Message}");
            // -1 JSON
        }
    }

    private bool ValidateDateTimeFormat(string dateTimeString)
    {
        DateTime parsedDateTime;
        if (DateTime.TryParseExact(dateTimeString, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDateTime))
        {
            return true;
        }

        Console.WriteLine("Помилка: Невірний формат дати та часу.");
        return false;
    }


    private bool ValidateFlight(Flight flight)
    {
        if (flight.FlightNumber.Length == 5 && 
    char.IsLetter(flight.FlightNumber[0]) &&
    char.IsLetter(flight.FlightNumber[1]) &&
    char.IsDigit(flight.FlightNumber[2]) &&
    char.IsDigit(flight.FlightNumber[3]) &&
    char.IsDigit(flight.FlightNumber[4]))
        {
        }
        else
        {
            return false;
        }

        if (string.IsNullOrEmpty(flight.Airline))
                {
                    return false;
                }

                if (string.IsNullOrEmpty(flight.Destination))
                {
                    return false;
                }

        if (flight.ArrivalTime > flight.DepartureTime)
        {
            TimeSpan duration = flight.ArrivalTime - flight.DepartureTime;

            // Перевірка тривалості рейсу
            if (duration == flight.Duration)
            {
            }
            else
            {
                return false;
            }
        }



        return true;

    }

    public string SerializeFlightsToJson()
    {
        try
        {
            return JsonConvert.SerializeObject(flights, Newtonsoft.Json.Formatting.Indented);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error serializing flights to JSON: {ex.Message}");
            return null;
        }
    }
}

class Program
{
    static void Main()
    {
        // Load Flights.json
        var flightSystem = new FlightInformationSystem();
        flightSystem.LoadFlightsFromJson("flights.json");

        // Add new flight
        var newFlight = new Flight
        {
            FlightNumber = "123",
            Airline = "AmericanAirlenes",
            Destination = "New York",
            DepartureTime = DateTime.Now.AddHours(2),
            ArrivalTime = DateTime.Now.AddHours(4),
            Status = FlightStatus.OnTime,
            Duration = TimeSpan.FromHours(2),
            AircraftType = "F16",
            Terminal = "1"
        };

        flightSystem.AddFlight(newFlight);

        // Delete
        flightSystem.RemoveFlight("BA560");

        // Search

        Console.WriteLine("Enter an option (1-5):");
        Console.WriteLine("1 - Flights by airline");
        Console.WriteLine("2 - Delayed flights");
        Console.WriteLine("3 - Flights on a specific date");
        Console.WriteLine("4 - Flights in a specific time range to a specific destination");
        Console.WriteLine("5 - Recent arrivals");

        if (int.TryParse(Console.ReadLine(), out int option))
        {
            switch (option)
            {
                case 1:
                    Console.WriteLine("Flights by airline:");
                    var airlineFlights = flightSystem.SearchFlightsByAirline("MAU");
                    PrintFlights(airlineFlights);
                    break;

                case 2:
                    Console.WriteLine("Delayed flights:");
                    var delayedFlights = flightSystem.SearchDelayedFlights();
                    PrintFlights(delayedFlights);
                    break;

                case 3:
                    DateTime specificDate = new DateTime(2023, 1, 1);
                    Console.WriteLine("Flights on a specific date:");
                    var specificDateFlights = flightSystem.SearchFlightsByDepartureDate(specificDate.Date);
                    PrintFlights(specificDateFlights);

                    //                    Console.WriteLine("Flights on a specific date:");
                    //                    var specificDateFlights = flightSystem.SearchFlightsByDepartureDate(DateTime.Now.Date);
                    //                    PrintFlights(specificDateFlights);
                    break;

                case 4:
                    DateTime specificDates; 
                    specificDates = new DateTime(2023, 3, 7);

                    DateTime startTime = new DateTime(specificDates.Year, specificDates.Month, specificDates.Day, 6, 0, 0);
                    DateTime endTime = new DateTime(specificDates.Year, specificDates.Month, specificDates.Day, 7, 0, 0);

                    Console.WriteLine("Flights in a specific time range to a specific destination:");
                    var timeAndDestinationFlights = flightSystem.SearchFlightsByTimeAndDestination(startTime, endTime, "Odesa");
                    PrintFlights(timeAndDestinationFlights);
                    break;

                case 5:
                    Console.WriteLine("Recent arrivals:");
                    DateTime specificDatess = new DateTime(2023, 3, 7); 
                    DateTime startTimes = new DateTime(specificDatess.Year, specificDatess.Month, specificDatess.Day, 6, 0, 0);
                    DateTime endTimes = new DateTime(specificDatess.Year, specificDatess.Month, specificDatess.Day, 7, 0, 0);

                    Console.WriteLine("Flights in a specific time range:");
                    var timeRangeFlights = flightSystem.SearchRecentArrivals(startTimes, endTimes);
                    PrintFlights(timeRangeFlights);


                    break;

                default:
                    Console.WriteLine("Invalid option.");
                    break;
            }
        }
        else
        {
            Console.WriteLine("Invalid input. Please enter a number.");

            // Save
            var serializedData = flightSystem.SerializeFlightsToJson();
            File.WriteAllText("flights_updated.json", serializedData);
        }

        static void PrintFlights(List<Flight> flights)
        {
            int visibleRows = Console.WindowHeight - 1;
            int startIndex = 0;

            while (startIndex < flights.Count)
            {
                int endIndex = Math.Min(startIndex + visibleRows, flights.Count);
                for (int i = startIndex; i < endIndex; i++)
                {
                    Console.WriteLine($"{flights[i].FlightNumber} - {flights[i].Airline} - {flights[i].Destination} - " +
                                      $"{flights[i].DepartureTime} - {flights[i].ArrivalTime} - {flights[i].Status}");
                }

                startIndex = endIndex;

                // Scrollbar
                if (endIndex < flights.Count)
                {
                    Console.WriteLine("Press Enter for more...");
                    Console.ReadLine();
                    Console.Clear();
                }
            }
        }
    }
}
