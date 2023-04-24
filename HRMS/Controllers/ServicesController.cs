using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HRMS.Data;
using HRMS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using Microsoft.AspNetCore.Authorization;

namespace HRMS.Controllers
{
    [Authorize(Roles = "Admin,Manager,Receptionist")]
    public class ServicesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Services
        public async Task<IActionResult> Index()
        {
              return View(await _context.Service.ToListAsync());
        }

        // GET: Services/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Service == null)
            {
                RedirectToAction("Error", "Home");
            }

            var service = await _context.Service
                .FirstOrDefaultAsync(m => m.Id == id);
            if (service == null)
            {
                RedirectToAction("Error", "Home");
            }

            return View(service);
        }

        // GET: Services/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Services/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Price,PricingType, Capacity")] Service service, IFormCollection form)
        {
            if (ModelState.IsValid)
            {
                string OpenedHour = form["OpenedHour"];
                string OpenedMinute = form["OpenedMinute"];
                string ClosedHour = form["ClosedHour"];
                string ClosedMinute = form["ClosedMinute"];
                service.OpenedTime = OpenedHour + ":" + OpenedMinute;
                service.ClosedTime = ClosedHour + ":" + ClosedMinute;
                service.Status = ServiceStatus.Available;
                _context.Add(service);
                await _context.SaveChangesAsync();               
                
                return RedirectToAction(nameof(Index));
            }

            
            return View(service);
        }

        // GET: Services/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Service == null)
            {
                RedirectToAction("Error", "Home");
            }

            var service = await _context.Service.FindAsync(id);
            if (service == null)
            {
                RedirectToAction("Error", "Home");
            }
            return View(service);
        }

        // POST: Services/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Price,PricingType, Capacity, Status")] Service service, IFormCollection form)
        {
            if (id != service.Id)
            {
                RedirectToAction("Error", "Home");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string OpenedHour = form["OpenedHour"];
                    string OpenedMinute = form["OpenedMinute"];
                    string ClosedHour = form["ClosedHour"];
                    string ClosedMinute = form["ClosedMinute"];
                    service.OpenedTime = OpenedHour + ":" + OpenedMinute;
                    service.ClosedTime = ClosedHour + ":" + ClosedMinute;
                    _context.Update(service);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceExists(service.Id))
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
            return View(service);
        }

        // GET: Services/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Service == null)
            {
                RedirectToAction("Error", "Home");
            }

            var service = await _context.Service
                .FirstOrDefaultAsync(m => m.Id == id);
            if (service == null)
            {
                RedirectToAction("Error", "Home");
            }

            return View(service);
        }

        // POST: Services/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Service == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Service'  is null.");
            }
            var service = await _context.Service.FindAsync(id);
            if (service != null)
            {
                _context.Service.Remove(service);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ServiceExists(int id)
        {
          return _context.Service.Any(e => e.Id == id);
        }
    }
}
