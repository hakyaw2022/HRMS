using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HRMS.Data;
using HRMS.Models;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Numerics;
using NuGet.Protocol;
using Microsoft.AspNetCore.Authorization;
using HRMS.Helpers;

namespace HRMS.Controllers
{
    [Authorize(Roles = "Admin,Manager,Receptionist")]
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private int price;
        private string commentPrice;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Bookings
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Booking.
                Where(b => b.BookingStatus == BookingStatus.Booked)
                //Where(s => s.BookingStatus != BookingStatus.Cancelled).
                //Where(s => s.BookingStatus != BookingStatus.Finished).
                //Where(s => s.BookingStatus != BookingStatus.BookedService).
                .Include(b => b.Agent).Include(b => b.Guest).Include(b => b.Room).OrderByDescending(s => s.CreatedDate);
            try
            {
                return View(await applicationDbContext.ToListAsync());
            }
            catch
            {
                return View();
            }

        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Booking == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var booking = await _context.Booking
                .Include(b => b.Agent)
                .Include(b => b.Guest)
                .Include(b => b.Room)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (booking == null)
            {
                RedirectToAction("Error", "Home");
            }

            return View(booking);
        }

        // GET: Bookings/Create
        public IActionResult CreatePeriod()
        {
            ViewData["RoomTypeId"] = new SelectList(_context.RoomType, "Id", "Name");
            return View();
        }

        // GET: Bookings Create walking
        public IActionResult CreatePeriodWalkin()
        {
            var today = Utility.GetMMTimeNow().ToString("yyyy-MM-dd");
            ViewBag.Today = today;
            ViewData["RoomTypeId"] = new SelectList(_context.RoomType, "Id", "Name");
            return View();
        }

        public IActionResult CreatePeriodForServiceBooking()
        {
            ViewData["Service"] = _context.Service.ToList();
            ViewData["Guest"] = new SelectList(_context.Guest, "Id", "Name");
            ViewData["Message"] = null;
            return View();
        }

        public async Task<IActionResult> SearchAvailableServices(IFormCollection form)
        {
            int serviceId = int.Parse(form["ServiceId"]);
            List<Booking> bookedServices = await _context.Booking.
                Where(s => s.BookingStatus == BookingStatus.BookedService).
                Where(s => s.RoomId == serviceId).
                ToListAsync();
            string Date = form["Date"];
            string FromHour = form["FromHour"];
            string FromMinute = form["FromMinute"];
            string ToHour = form["ToHour"];
            string ToMinute = form["ToMinute"];
            int FromInt = int.Parse(FromHour) * 60 + int.Parse(FromMinute);
            int ToInt = int.Parse(ToHour) * 60 + int.Parse(ToMinute);
            var serviceChosen = _context.Service.Where(s => s.Id == serviceId).First();
            string OpenedTime = serviceChosen.OpenedTime;
            var OpenedTimeSplit = OpenedTime.Split(':');
            int OpenedTimeInt = int.Parse(OpenedTimeSplit[0]) * 60 + int.Parse(OpenedTimeSplit[1]);

            string ClosedTime = serviceChosen.ClosedTime;
            var ClosedTimeSplit = ClosedTime.Split(":");
            int ClosedTimeInt = int.Parse(ClosedTimeSplit[0]) * 60 + int.Parse(ClosedTimeSplit[1]);
            if (FromInt < OpenedTimeInt || ToInt > ClosedTimeInt)
            {
                ViewData["Service"] = _context.Service.ToList();
                ViewData["Guest"] = new SelectList(_context.Guest, "Id", "Name");
                ViewData["Message"] = "Chosen service is not available for chosen time frame";
                return View(nameof(CreatePeriodForServiceBooking));
            }
            try
            {
                var Split = Date.Split('-');
                int Year = int.Parse(Split[0]);
                int Month = int.Parse(Split[1]);
                int Day = int.Parse(Split[2]);
                DateTime From = new(Year, Month, Day, int.Parse(FromHour), int.Parse(FromMinute), 0);
                DateTime To = new(Year, Month, Day, int.Parse(ToHour), int.Parse(ToMinute), 0);
                if (From < DateTime.Now)
                {
                    ViewData["Service"] = _context.Service.ToList();
                    ViewData["Guest"] = new SelectList(_context.Guest, "Id", "Name");
                    ViewData["Message"] = "Please choose date and time later than right now";
                    return View(nameof(CreatePeriodForServiceBooking));
                }
                int availableCapacity = _context.Service.Where(s => s.Id == serviceId).First().Capacity;
                foreach (Booking booking in bookedServices)
                {
                    if (From >= booking.From && From <= booking.To)
                    {
                        availableCapacity--;
                    }
                }
                if (availableCapacity < 1)
                {
                    ViewData["Service"] = _context.Service.ToList();
                    ViewData["Guest"] = new SelectList(_context.Guest, "Id", "Name");
                    ViewData["Message"] = "The service is fully booked for chosen time frame";
                    return View(nameof(CreatePeriodForServiceBooking));
                }
                else
                {
                    string guestId = form["GuestId"];
                    Booking newBooking = new();
                    newBooking.From = From;
                    newBooking.To = To;
                    newBooking.BookingStatus = BookingStatus.BookedService;
                    newBooking.RoomId = serviceId;
                    newBooking.GuestId = int.Parse(guestId);
                    newBooking.Comment = form["Comment"];
                    newBooking.CreatedDate = DateTime.Now;
                    _context.Add(newBooking);
                    await _context.SaveChangesAsync();
                }
                ViewData["Service"] = _context.Service.ToList();
                ViewData["Guest"] = new SelectList(_context.Guest, "Id", "Name");
                ViewData["Message"] = "Successfully booked your selected service";
                return View(nameof(CreatePeriodForServiceBooking));
            }
            catch (Exception)
            {
                ViewData["Service"] = new SelectList(_context.Service, "Id", "Name");
                ViewData["Guest"] = new SelectList(_context.Guest, "Id", "Name");
                ViewData["Message"] = "Please choose the date";
                return View(nameof(CreatePeriodForServiceBooking));
            }

        }


        public async Task<IActionResult> BookedServices()
        {


            ViewBag.Service = await _context.Service.ToListAsync();

            return View(await _context.Booking.
                Where(s => s.BookingStatus == BookingStatus.BookedService).
                Include(s => s.Guest).
                ToListAsync());
        }


        public async Task<IActionResult> StartService(int? id)
        {
            var booking = _context.Booking.Where(s => s.Id == id).First();
            int guestId = booking.GuestId;
            int serviceId = booking.RoomId;
            var service = _context.Service.Where(s => s.Id == serviceId).First();
            var serviceType = service.PricingType;
            if (serviceType == PricingType.Session)
            {
                price = service.Price;
                commentPrice = service.Price.ToString();
            }
            else
            {
                TimeSpan t = booking.To.Subtract(booking.From);
                price = t.Hours * service.Price;
                commentPrice = service.Price.ToString() + "x" + t.Hours.ToString();
            }
            try
            {
                var existCheckedIn = _context.CheckedInCustomer.Where(s => s.GuestId == guestId).First();
                Transaction transaction = new();
                transaction.Amount = price;
                transaction.RoomId = existCheckedIn.RoomId;
                transaction.TransactionStatus = TransactionStatus.Active;
                transaction.Comment = service.Name + "-" + commentPrice;
                transaction.TransactionType = TransactionType.Service;
                transaction.CreatedDate = DateTime.Now;
                _context.Transaction.Add(transaction);
                await _context.SaveChangesAsync();
                _context.Booking.Remove(booking);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                Transaction transaction = new();
                transaction.Amount = price;
                transaction.TransactionStatus = TransactionStatus.Inactive;
                transaction.Comment = service.Name + "-" + commentPrice;
                transaction.TransactionType = TransactionType.Service;
                transaction.CreatedDate = DateTime.Now;
                _context.Transaction.Add(transaction);
                await _context.SaveChangesAsync();
                _context.Booking.Remove(booking);
                await _context.SaveChangesAsync();
                ViewBag.Name = _context.Guest.Where(s => s.Id == guestId).First().Name;

                Receipt receipt = new();
                receipt.Amount = price;
                receipt.ReceiptNumber =
                receipt.Name = transaction.Comment;
                receipt.CreatedDate = transaction.CreatedDate;
                receipt.TransactionType = TransactionType.Service;
                _context.Receipt.Add(receipt);
                await _context.SaveChangesAsync();
                return View("Receipt", transaction);
            }
            return RedirectToAction(nameof(BookedServices));
        }

        public async Task<IActionResult> SearchAvailableRooms([Bind("RoomTypeId")] Room room, IFormCollection form)
        {
            var roomTypeId = room.RoomTypeId;
            var checkedInOutTime = await _context.CheckedInOutTime.FirstOrDefaultAsync(t => t.RoomTypeId == roomTypeId);

            // if checkedInOutTime is null, redirect to CreatePeriod page
            if (checkedInOutTime == null)
            {
                ModelState.AddModelError("Id", "Selected Room Type does not have check in/out time setup yet!");
                ViewData["RoomTypeId"] = new SelectList(_context.RoomType, "Id", "Name");
                return View("CreatePeriod", new Room());
            }

            var checkedInTime = checkedInOutTime!.CheckedInTime;
            var checkedOutTime = checkedInOutTime!.CheckedOutTime;

            string fromDate = form["CheckedInDate"];
            string toDate = form["CheckedOutDate"];

            ViewData["FromDate"] = fromDate;
            ViewData["ToDate"] = toDate;

            var fromDateTime = DateTime.Parse(fromDate + " " + checkedInTime);
            var toDateTime = DateTime.Parse(toDate + " " + checkedOutTime);

            if (fromDateTime >= toDateTime)
            {
                ModelState.AddModelError("Id", "Invalid date selections! Check out date should be later than check in date.");
                ViewData["RoomTypeId"] = new SelectList(_context.RoomType, "Id", "Name");
                return View("CreatePeriod", new Room());
            }

            //string fromTime = form["CheckedInHour"] + ":" + form["CheckedInMinute"];
            //string toTime = form["CheckedOutHour"] + ":" + form["CheckedOutMinute"];
            //string from = fromDate + " " + fromTime;

            //var fromSplit = fromDate.Split('-');
            //int fromYear = int.Parse(fromSplit[0]);
            //int fromMonth = int.Parse(fromSplit[1]);
            //int fromDay = int.Parse(fromSplit[2]);

            //DateTime fromDateTime = new(fromYear, fromMonth, fromDay, int.Parse(form["CheckedInHour"]), int.Parse(form["CheckedInMinute"]), 0);

            //var to = toDate + ":" + toTime;

            var bookingType = form["BookingType"];
            var roomsBooked = new List<Booking>();

            /*
            if (fromDate != toDate)
            {
                roomsBooked = await _context.Booking.Where(s => s.Room.RoomTypeId == roomTypeId).
                     Where(s => s.BookingStatus != BookingStatus.Cancelled).
                     Where(s => s.BookingStatus != BookingStatus.Finished).
                     Where(s => s.BookingStatus != BookingStatus.BookedService).
                     Where(s => fromDateTime <= s.To).ToListAsync();
            }
            else
            {
                roomsBooked = await _context.Booking.Where(s => s.Room.RoomTypeId == roomTypeId).
                    Where(s => s.BookingStatus != BookingStatus.Cancelled).
                    Where(s => s.BookingStatus != BookingStatus.Finished).
                    Where(s => s.BookingStatus != BookingStatus.BookedService).
                    Where(s => fromDateTime >= s.From && fromDateTime < s.To).ToListAsync();
                if (roomsBooked.Any())
                {
                    foreach (var roomBooked in roomsBooked)
                    {
                        if (fromDateTime <= roomBooked.From)
                        {
                            roomsBooked.Remove(roomBooked);
                        }
                    }
                }
            }
            */

            roomsBooked = await _context.Booking.Where(b => b.Room.RoomTypeId == roomTypeId).
                    Where(b => b.BookingStatus != BookingStatus.Cancelled).
                    Where(b => b.BookingStatus != BookingStatus.Finished).
                    Where(b => b.BookingStatus != BookingStatus.BookedService).
                    Where(b => fromDateTime <= b.To && toDateTime >= b.From).ToListAsync();

            List<Room> rooms = await _context.Room.Where(s => s.RoomType.Id == roomTypeId)
                                                    .Include(t => t.RoomType).ToListAsync();

            rooms.RemoveAll(r => roomsBooked.Select(rb => rb.Room).Contains(r));

            /*
            if (roomsBooked.Any())
            {
                foreach (var roomBooked in roomsBooked)
                {
                    rooms.Remove(rooms.First(s => s.Id == roomBooked.RoomId));
                }
            }
            */




            ViewData["Agents"] = new SelectList(
                                                _context.Agent.Select(
                                                    a => new
                                                    {
                                                        Id = a.Id,
                                                        Name = a.Name + " - " + a.NRC
                                                    }).OrderBy(aa => aa.Name),
                                                    "Id", "Name");
            ViewData["Guests"] = new SelectList(_context.Guest.Select(
                                                    g => new
                                                    {
                                                        Id = g.Id,
                                                        Name = g.Name + " - " + g.NRC
                                                    }).OrderBy(gg => gg.Name),
                                                    "Id", "Name");
            ViewData["FromDate"] = fromDate;
            ViewData["ToDate"] = toDate;
            ViewData["FromTime"] = checkedInTime;
            ViewData["ToTime"] = checkedOutTime;
            ViewData["RoomTypeId"] = roomTypeId;

            return View(rooms);
        }

        public async Task<IActionResult> SearchAvailableRoomsWalkinCheckin([Bind("RoomTypeId")] Room room, IFormCollection form)
        {
            var roomTypeId = room.RoomTypeId;
            var checkedInOutTime = await _context.CheckedInOutTime.FirstOrDefaultAsync(t => t.RoomTypeId == roomTypeId);

            // if checkedInOutTime is null, redirect to CreatePeriod page
            if (checkedInOutTime == null)
            {
                ModelState.AddModelError("Id", "Selected Room Type does not have check in/out time setup yet!");
                ViewData["RoomTypeId"] = new SelectList(_context.RoomType, "Id", "Name");
                return View("CreatePeriod", new Room());
            }

            var checkedInTime = checkedInOutTime!.CheckedInTime;
            var checkedOutTime = checkedInOutTime!.CheckedOutTime;

            string fromDate = form["CheckedInDate"];
            string toDate = form["CheckedOutDate"];

            ViewData["FromDate"] = fromDate;
            ViewData["ToDate"] = toDate;

            var fromDateTime = DateTime.Parse(fromDate + " " + checkedInTime);
            var toDateTime = DateTime.Parse(toDate + " " + checkedOutTime);

            if (fromDateTime >= toDateTime)
            {
                ModelState.AddModelError("Id", "Invalid date selections! Check out date should be later than check in date.");
                ViewData["RoomTypeId"] = new SelectList(_context.RoomType, "Id", "Name");
                return View("CreatePeriod", new Room());
            }

            //string fromTime = form["CheckedInHour"] + ":" + form["CheckedInMinute"];
            //string toTime = form["CheckedOutHour"] + ":" + form["CheckedOutMinute"];
            //string from = fromDate + " " + fromTime;

            //var fromSplit = fromDate.Split('-');
            //int fromYear = int.Parse(fromSplit[0]);
            //int fromMonth = int.Parse(fromSplit[1]);
            //int fromDay = int.Parse(fromSplit[2]);

            //DateTime fromDateTime = new(fromYear, fromMonth, fromDay, int.Parse(form["CheckedInHour"]), int.Parse(form["CheckedInMinute"]), 0);

            //var to = toDate + ":" + toTime;

            var bookingType = form["BookingType"];
            var roomsBooked = new List<Booking>();

            /*
            if (fromDate != toDate)
            {
                roomsBooked = await _context.Booking.Where(s => s.Room.RoomTypeId == roomTypeId).
                     Where(s => s.BookingStatus != BookingStatus.Cancelled).
                     Where(s => s.BookingStatus != BookingStatus.Finished).
                     Where(s => s.BookingStatus != BookingStatus.BookedService).
                     Where(s => fromDateTime <= s.To).ToListAsync();
            }
            else
            {
                roomsBooked = await _context.Booking.Where(s => s.Room.RoomTypeId == roomTypeId).
                    Where(s => s.BookingStatus != BookingStatus.Cancelled).
                    Where(s => s.BookingStatus != BookingStatus.Finished).
                    Where(s => s.BookingStatus != BookingStatus.BookedService).
                    Where(s => fromDateTime >= s.From && fromDateTime < s.To).ToListAsync();
                if (roomsBooked.Any())
                {
                    foreach (var roomBooked in roomsBooked)
                    {
                        if (fromDateTime <= roomBooked.From)
                        {
                            roomsBooked.Remove(roomBooked);
                        }
                    }
                }
            }
            */

            roomsBooked = await _context.Booking.Where(b => b.Room.RoomTypeId == roomTypeId).
                    Where(b => b.BookingStatus != BookingStatus.Cancelled).
                    Where(b => b.BookingStatus != BookingStatus.Finished).
                    Where(b => b.BookingStatus != BookingStatus.BookedService).
                    Where(b => fromDateTime <= b.To && toDateTime >= b.From).ToListAsync();

            List<Room> rooms = await _context.Room.Where(s => s.RoomType.Id == roomTypeId)
                                                    .Include(t => t.RoomType).ToListAsync();

            rooms.RemoveAll(r => roomsBooked.Select(rb => rb.Room).Contains(r));

            /*
            if (roomsBooked.Any())
            {
                foreach (var roomBooked in roomsBooked)
                {
                    rooms.Remove(rooms.First(s => s.Id == roomBooked.RoomId));
                }
            }
            */




            ViewData["Agents"] = new SelectList(
                                                _context.Agent.Select(
                                                    a => new
                                                    {
                                                        Id = a.Id,
                                                        Name = a.Name + " - " + a.NRC
                                                    }).OrderBy(aa => aa.Name),
                                                    "Id", "Name");
            ViewData["Guests"] = new SelectList(_context.Guest.Select(
                                                    g => new
                                                    {
                                                        Id = g.Id,
                                                        Name = g.Name + " - " + g.NRC
                                                    }).OrderBy(gg => gg.Name),
                                                    "Id", "Name");
            ViewData["FromDate"] = fromDate;
            ViewData["ToDate"] = toDate;
            ViewData["FromTime"] = checkedInTime;
            ViewData["ToTime"] = checkedOutTime;
            ViewData["RoomTypeId"] = roomTypeId;

            return View(rooms);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(IFormCollection form)
        {
            string comment = form["Comment"];
            int total = int.Parse(form["total"]);
            string fromDate = form["fromDate"];
            var fromDateSplit = fromDate.Split("-");
            int fromYear = int.Parse(fromDateSplit[0]);
            int fromMonth = int.Parse(fromDateSplit[1]);
            int fromDay = int.Parse(fromDateSplit[2]);
            string fromTime = form["fromTime"];
            var fromTimeSplit = fromTime.Split(":");
            int fromHour = int.Parse(fromTimeSplit[0]);
            int fromMinute = int.Parse(fromTimeSplit[1]);

            DateTime from = new(fromYear, fromMonth, fromDay, fromHour, fromMinute, 0);

            string toDate = form["toDate"];
            var toDateSplit = toDate.Split("-");
            int toYear = int.Parse(toDateSplit[0]);
            int toMonth = int.Parse(toDateSplit[1]);
            int toDay = int.Parse(toDateSplit[2]);
            string toTime = form["toTime"];
            var toTimeSplit = toTime.Split(":");
            int toHour = int.Parse(toTimeSplit[0]);
            int toMinute = int.Parse(toTimeSplit[1]);

            int fromTimeCalculated = fromHour * 60 + fromMinute;
            int toTimeCalculated = toHour * 60 + toMinute;

            DateTime to = new(toYear, toMonth, toDay, toHour, toMinute, 0);
            List<int> roomSelected = new();
            for (int i = 0; i <= total; i++)
            {
                string roomSelect = form[i.ToString()];
                if (roomSelect != null)
                {
                    int count = 0;
                    count++;
                    roomSelected.Add(int.Parse(roomSelect));
                }
            }

            foreach (int i in roomSelected)
            {
                Booking booking = new();
                booking.RoomId = i;
                var roomEach = _context.Room.Where(s => s.Id == i).First();
                var roomTypeSelectedId = roomEach.RoomTypeId;
                int guestId = int.Parse(form["GuestId"]);
                int? agentId = int.TryParse(form["AgentId"], out int tmpAgentId) ? (int?)tmpAgentId : null;

                booking.RoomPrice = _context.RoomType.Where(s => s.Id == roomTypeSelectedId).First().Price;
                booking.Comment = form["Comment"];
                booking.AgentId = agentId;
                booking.GuestId = guestId;
                booking.From = from;
                booking.To = to;
                booking.CreatedDate = DateTime.Now;
                booking.Comment = comment;

                if (fromDate == toDate)
                {
                    if (booking.From.AddMinutes(30) <= DateTime.Now)
                    {
                        Room room = _context.Room.Where(s => s.Id == i).First();
                        room.GuestId = booking.GuestId;
                        room.RoomStatus = RoomStatus.ReservedHourly;
                        _context.Update(room);
                        await _context.SaveChangesAsync();
                        booking.RoomStatusUpdate = true;

                    }

                    booking.BookingStatus = BookingStatus.BookedHourly;
                    int roomTypeId = int.Parse(form["roomTypeId"]);
                    _context.Add(booking);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    if (booking.From.AddMinutes(30) <= DateTime.Now)
                    {
                        Room room = _context.Room.Where(s => s.Id == i).First();
                        room.RoomStatus = RoomStatus.Reserved;
                        room.GuestId = booking.GuestId;
                        _context.Update(room);
                        await _context.SaveChangesAsync();
                        booking.RoomStatusUpdate = true;

                    }

                    int roomTypeId = int.Parse(form["roomTypeId"]);
                    string? checkedInTime = _context.CheckedInOutTime.Where(s => s.RoomTypeId == roomTypeId).First().CheckedInTime;
                    string? checkedOutTime = _context.CheckedInOutTime.Where(s => s.RoomTypeId == roomTypeId).First().CheckedOutTime;
                    if (checkedInTime is not null && checkedOutTime is not null)
                    {
                        var checkedInTimeSplit = checkedInTime.Split(":");
                        int checkedInHour = int.Parse(checkedInTimeSplit[0]);
                        int checkedInMinute = int.Parse(checkedInTimeSplit[1]);
                        int checkedInTimeCalculated = checkedInHour * 60 + checkedInMinute;

                        var checkedOutTimeSplit = checkedOutTime.Split(":");
                        int checkedOutHour = int.Parse(checkedOutTimeSplit[0]);
                        int checkedOutMinute = int.Parse(checkedOutTimeSplit[1]);
                        int checkedOutTimeCalculated = checkedOutHour * 60 + checkedOutMinute;

                        if (fromTimeCalculated < checkedInTimeCalculated && toTimeCalculated <= checkedOutTimeCalculated)
                        {
                            booking.BookingStatus = BookingStatus.BookedEarlyCheckIn;
                        }
                        else if (fromTimeCalculated < checkedInTimeCalculated && toTimeCalculated > checkedOutTimeCalculated)
                        {
                            booking.BookingStatus = BookingStatus.BookedEarlyCheckInAndLateCheckOut;
                        }
                        else if (fromTimeCalculated >= checkedInTimeCalculated && toTimeCalculated <= checkedOutTimeCalculated)
                        {
                            booking.BookingStatus = BookingStatus.Booked;
                        }
                        else if (fromTimeCalculated >= checkedInTimeCalculated && toTimeCalculated > checkedOutTimeCalculated)
                        {
                            booking.BookingStatus = BookingStatus.BookedLateCheckOut;
                        }

                        _context.Add(booking);
                        await _context.SaveChangesAsync();
                    }
                }
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]

        public async Task<IActionResult> BookWalkin(IFormCollection form)
        {
            string comment = form["Comment"];
            int total = int.Parse(form["total"]);
            string fromDate = form["fromDate"];
            var fromDateSplit = fromDate.Split("-");
            int fromYear = int.Parse(fromDateSplit[0]);
            int fromMonth = int.Parse(fromDateSplit[1]);
            int fromDay = int.Parse(fromDateSplit[2]);
            string fromTime = form["fromTime"];
            var fromTimeSplit = fromTime.Split(":");
            int fromHour = int.Parse(fromTimeSplit[0]);
            int fromMinute = int.Parse(fromTimeSplit[1]);

            DateTime from = new(fromYear, fromMonth, fromDay, fromHour, fromMinute, 0);

            string toDate = form["toDate"];
            var toDateSplit = toDate.Split("-");
            int toYear = int.Parse(toDateSplit[0]);
            int toMonth = int.Parse(toDateSplit[1]);
            int toDay = int.Parse(toDateSplit[2]);
            string toTime = form["toTime"];
            var toTimeSplit = toTime.Split(":");
            int toHour = int.Parse(toTimeSplit[0]);
            int toMinute = int.Parse(toTimeSplit[1]);

            int fromTimeCalculated = fromHour * 60 + fromMinute;
            int toTimeCalculated = toHour * 60 + toMinute;

            DateTime to = new(toYear, toMonth, toDay, toHour, toMinute, 0);
            List<int> roomSelected = new();
            int selectedRoomId = 0;

            for (int i = 0; i <= total; i++)
            {
                string roomSelect = form[i.ToString()];
                if (roomSelect != null)
                {
                    int count = 0;
                    count++;
                    roomSelected.Add(int.Parse(roomSelect));
                }
            }

            int guestId = int.Parse(form["GuestId"]);
            foreach (int i in roomSelected)
            {
                Booking booking = new();
                booking.RoomId = i;
                var roomEach = _context.Room.Where(s => s.Id == i).First();
                var roomTypeSelectedId = roomEach.RoomTypeId;
                int? agentId = int.TryParse(form["AgentId"], out int tmpAgentId) ? (int?)tmpAgentId : null;

                booking.RoomPrice = _context.RoomType.Where(s => s.Id == roomTypeSelectedId).First().Price;
                booking.Comment = form["Comment"];
                booking.AgentId = agentId;
                booking.GuestId = guestId;
                booking.From = from;
                booking.To = to;
                booking.CreatedDate = DateTime.Now;
                booking.Comment = comment;

                Room room = _context.Room.Where(s => s.Id == i).First();
                room.RoomStatus = RoomStatus.Reserved;
                room.GuestId = booking.GuestId;
                _context.Update(room);
                await _context.SaveChangesAsync();
                booking.RoomStatusUpdate = true;


                int roomTypeId = int.Parse(form["roomTypeId"]);
                string? checkedInTime = _context.CheckedInOutTime.Where(s => s.RoomTypeId == roomTypeId).First().CheckedInTime;
                string? checkedOutTime = _context.CheckedInOutTime.Where(s => s.RoomTypeId == roomTypeId).First().CheckedOutTime;
                if (checkedInTime is not null && checkedOutTime is not null)
                {
                    var checkedInTimeSplit = checkedInTime.Split(":");
                    int checkedInHour = int.Parse(checkedInTimeSplit[0]);
                    int checkedInMinute = int.Parse(checkedInTimeSplit[1]);
                    int checkedInTimeCalculated = checkedInHour * 60 + checkedInMinute;

                    var checkedOutTimeSplit = checkedOutTime.Split(":");
                    int checkedOutHour = int.Parse(checkedOutTimeSplit[0]);
                    int checkedOutMinute = int.Parse(checkedOutTimeSplit[1]);
                    int checkedOutTimeCalculated = checkedOutHour * 60 + checkedOutMinute;

                    if (fromTimeCalculated < checkedInTimeCalculated && toTimeCalculated <= checkedOutTimeCalculated)
                    {
                        booking.BookingStatus = BookingStatus.BookedEarlyCheckIn;
                    }
                    else if (fromTimeCalculated < checkedInTimeCalculated && toTimeCalculated > checkedOutTimeCalculated)
                    {
                        booking.BookingStatus = BookingStatus.BookedEarlyCheckInAndLateCheckOut;
                    }
                    else if (fromTimeCalculated >= checkedInTimeCalculated && toTimeCalculated <= checkedOutTimeCalculated)
                    {
                        booking.BookingStatus = BookingStatus.Booked;
                    }
                    else if (fromTimeCalculated >= checkedInTimeCalculated && toTimeCalculated > checkedOutTimeCalculated)
                    {
                        booking.BookingStatus = BookingStatus.BookedLateCheckOut;
                    }

                    _context.Add(booking);
                    await _context.SaveChangesAsync();
                }

                selectedRoomId = booking.RoomId;
            }

            return RedirectToAction("CheckIn", "Rooms", new { id = selectedRoomId.ToString() }); 
        }
        public async Task<IActionResult> CreateAsync()
        {
            ViewData["AgentId"] = new SelectList(_context.Agent, "Id", "Name");
            ViewData["GuestId"] = new SelectList(_context.Guest, "Id", "Name");
            var rooms = await _context.Room.Where(s => s.RoomStatus == RoomStatus.Available).ToListAsync();
            ViewData["RoomId"] = new SelectList(rooms, "Id", "RoomNumber");
            return View();
        }

        public static int CompareTime(string x, string y)
        {
            int xHour = int.Parse(x.Split(":")[0]);
            int xMinute = int.Parse(x.Split(":")[1]);
            int yHour = int.Parse(y.Split(":")[0]);
            int yMinute = int.Parse(y.Split(":")[1]);
            if (xHour > yHour)
            {
                return 1;
            }
            else if (xHour < yHour)
            {
                return -1;
            }
            else
            {
                if (xMinute > yMinute)
                {
                    return 1;
                }
                else if (xMinute < yMinute)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
        }

        // POST: Bookings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,RoomId,GuestId,From,To,BookingStatus,AgentId")] Booking booking)
        {
            if (ModelState.IsValid)
            {
                if (booking.From.Date == booking.To.Date)
                {
                    booking.BookingStatus = BookingStatus.BookedHourly;
                }
                booking.CreatedDate = DateTime.Now;
                _context.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AgentId"] = new SelectList(_context.Agent, "Id", "Address", booking.AgentId);
            ViewData["GuestId"] = new SelectList(_context.Guest, "Id", "Address", booking.GuestId);
            ViewData["RoomId"] = new SelectList(_context.Room, "Id", "Id", booking.RoomId);
            return View(booking);
        }

        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Booking == null)
            {
                RedirectToAction("Error", "Home");
            }

            var booking = await _context.Booking.FindAsync(id);
            if (booking == null)
            {
                RedirectToAction("Error", "Home");
            }

            ViewData["Agents"] = new SelectList(
                                                _context.Agent.Select(
                                                    a => new
                                                    {
                                                        Id = a.Id,
                                                        Name = a.Name + " - " + a.NRC
                                                    }).OrderBy(aa => aa.Name),
                                                    "Id", "Name", booking.AgentId);
            ViewData["Guests"] = new SelectList(_context.Guest.Select(
                                                    g => new
                                                    {
                                                        Id = g.Id,
                                                        Name = g.Name + " - " + g.NRC
                                                    }).OrderBy(gg => gg.Name),
                                                    "Id", "Name", booking.GuestId);
            ViewData["Rooms"] = new SelectList(_context.Room, "Id", "RoomNumber", booking.RoomId);
            return View(booking);
        }

        // POST: Bookings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RoomId,GuestId,From,To,BookingStatus,AgentId")] Booking booking)
        {
            if (id != booking.Id)
            {
                RedirectToAction("Error", "Home");
            }

            //get check in out time
            var roomTypeId = _context.Room.FirstOrDefault(r => r.Id == booking.RoomId)!.RoomTypeId;
            var checkedInOutTime = await _context.CheckedInOutTime.FirstOrDefaultAsync(t => t.RoomTypeId == roomTypeId);

            // if checkedInOutTime is null, redirect to CreatePeriod page
            if (checkedInOutTime == null)
            {
                ModelState.AddModelError("Id", "Selected Room Type does not have check in/out time setup yet!");
                ViewData["RoomTypeId"] = new SelectList(_context.RoomType, "Id", "Name");
                return View("CreatePeriod", new Room());
            }

            var checkedInTime = checkedInOutTime!.CheckedInTime;
            var checkedOutTime = checkedInOutTime!.CheckedOutTime;

            booking!.From = booking!.From!.Date + TimeSpan.Parse(checkedInTime);
            booking!.To = booking!.To!.Date + TimeSpan.Parse(checkedOutTime);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.Id))
                    {
                        RedirectToAction("Error", "Home");
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AgentId"] = new SelectList(_context.Agent, "Id", "Address", booking.AgentId);
            ViewData["GuestId"] = new SelectList(_context.Guest, "Id", "Address", booking.GuestId);
            ViewData["RoomId"] = new SelectList(_context.Room, "Id", "Id", booking.RoomId);
            return View(booking);
        }

        // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Booking == null)
            {
                RedirectToAction("Error", "Home");
            }

            var booking = await _context.Booking
                .Include(b => b.Agent)
                .Include(b => b.Guest)
                .Include(b => b.Room)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (booking == null)
            {
                RedirectToAction("Error", "Home");
            }

            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Booking == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Booking'  is null.");
            }
            var booking = await _context.Booking.FindAsync(id);
            var room = await _context.Room.Where(r => r.Id == booking!.RoomId).FirstOrDefaultAsync();
            if (booking != null)
            {
                if (room != null)
                    room.RoomStatus = RoomStatus.Available;

                _context.Booking.Remove(booking);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Booking.Any(e => e.Id == id);
        }
    }
}
