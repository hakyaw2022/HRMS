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
using Microsoft.Extensions.Hosting;

namespace HRMS.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class AgentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public AgentsController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment; 
        }

        // GET: Agents
        public async Task<IActionResult> Index()
        {
              return View(await _context.Agent.ToListAsync());
        }

        // GET: Agents/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Agent == null)
            {
                RedirectToAction("Error", "Home");
            }

            var agent = await _context.Agent
                .FirstOrDefaultAsync(m => m.Id == id);
            if (agent == null)
            {
                RedirectToAction("Error", "Home");
            }

            return View(agent);
        }

        // GET: Agents/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Agents/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Phone,Address,NRC,Photo1,Photo2,Photo3")] Agent agent)
        {

            if (ModelState.IsValid)
            {
                var existingAgent = _context.Agent.Any(a => a.Name!.ToLower() == agent.Name!.ToLower());

                if(existingAgent)
                {
					ModelState.AddModelError(string.Empty, "Existing Agent!");
                    return View(agent);
				}
                else
                {
					agent.CreatedDate = DateTime.Now;
					string wwwRootPath = _hostEnvironment.WebRootPath;
                    Directory.CreateDirectory(Path.Combine(wwwRootPath, "Image", "Agent", "Photo1"));
                    Directory.CreateDirectory(Path.Combine(wwwRootPath, "Image", "Agent", "Photo2"));
                    Directory.CreateDirectory(Path.Combine(wwwRootPath, "Image", "Agent", "Photo3"));
					if (agent.Photo1 is not null)
					{
						string fileName1 = Path.GetFileNameWithoutExtension(agent.Photo1.FileName);
						string extension1 = Path.GetExtension(agent.Photo1.FileName);
						agent.Photo1Name = fileName1 = fileName1 + DateTime.Now.ToString("yymmssfff") + extension1;
						string path1 = Path.Combine(wwwRootPath + "/Image/Agent/Photo1", fileName1);
						using (var fileStream1 = new FileStream(path1, FileMode.Create))
						{
							await agent.Photo1.CopyToAsync(fileStream1);
						}
					}

					if (agent.Photo2 is not null)
					{
						string fileName2 = Path.GetFileNameWithoutExtension(agent.Photo2.FileName);
						string extension2 = Path.GetExtension(agent.Photo2.FileName);
						agent.Photo2Name = fileName2 = fileName2 + DateTime.Now.ToString("yymmssfff") + extension2;

						string path2 = Path.Combine(wwwRootPath + "/Image/Agent/Photo2", fileName2);
						using (var fileStream2 = new FileStream(path2, FileMode.Create))
						{
							await agent.Photo2.CopyToAsync(fileStream2);
						}
					}

					if (agent.Photo3 is not null)
					{
						string fileName3 = Path.GetFileNameWithoutExtension(agent.Photo3.FileName);
						string extension3 = Path.GetExtension(agent.Photo3.FileName);
						agent.Photo3Name = fileName3 = fileName3 + DateTime.Now.ToString("yymmssfff") + extension3;

						string path3 = Path.Combine(wwwRootPath + "/Image/Agent/Photo3", fileName3);
						using (var fileStream3 = new FileStream(path3, FileMode.Create))
						{
							await agent.Photo3.CopyToAsync(fileStream3);
						}
					}
					_context.Add(agent);
					await _context.SaveChangesAsync();
					return RedirectToAction(nameof(Index));
				}
            }
            return View(agent);
        }

        // GET: Agents/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Agent == null)
            {
                RedirectToAction("Error", "Home");
            }

            var agent = await _context.Agent.FindAsync(id);
            if (agent == null)
            {
                RedirectToAction("Error", "Home");
            }
            return View(agent);
        }

        // POST: Agents/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Phone,Address,NRC,Photo1, Photo1Name, Photo2, Photo2Name, Photo3, Photo3Name")] Agent agent)
        {
            if (id != agent.Id)
            {
                RedirectToAction("Error", "Home");
            }


            if (ModelState.IsValid)
            {
                string wwwRootPath = _hostEnvironment.WebRootPath;
                Directory.CreateDirectory(Path.Combine(wwwRootPath, "Image", "Agent", "Photo1"));
                Directory.CreateDirectory(Path.Combine(wwwRootPath, "Image", "Agent", "Photo2"));
                Directory.CreateDirectory(Path.Combine(wwwRootPath, "Image", "Agent", "Photo3"));
                if (agent.Photo1 is not null)
                {
                    string fileName1 = Path.GetFileNameWithoutExtension(agent.Photo1.FileName);
                    string extension1 = Path.GetExtension(agent.Photo1.FileName);
                    agent.Photo1Name = fileName1 = fileName1 + DateTime.Now.ToString("yymmssfff") + extension1;
                    string path1 = Path.Combine(wwwRootPath + "/Image/Agent/Photo1", fileName1);
                    using (var fileStream1 = new FileStream(path1, FileMode.Create))
                    {
                        await agent.Photo1.CopyToAsync(fileStream1);
                    }
                }

                if (agent.Photo2 is not null)
                {
                    string fileName2 = Path.GetFileNameWithoutExtension(agent.Photo2.FileName);
                    string extension2 = Path.GetExtension(agent.Photo2.FileName);
                    agent.Photo2Name = fileName2 = fileName2 + DateTime.Now.ToString("yymmssfff") + extension2;

                    string path2 = Path.Combine(wwwRootPath + "/Image/Agent/Photo2", fileName2);
                    using (var fileStream2 = new FileStream(path2, FileMode.Create))
                    {
                        await agent.Photo2.CopyToAsync(fileStream2);
                    }
                }

                if (agent.Photo3 is not null)
                {
                    string fileName3 = Path.GetFileNameWithoutExtension(agent.Photo3.FileName);
                    string extension3 = Path.GetExtension(agent.Photo3.FileName);
                    agent.Photo3Name = fileName3 = fileName3 + DateTime.Now.ToString("yymmssfff") + extension3;

                    string path3 = Path.Combine(wwwRootPath + "/Image/Agent/Photo3", fileName3);
                    using (var fileStream3 = new FileStream(path3, FileMode.Create))
                    {
                        await agent.Photo3.CopyToAsync(fileStream3);
                    }
                }

                try
                {
                    _context.Update(agent);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AgentExists(agent.Id))
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
            return View(agent);
        }

        // GET: Agents/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Agent == null)
            {
                RedirectToAction("Error", "Home");
            }

            var agent = await _context.Agent
                .FirstOrDefaultAsync(m => m.Id == id);

            if (agent == null)
            {
                RedirectToAction("Error", "Home");
            }

            return View(agent);
        }

        // POST: Agents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Agent == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Agent'  is null.");
            }
            var agent = await _context.Agent.FindAsync(id);

            if (agent != null)
            {
                // check if there is any booked/checked in

                var booked = _context.Booking.Any(b => b.AgentId == agent.Id
                                                       &&
                                                    (b.BookingStatus == BookingStatus.Booked
                                                    || b.BookingStatus == BookingStatus.CheckedIn));
                if(booked)
                {
                    ModelState.AddModelError("", "Agent can't be deleted! There are existing booked/checked-in rooms with this agent.");
                    return View(agent);
                }

                if (agent.Photo1Name is not null)
                {
                    var imagePath1 = Path.Combine(_hostEnvironment.WebRootPath, "Image/Agent/Photo1", agent.Photo1Name);
                    if (System.IO.File.Exists(imagePath1))
                        System.IO.File.Delete(imagePath1);
                }

                if (agent.Photo2Name is not null)
                {
                    var imagePath2 = Path.Combine(_hostEnvironment.WebRootPath, "Image/Agent/Photo2", agent.Photo2Name);
                    if (System.IO.File.Exists(imagePath2))
                        System.IO.File.Delete(imagePath2);
                }

                if (agent.Photo3Name is not null)
                {
                    var imagePath3 = Path.Combine(_hostEnvironment.WebRootPath, "Image/Agent/Photo3", agent.Photo3Name);
                    if (System.IO.File.Exists(imagePath3))
                        System.IO.File.Delete(imagePath3);
                }
                _context.Agent.Remove(agent);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AgentExists(int id)
        {
          return _context.Agent.Any(e => e.Id == id);
        }
    }
}
