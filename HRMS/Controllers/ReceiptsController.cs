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
    [Authorize(Roles = "Admin,Manager,Receptionist")]
    public class ReceiptsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReceiptsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Receipts
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Receipt.Include(r => r.Agent).Include(r => r.Guest).Include(r => r.Room);
            return View(await applicationDbContext.OrderByDescending(s=>s.CreatedDate).ToListAsync());
        }

        // GET: Receipts/Details/5
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null || _context.Receipt == null)
            {
                RedirectToAction("Error", "Home");
            }

            var receipt = _context.Receipt.Where(s=>s.ReceiptNumber == id).Include(s=>s.Guest).Include(s=>s.Room).Include(s=>s.Agent).First();
            if (receipt.GuestId is not null)
            {
                ViewData["Guest"] = receipt.Guest.Name;
            }
            else
            {
                ViewData["Guest"] = "NoGuest";
            }

            if (receipt.RoomId is not null)
            {
                ViewData["Room"] = receipt.Room.RoomNumber;
            }
            else
            {
                ViewData["Room"] = "NoRoomNumber";
            }
            if (receipt.AgentId is not null)
            {
                ViewData["Agent"] = receipt.Agent.Name;
            }
            else
            {
                ViewData["Agent"] = "NoAgent";
            } 
            var receipts = await _context.Receipt.Where(s => s.ReceiptNumber == id).ToListAsync();
            ViewData["Total"] = receipts.Sum(s => s.Amount);
            return View(receipts);
        }

        // GET: Receipts/Create
        public IActionResult Create()
        {
            ViewData["AgentId"] = new SelectList(_context.Agent, "Id", "Address");
            ViewData["GuestId"] = new SelectList(_context.Guest, "Id", "Address");
            ViewData["RoomId"] = new SelectList(_context.Room, "Id", "Id");
            return View();
        }

        // POST: Receipts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,GuestId,RoomId,AgentId,Name,Amount,CreatedDate,ReceiptNumber")] Receipt receipt)
        {
            if (ModelState.IsValid)
            {
                _context.Add(receipt);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AgentId"] = new SelectList(_context.Agent, "Id", "Address", receipt.AgentId);
            ViewData["GuestId"] = new SelectList(_context.Guest, "Id", "Address", receipt.GuestId);
            ViewData["RoomId"] = new SelectList(_context.Room, "Id", "Id", receipt.RoomId);
            return View(receipt);
        }

        // GET: Receipts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Receipt == null)
            {
                RedirectToAction("Error", "Home");
            }

            var receipt = await _context.Receipt.FindAsync(id);
            if (receipt == null)
            {
                RedirectToAction("Error", "Home");
            }
            ViewData["AgentId"] = new SelectList(_context.Agent, "Id", "Address", receipt.AgentId);
            ViewData["GuestId"] = new SelectList(_context.Guest, "Id", "Address", receipt.GuestId);
            ViewData["RoomId"] = new SelectList(_context.Room, "Id", "Id", receipt.RoomId);
            return View(receipt);
        }

        // POST: Receipts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,GuestId,RoomId,AgentId,Name,Amount,CreatedDate,ReceiptNumber")] Receipt receipt)
        {
            if (id != receipt.Id)
            {
                RedirectToAction("Error", "Home");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(receipt);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReceiptExists(receipt.Id))
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
            ViewData["AgentId"] = new SelectList(_context.Agent, "Id", "Address", receipt.AgentId);
            ViewData["GuestId"] = new SelectList(_context.Guest, "Id", "Address", receipt.GuestId);
            ViewData["RoomId"] = new SelectList(_context.Room, "Id", "Id", receipt.RoomId);
            return View(receipt);
        }

        // GET: Receipts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Receipt == null)
            {
                RedirectToAction("Error", "Home");
            }

            var receipt = await _context.Receipt
                .Include(r => r.Agent)
                .Include(r => r.Guest)
                .Include(r => r.Room)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (receipt == null)
            {
                RedirectToAction("Error", "Home");
            }

            return View(receipt);
        }

        // POST: Receipts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Receipt == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Receipt'  is null.");
            }
            var receipt = await _context.Receipt.FindAsync(id);
            if (receipt != null)
            {
                _context.Receipt.Remove(receipt);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReceiptExists(int id)
        {
          return _context.Receipt.Any(e => e.Id == id);
        }
    }
}
