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

using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Hosting;

namespace HRMS.Controllers
{
    [Authorize(Roles ="Admin,Manager,Receptionist")]
    public class GuestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public GuestsController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment; 
        }

        // GET: Guests       
        public async Task<IActionResult> Index()
        {
              return View(await _context.Guest.ToListAsync());
        }

        public async Task<IActionResult> CheckedInCustomers()
        {
            return View(await _context.CheckedInCustomer.Include(s=>s.Guest).Include(s=>s.Room).ToListAsync());
        }

        // GET: Guests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Guest == null)
            {
                RedirectToAction("Error", "Home");
            }

            var guest = await _context.Guest
                .FirstOrDefaultAsync(m => m.Id == id);
            if (guest == null)
            {
                RedirectToAction("Error", "Home");
            }

            return View(guest);
        }

        // GET: Guests/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Guests/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Phone,Address,NRC,Photo1,Photo2,Photo3")] Guest guest)
        {
            if (ModelState.IsValid)
            {
                guest.CreatedDate = DateTime.Now;
                if(_context.Guest.Any(s=>s.NRC!.ToLower() == guest!.NRC!.ToLower())) 
                {
                    ModelState.AddModelError("NRC", "NRC or Passport already exists.");
                    return View(guest);
                }
                else
                {
                    if(_context.Guest.Where(s=>s.Name == guest.Name).Any())
                    {
                        guest.Name += "-" + guest.NRC;
                    }
                    string wwwRootPath = _hostEnvironment.WebRootPath;

                    Directory.CreateDirectory(Path.Combine(wwwRootPath, "Image", "Guest", "Photo1"));
                    Directory.CreateDirectory(Path.Combine(wwwRootPath, "Image", "Guest", "Photo2"));
                    Directory.CreateDirectory(Path.Combine(wwwRootPath, "Image", "Guest", "Photo3"));

                    if (guest.Photo1 is not null)
                    {                        
                        string fileName1 = Path.GetFileNameWithoutExtension(guest.Photo1.FileName);
                        string extension1 = Path.GetExtension(guest.Photo1.FileName);
                        guest.Photo1Name = fileName1 = fileName1 + DateTime.Now.ToString("yymmssfff") + extension1;
                        string path1 = Path.Combine(wwwRootPath + "/Image/Guest/Photo1", fileName1);
                        using (var fileStream1 = new FileStream(path1, FileMode.Create))
                        {
                            await guest.Photo1.CopyToAsync(fileStream1);
                        }
                    }

                    if(guest.Photo2 is not null)
                    {
                        string fileName2 = Path.GetFileNameWithoutExtension(guest.Photo2.FileName);
                        string extension2 = Path.GetExtension(guest.Photo2.FileName);
                        guest.Photo2Name = fileName2 = fileName2 + DateTime.Now.ToString("yymmssfff") + extension2;

                        string path2 = Path.Combine(wwwRootPath + "/Image/Guest/Photo2", fileName2);
                        using (var fileStream2 = new FileStream(path2, FileMode.Create))
                        {
                            await guest.Photo2.CopyToAsync(fileStream2);
                        }
                    }

                    if(guest.Photo3 is not null)
                    {
                        string fileName3 = Path.GetFileNameWithoutExtension(guest.Photo3.FileName);
                        string extension3 = Path.GetExtension(guest.Photo3.FileName);
                        guest.Photo3Name = fileName3 = fileName3 + DateTime.Now.ToString("yymmssfff") + extension3;

                        string path3 = Path.Combine(wwwRootPath + "/Image/Guest/Photo3", fileName3);
                        using (var fileStream3 = new FileStream(path3, FileMode.Create))
                        {
                            await guest.Photo3.CopyToAsync(fileStream3);
                        }
                    }  
                    _context.Add(guest);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                
            }
            return View(guest);
        }

        // GET: Guests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Guest == null)
            {
                RedirectToAction("Error", "Home");
            }

            var guest = await _context.Guest.FindAsync(id);
            if (guest == null)
            {
                RedirectToAction("Error", "Home");
            }
            return View(guest);
        }

        // POST: Guests/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Phone,Address,NRC,Photo1, Photo2, Photo3")] Guest guest)
        {
            if (id != guest.Id)
            {
                RedirectToAction("Error", "Home");
            }

            if (ModelState.IsValid)
            {
                string wwwRootPath = _hostEnvironment.WebRootPath;
                Directory.CreateDirectory(Path.Combine(wwwRootPath, "Image", "Guest", "Photo1"));
                Directory.CreateDirectory(Path.Combine(wwwRootPath, "Image", "Guest", "Photo2"));
                Directory.CreateDirectory(Path.Combine(wwwRootPath, "Image", "Guest", "Photo3"));
                if (guest.Photo1 is not null)
                {
                    string fileName1 = Path.GetFileNameWithoutExtension(guest.Photo1.FileName);
                    string extension1 = Path.GetExtension(guest.Photo1.FileName);
                    guest.Photo1Name = fileName1 = fileName1 + DateTime.Now.ToString("yymmssfff") + extension1;
                    string path1 = Path.Combine(wwwRootPath + "/Image/Guest/Photo1", fileName1);
                    using (var fileStream1 = new FileStream(path1, FileMode.Create))
                    {
                        await guest.Photo1.CopyToAsync(fileStream1);
                    }
                }                

                if (guest.Photo2 is not null)
                {
                    string fileName2 = Path.GetFileNameWithoutExtension(guest.Photo2.FileName);
                    string extension2 = Path.GetExtension(guest.Photo2.FileName);
                    guest.Photo2Name = fileName2 = fileName2 + DateTime.Now.ToString("yymmssfff") + extension2;

                    string path2 = Path.Combine(wwwRootPath + "/Image/Guest/Photo2", fileName2);
                    using (var fileStream2 = new FileStream(path2, FileMode.Create))
                    {
                        await guest.Photo2.CopyToAsync(fileStream2);
                    }
                }

                if (guest.Photo3 is not null)
                {
                    string fileName3 = Path.GetFileNameWithoutExtension(guest.Photo3.FileName);
                    string extension3 = Path.GetExtension(guest.Photo3.FileName);
                    guest.Photo3Name = fileName3 = fileName3 + DateTime.Now.ToString("yymmssfff") + extension3;

                    string path3 = Path.Combine(wwwRootPath + "/Image/Guest/Photo3", fileName3);
                    using (var fileStream3 = new FileStream(path3, FileMode.Create))
                    {
                        await guest.Photo3.CopyToAsync(fileStream3);
                    }
                }
                try
                {  
                    _context.Update(guest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GuestExists(guest.Id))
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
            return View(guest);
        }

        // GET: Guests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Guest == null)
            {
                RedirectToAction("Error", "Home");
            }

            var guest = await _context.Guest
                .FirstOrDefaultAsync(m => m.Id == id);
            if (guest == null)
            {
                RedirectToAction("Error", "Home");
            }

            return View(guest);
        }

        // POST: Guests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Guest == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Guest'  is null.");
            }
            var guest = await _context.Guest.FindAsync(id);
            if (guest != null)
            {

                var booked = _context.Room.Any(r => r.Guest == guest);
                if (booked)
                {
                    ModelState.AddModelError("Name", "This guest has booked or checked-in to a room. Delete not allowed!");
                    return View(guest);
                }

                var trans = _context.Transaction.Any(t => t.Guest == guest);
                if (trans)
                {
                    ModelState.AddModelError("Name", "There is a billing/transaction record with this guest. Delete not allowed!");
                    return View(guest);
                }

                if(guest.Photo1Name is not null)
                {
                    var imagePath1 = Path.Combine(_hostEnvironment.WebRootPath, "Image/Guest/Photo1", guest.Photo1Name);
                    if (System.IO.File.Exists(imagePath1))
                        System.IO.File.Delete(imagePath1);
                }

                if(guest.Photo2Name is not null)
                {
                    var imagePath2 = Path.Combine(_hostEnvironment.WebRootPath, "Image/Guest/Photo2", guest.Photo2Name);
                    if (System.IO.File.Exists(imagePath2))
                        System.IO.File.Delete(imagePath2);
                }

                if(guest.Photo3Name is not null)
                {
                    var imagePath3 = Path.Combine(_hostEnvironment.WebRootPath, "Image/Guest/Photo3", guest.Photo3Name);
                    if (System.IO.File.Exists(imagePath3))
                        System.IO.File.Delete(imagePath3);
                }
                _context.Guest.Remove(guest);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GuestExists(int id)
        {
          return _context.Guest.Any(e => e.Id == id);
        }
    }
}
