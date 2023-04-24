using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HRMS.Data;
using HRMS.Models;
using Microsoft.AspNetCore.Authorization;

namespace HRMS.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class CheckedInOutTimesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CheckedInOutTimesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: CheckedInOutTimes
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.CheckedInOutTime.Include(c => c.RoomType);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: CheckedInOutTimes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.CheckedInOutTime == null)
            {
                RedirectToAction("Error", "Home");
            }

            var checkedInOutTime = await _context.CheckedInOutTime
                .Include(c => c.RoomType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (checkedInOutTime == null)
            {
                RedirectToAction("Error", "Home");
            }

            return View(checkedInOutTime);
        }

        // GET: CheckedInOutTimes/Create
        public IActionResult Create()
        {
            ViewData["RoomTypeId"] = new SelectList(_context.RoomType, "Id", "Name");
            return View();
        }

        // POST: CheckedInOutTimes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,RoomTypeId, CheckedInTime, CheckedOutTime")] CheckedInOutTime checkedInOutTime, IFormCollection form)
        {
            checkedInOutTime.CheckedInTime = form["CheckedInHour"] + ":" + form["CheckedInMinute"];
            checkedInOutTime.CheckedOutTime = form["CheckedOutHour"] + ":" + form["CheckedOutMinute"];
            if (ModelState.IsValid)
            {

                var existing = _context.CheckedInOutTime.Any(c => c.RoomTypeId == checkedInOutTime.RoomTypeId);
                if (existing)
                {
                    ModelState.AddModelError("RoomTypeId", "This room type already has a check in/out time setup. Use edit function instead!");
                    ViewData["RoomTypeId"] = new SelectList(_context.RoomType, "Id", "Name", checkedInOutTime.RoomTypeId);
                    return View(checkedInOutTime);
                }

                _context.Add(checkedInOutTime);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["RoomTypeId"] = new SelectList(_context.RoomType, "Id", "Name", checkedInOutTime.RoomTypeId);
            return View(checkedInOutTime);
        }

        // GET: CheckedInOutTimes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.CheckedInOutTime == null)
            {
                RedirectToAction("Error", "Home");
            }

            var checkedInOutTime = await _context.CheckedInOutTime.FindAsync(id);
            if (checkedInOutTime == null)
            {
                RedirectToAction("Error", "Home");
            }
            ViewData["RoomTypeId"] = new SelectList(_context.RoomType, "Id", "Name", checkedInOutTime.RoomTypeId);
            return View(checkedInOutTime);
        }

        // POST: CheckedInOutTimes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RoomTypeId")] CheckedInOutTime checkedInOutTime, IFormCollection form)
        {
            if (id != checkedInOutTime.Id)
            {
                RedirectToAction("Error", "Home");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    checkedInOutTime.CheckedInTime = form["CheckedInHour"] + ":" + form["CheckedInMinute"];
                    checkedInOutTime.CheckedOutTime = form["CheckedOutHour"] + ":" + form["CheckedOutMinute"];
                    _context.Update(checkedInOutTime);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CheckedInOutTimeExists(checkedInOutTime.Id))
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
            ViewData["RoomTypeId"] = new SelectList(_context.RoomType, "Id", "Name", checkedInOutTime.RoomTypeId);
            return View(checkedInOutTime);
        }

        // GET: CheckedInOutTimes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.CheckedInOutTime == null)
            {
                RedirectToAction("Error", "Home");
            }

            var checkedInOutTime = await _context.CheckedInOutTime
                .Include(c => c.RoomType)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (checkedInOutTime == null)
            {
                RedirectToAction("Error", "Home");
            }



            return View(checkedInOutTime);
        }

        // POST: CheckedInOutTimes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.CheckedInOutTime == null)
            {
                return Problem("Entity set 'ApplicationDbContext.CheckedInOutTime'  is null.");
            }

            var checkedInOutTime = await _context.CheckedInOutTime
                                           .Include(c => c.RoomType)
                                           .FirstOrDefaultAsync(m => m.Id == id);

            if (checkedInOutTime != null)
            {
                var booked = _context.Booking.Any(b => b.Room!.RoomType == checkedInOutTime.RoomType
                                                        &&
                                                        (b.BookingStatus == BookingStatus.Booked
                                                        || b.BookingStatus == BookingStatus.CheckedIn));

                if (booked)
                {
                    ModelState.AddModelError("RoomType.Name", "Checked in/out time cannot be deleted! There are rooms booked or checked in using this setup times.");
                    return View(checkedInOutTime);
                }


                _context.CheckedInOutTime.Remove(checkedInOutTime);

            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));


        }

        private bool CheckedInOutTimeExists(int id)
        {
            return _context.CheckedInOutTime.Any(e => e.Id == id);
        }
    }
}
