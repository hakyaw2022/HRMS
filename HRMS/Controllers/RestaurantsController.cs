using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRMS.Data;
using HRMS.Models;
using HRMS.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace HRMS.Controllers
{
    [Authorize(Roles ="Admin,Manager,Receptionist")]
    public class RestaurantsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RestaurantsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Restaurants
        public async Task<IActionResult> Index()
        {
              return View(await _context.Restaurant.ToListAsync());
        }

        public async Task<IActionResult> Order()
        {
            var cart = SessionHelper.GetObjectFromJson<List<Item>>(HttpContext.Session, "cart");
            ViewBag.cart = cart;
            return View(await _context.Restaurant.ToListAsync());
        }

        public async Task<ActionResult> Category(string? id)
        {
            var cart = SessionHelper.GetObjectFromJson<List<Item>>(HttpContext.Session, "cart");
            ViewBag.cart = cart;
            ViewData["Category"] = id;
            return View(await _context.Restaurant.ToListAsync());
        }

        public async Task<ActionResult> TransactionCreate(int? id)
        {
            ViewData["Id"] = id;
            return View(await _context.Restaurant.ToListAsync());
        }

        // GET: Restaurants/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Restaurant == null)
            {
                RedirectToAction("Error", "Home");
            }

            var restaurant = await _context.Restaurant
                .FirstOrDefaultAsync(m => m.Id == id);
            if (restaurant == null)
            {
                RedirectToAction("Error", "Home");
            }

            return View(restaurant);
        }

        // GET: Restaurants/Create
        public IActionResult Create()
        {
            //category suggestions
            var categories = _context.Restaurant.Select(r => r.Category).Distinct().ToList();
            categories.Add("Breakfast");
            categories.Add("Lunch");
            categories.Add("Dinner");
            categories.Add("Beverage");
            categories = categories.Distinct().ToList();
            ViewBag.Categories = categories;

            //item code number
            var itemCount = _context.Restaurant.Count() + 1;
            var itemNumber = "000" + itemCount.ToString();
            itemNumber = itemNumber.Substring(itemNumber.Length - 3);

            ViewBag.ItemNumber = itemNumber;


            return View();
        }

        // POST: Restaurants/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,ItemCode,Quantity,UnitPrice,Category")] Restaurant restaurant)
        {
            if (ModelState.IsValid)
            {
                _context.Add(restaurant);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(restaurant);
        }

        // GET: Restaurants/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Restaurant == null)
            {
                RedirectToAction("Error", "Home");
            }

            var restaurant = await _context.Restaurant.FindAsync(id);
            if (restaurant == null)
            {
                RedirectToAction("Error", "Home");
            }
            
            //category suggestions
            var categories = _context.Restaurant.Select(r => r.Category).Distinct().ToList();
            categories.Add("Breakfast");
            categories.Add("Lunch");
            categories.Add("Dinner");
            categories.Add("Beverage");
            categories = categories.Distinct().ToList();
            ViewBag.Categories = categories;

            return View(restaurant);
        }

        // POST: Restaurants/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ItemCode,Quantity,UnitPrice,Category")] Restaurant restaurant)
        {
            if (id != restaurant.Id)
            {
                RedirectToAction("Error", "Home");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(restaurant);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RestaurantExists(restaurant.Id))
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
            return View(restaurant);
        }

        // GET: Restaurants/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Restaurant == null)
            {
                RedirectToAction("Error", "Home");
            }

            var restaurant = await _context.Restaurant
                .FirstOrDefaultAsync(m => m.Id == id);
            if (restaurant == null)
            {
                RedirectToAction("Error", "Home");
            }

            return View(restaurant);
        }

        // POST: Restaurants/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Restaurant == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Restaurant'  is null.");
            }
            var restaurant = await _context.Restaurant.FindAsync(id);
            if (restaurant != null)
            {
                _context.Restaurant.Remove(restaurant);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RestaurantExists(int id)
        {
          return _context.Restaurant.Any(e => e.Id == id);
        }
    }
}
