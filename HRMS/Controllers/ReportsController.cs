using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HRMS.Data;
using HRMS.Models;
using Microsoft.AspNetCore.Authorization;

namespace HRMS.Controllers
{
    [Authorize(Roles = "Admin, Manager")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }
        // GET: ReportsController
        public async Task<IActionResult> Index()
        {
            DateTime date = DateTime.Now.Date;
            var bookings = await _context.Booking.Where(s=> s.CreatedDate.Date.Equals(date)).Include(s=>s.Room.RoomType).ToListAsync();
            ViewBag.TotalBookings = bookings.Count;
            var roomTypes = await _context.RoomType.ToListAsync();
            Dictionary<string, int> roomTypesBookedCount = new();
            foreach(var roomType in roomTypes)
            {
                var bookingsEach = bookings.Where(s => s.Room.RoomType.Equals(roomType));
                roomTypesBookedCount.Add(roomType.Name , bookingsEach.Count());                
            }
            ViewBag.BookedRoomTypes = roomTypesBookedCount;                       

            var receipts = await _context.Receipt.Where(s => s.CreatedDate.Date.Equals(date)).ToListAsync();
            ViewBag.TotalIncome = receipts.Sum(s => s.Amount);
            ViewBag.IncomeFromRoom = receipts.Where(s => s.TransactionType == TransactionType.Room).Sum(s => s.Amount);
            ViewBag.IncomeFromRestaurant = receipts.Where(s => s.TransactionType == TransactionType.Restaurant).Sum(s => s.Amount);
            ViewBag.IncomeFromService = receipts.Where(s => s.TransactionType == TransactionType.Service).Sum(s => s.Amount);

            var rooms = bookings.Where(s => s.From.Date.Equals(date)).Where(s=>s.BookingStatus == BookingStatus.CheckedIn).Select(s=>s.Room);
            ViewBag.CheckedInRooms = rooms.Count();
            ViewBag.CheckedInCustomers = _context.CheckedInCustomer.Include(s => s.Room).Where(s => rooms.Contains(s.Room)).Count();
            ViewBag.CheckedOutRooms = bookings.Where(s => s.From.Date.Equals(date)).Where(s=>s.BookingStatus == BookingStatus.Finished).Count();
            return View();
        }

        // GET: ReportsController/Details/5
        public ActionResult DetailsView()
        {           
            return View();
        }

        // GET: ReportsController/Details/5
        public async Task<IActionResult> Details(IFormCollection form)
        {
            DateTime from = DateTime.Parse(form["from_date"]);
            DateTime to = DateTime.Parse(form["to_date"]);           
            if(to < from)
            {
                ViewData["Message"] = "From Date should be later than To Date";
                return View(nameof(DetailsView));
            }
            if((to-from).Days > 31)
            {
                ViewData["Message"] = "The difference between From Date and To Date should not be more than 31 Days";
                return View(nameof(DetailsView));
            }

            string report_name = form["report_type"];
            if(report_name == "room_income")
            {
                var income = await _context.Receipt.Where(s=>s.CreatedDate >= from).
                    Where(s=>s.CreatedDate <= to).
                    Where(s=>s.TransactionType == TransactionType.Room).
                    ToListAsync();
                ViewBag.Total = income.Sum(s => s.Amount);                
                ViewBag.From = from.Date; ViewBag.To = to.Date;
                ViewBag.Type = "Room";
                return View("Details", income);
            }
            else if(report_name == "service_income")
            {
                var income = await _context.Receipt.Where(s => s.CreatedDate >= from).
                    Where(s => s.CreatedDate <= to).
                    Where(s => s.TransactionType == TransactionType.Service).
                    ToListAsync();
                ViewBag.Total = income.Sum(s => s.Amount);
                ViewBag.From = from.Date; ViewBag.To = to.Date;
                ViewBag.Type = "Service";
                return View("Details", income);
            }
            else if(report_name == "retaurant_income")
            {
                var income = await _context.Receipt.Where(s => s.CreatedDate >= from).
                    Where(s => s.CreatedDate <= to).
                    Where(s => s.TransactionType == TransactionType.Restaurant).
                    ToListAsync();
                ViewBag.Total = income.Sum(s => s.Amount);
                ViewBag.From = from.Date; ViewBag.To = to.Date;
                ViewBag.Type = "Restaurant";
                return View("Details", income);
            }
            

            if (report_name == "total_bookings")
            {
                
                var bookings = await _context.Booking.Where(s => s.CreatedDate >= from).
                    Where(s => s.CreatedDate <= to).Include(s => s.Room).Include(s => s.Guest).Include(s => s.Agent).ToListAsync();
                ViewBag.Total = bookings.Count();

                ViewBag.From = from.Date; ViewBag.To = to.Date;

                int total_days = 0;
               
                foreach(var booking in bookings)
                {
                    var new_days = booking.To - booking.From;
                    if(new_days.TotalHours <=24 || new_days.TotalHours >= 24)
                    {
                        total_days = total_days + 1;
                    }
                    else
                    {
                        total_days = total_days + new_days.Days;
                    }
                   
                    
                }
                ViewBag.DaysBooked = total_days;
                return View("DetailsTotalBookings", bookings);

            }

            return View();
        }

        // GET: ReportsController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ReportsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ReportsController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ReportsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ReportsController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ReportsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
