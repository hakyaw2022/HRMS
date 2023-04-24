using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HRMS.Data;
using HRMS.Models;
using HRMS.ViewModels;
using System.Dynamic;
using HRMS.Helpers;

namespace HRMS.Controllers
{
    public class TransController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Trans
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Transaction.Include(t => t.Room);
            return View(await applicationDbContext.ToListAsync());

        }

        public async Task<IActionResult> AllServices()
        {
            var allServices = new AllServicesViewModel();

            allServices.Services = _context.Service
                                    .Select(s => new ServiceViewModel
                                    {
                                        Id = s.Id,
                                        Name = s.Name,
                                        Price = s.Price,
                                        PricingType = s.PricingType,
                                        Status = s.Status,
                                        Quantity = 0
                                    })
                                    .OrderBy(s => s.Name)
                                    .ToList();

            allServices.RestaurantItems = _context.Restaurant
                                            .Select(r => new RestaurantItemViewModel
                                            {
                                                Id = r.Id,
                                                Name = r.Name,
                                                ItemCode = r.ItemCode,
                                                UnitPrice = r.UnitPrice,
                                                Category = r.Category,
                                                Quantity = 0
                                            })
                                            .OrderBy(r => r.Category).ThenBy(r => r.Name)
                                            .ToList();

            allServices.Rooms = _context.Room
                            .Where(r => r.RoomStatus == RoomStatus.Occupied)
                            .OrderBy(r => r.RoomNumber)
                            .Include("Guest")
                            .Include("RoomType")
                            .ToList();

            // add message if there is any
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"]!.ToString();
                TempData["message"] = "";

            }
            else
                ViewBag.Message = "";

            return View(allServices);

        }

        [HttpPost]
        public async Task<IActionResult> AllServicesCharge(AllServicesViewModel vm, IFormCollection form)
        {
            try
            {
                // gather required informations
                int roomId = -1;
                roomId = int.TryParse(form["ChargeToRoom"].FirstOrDefault(), out roomId) ? roomId : -1;

                var room = await _context.Room.FindAsync(roomId);
                var guest = await _context.Guest.FindAsync(room!.GuestId);
                var roomType = await _context.RoomType.FindAsync(room!.RoomTypeId);
                Agent? agent =  _context.Booking.Where(b =>
                                        b.Room == room && b.Guest == guest)
                                        !.OrderBy(b => b.Id)
                                        !.LastOrDefault()
                                        !.Agent;
                var curDateTime = Utility.GetMMTimeNow(); // DateTime.Now;

                // prepare transactions object to be saved
                
                // restaurant items
                foreach (var item in vm!.RestaurantItems!.Where(r => r.Quantity > 0).ToList())
                {
                    int qty = (int)item.Quantity;
                    var itemId = item.Id;
                    var restaurantItem = await _context.Restaurant.FindAsync(itemId);
                    var t = new Transaction
                    {
                        TransactionType = TransactionType.Restaurant,
                        TransactionStatus = TransactionStatus.Active,
                        Room = room,
                        Guest = guest,
                        Agent = agent,
                        Restaurant = restaurantItem,
                        CreatedDate = curDateTime,
                        Quantity = qty,
                        Amount = restaurantItem!.UnitPrice,
                        SubTotal = restaurantItem!.UnitPrice * qty
                    };

                    await _context.AddAsync(t);
                }


                // service items
                foreach (var item in vm!.Services!.Where(s => s.Quantity > 0).ToList())
                {
                    int qty = (int)item.Quantity;
                    var itemId = item.Id;
                    var service = await _context.Service.FindAsync(itemId);
                    var t = new Transaction();
                    t.TransactionType = TransactionType.Service;
                    t.TransactionStatus = TransactionStatus.Active;
                    t.Room = room;
                    t.Guest = guest;
                    t.Agent = agent;
                    t.Service = service;
                    t.CreatedDate = curDateTime;
                    t.Quantity = qty;
                    t.Amount = service!.Price;
                    t.SubTotal = service!.Price * qty;

                    await _context.AddAsync(t);
                }

                await _context.SaveChangesAsync();


                TempData["message"] = $"Successfully saved charges for " +
                                      $"{roomType!.Name}-{room.RoomNumber} - {guest!.Name}";
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving charges to a room / customer", ex);
            }

            return RedirectToAction(nameof(AllServices));
        }

        // GET: Trans/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Transaction == null)
            {
                RedirectToAction("Error", "Home");
            }

            var transaction = await _context.Transaction
                .Include(t => t.Room)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transaction == null)
            {
                RedirectToAction("Error", "Home");
            }

            return View(transaction);
        }

        // GET: Trans/Create
        public IActionResult Create()
        {
            ViewData["RoomId"] = new SelectList(_context.Room, "Id", "Id");
            return View();
        }

        // POST: Trans/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TransactionType,TransactionStatus,RoomId,Amount,Comment,CreatedDate,Description")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                _context.Add(transaction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RoomId"] = new SelectList(_context.Room, "Id", "Id", transaction.RoomId);
            return View(transaction);
        }

        // GET: Trans/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Transaction == null)
            {
                RedirectToAction("Error", "Home");
            }

            var transaction = await _context.Transaction.FindAsync(id);
            if (transaction == null)
            {
                RedirectToAction("Error", "Home");
            }
            ViewData["RoomId"] = new SelectList(_context.Room, "Id", "Id", transaction.RoomId);
            return View(transaction);
        }

        // POST: Trans/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TransactionType,TransactionStatus,RoomId,Amount,Comment,CreatedDate,Description")] Transaction transaction)
        {
            if (id != transaction.Id)
            {
                RedirectToAction("Error", "Home");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionExists(transaction.Id))
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
            ViewData["RoomId"] = new SelectList(_context.Room, "Id", "Id", transaction.RoomId);
            return View(transaction);
        }

        // GET: Trans/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Transaction == null)
            {
                RedirectToAction("Error", "Home");
            }

            var transaction = await _context.Transaction
                .Include(t => t.Room)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transaction == null)
            {
                RedirectToAction("Error", "Home");
            }

            return View(transaction);
        }

        // POST: Trans/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Transaction == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Transaction'  is null.");
            }
            var transaction = await _context.Transaction.FindAsync(id);
            if (transaction != null)
            {
                _context.Transaction.Remove(transaction);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TransactionExists(int id)
        {
            return (_context.Transaction?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
