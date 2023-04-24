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
    [Authorize(Roles = "Admin,Manager")]
    public class RoomTypesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RoomTypesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: RoomTypes
        public async Task<IActionResult> Index()
        {
              return View(await _context.RoomType.ToListAsync());
        }

        // GET: RoomTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.RoomType == null)
            {
                RedirectToAction("Error", "Home");
            }

            var roomType = await _context.RoomType
                .FirstOrDefaultAsync(m => m.Id == id);
            if (roomType == null)
            {
                RedirectToAction("Error", "Home");
            }

            return View(roomType);
        }

        // GET: RoomTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: RoomTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Price")] RoomType roomType)
        {            
            if (ModelState.IsValid)
            {
                var existing = _context.RoomType.Any(r => r.Name!.ToLower() == roomType.Name!.ToLower());
                if(existing)
                {
                    ModelState.AddModelError("", "Existing Room Type!");
                    return View(roomType);
                }

                _context.Add(roomType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(roomType);
        }

        // GET: RoomTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.RoomType == null)
            {
                RedirectToAction("Error", "Home");
            }

            var roomType = await _context.RoomType.FindAsync(id);
            if (roomType == null)
            {
                RedirectToAction("Error", "Home");
            }
            return View(roomType);
        }

        // POST: RoomTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Price, HourlyPrice")] RoomType roomType)
        {
            if (id != roomType.Id)
            {
                RedirectToAction("Error", "Home");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(roomType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomTypeExists(roomType.Id))
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
            return View(roomType);
        }

        // GET: RoomTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.RoomType == null)
            {
                RedirectToAction("Error", "Home");
            }

            var roomType = await _context.RoomType
                .FirstOrDefaultAsync(m => m.Id == id);
            if (roomType == null)
            {
                RedirectToAction("Error", "Home");
            }

            return View(roomType);
        }

        // POST: RoomTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.RoomType == null)
            {
                return Problem("Entity set 'ApplicationDbContext.RoomType'  is null.");
            }
            var roomType = await _context.RoomType.FindAsync(id);
            if (roomType != null)
            {
                var room = _context.Room.Any(r => r.RoomType == roomType);
                if(room)
                {
                    ModelState.AddModelError("Name", "Rooms exist with this room type and not allowed to delete it");
                    return View(roomType);
                }

                    _context.RoomType.Remove(roomType);
  
                
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RoomTypeExists(int id)
        {
          return _context.RoomType.Any(e => e.Id == id);
        }
    }
}
