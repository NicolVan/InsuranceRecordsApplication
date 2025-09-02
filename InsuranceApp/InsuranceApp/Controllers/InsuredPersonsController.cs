using InsuranceApp.Data;
using InsuranceApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace InsuranceApp.Controllers
{
    [Authorize]
    public class InsuredPersonsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public InsuredPersonsController(
            ApplicationDbContext context,
           UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: InsuredPersons
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);
            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("Admin") || roles.Contains("Agent"))
            {
                var allInsuredPersons = await _context.InsuredPersons
                    .Include(i => i.ApplicationUser)
                    .ToListAsync();

                return View(allInsuredPersons);
            }
            else
            {
                var insuredPerson = await _context.InsuredPersons
                    .Include(i => i.ApplicationUser)
                    .Include(i => i.Insurances)
                    .FirstOrDefaultAsync(i => i.ApplicationUserId == userId);

                if (insuredPerson == null)
                    return NotFound("Profile not found.");

                return View("Details", insuredPerson);
            }
        }

        // GET: InsuredPersons/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);
            var roles = await _userManager.GetRolesAsync(user);

            if (id == null)
                return NotFound();

            IQueryable<InsuredPerson> query = _context.InsuredPersons
                .Include(ip => ip.ApplicationUser)
                .Include(ip => ip.Insurances);

            if (roles.Contains("Client"))
            {
                query = query.Where(ip => ip.ApplicationUserId == userId && ip.Id == id);
            }
            else
            {
                query = query.Where(ip => ip.Id == id);
            }

            var insuredPerson = await query.FirstOrDefaultAsync();

            if (insuredPerson == null)
                return NotFound();

            return View(insuredPerson);
        }
        public IActionResult Create()
        {
            return View();
        }

        // POST: InsuredPersons/Create
        [Authorize(Roles = "Admin,Agent")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FullName,DateOfBirth,PhoneNumber,Street,City,Country,PostCode")] InsuredPerson insuredPerson, string email, string password)
        {
            ModelState.Remove("ApplicationUserId");
            ModelState.Remove("ApplicationUser");

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = insuredPerson.FullName
                };

                var result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Client");

                    insuredPerson.ApplicationUserId = user.Id;
                    _context.Add(insuredPerson);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Create successfully.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }

            return View(insuredPerson);
        }

        // GET: InsuredPersons/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var insuredPerson = await _context.InsuredPersons.FindAsync(id);
            if (insuredPerson == null)
            {
                return NotFound();
            }
            ViewData["ApplicationUserId"] = new SelectList(_context.Users, "Id", "Id", insuredPerson.ApplicationUserId);
            return View(insuredPerson);
        }

        // POST: InsuredPersons/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,DateOfBirth,PhoneNumber,Street,City,Country,PostCode,ApplicationUserId")] InsuredPerson model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }
            ModelState.Remove("ApplicationUser");

            if (ModelState.IsValid)
            {
                var insuredPerson = await _context.InsuredPersons.FindAsync(id);
                if (insuredPerson == null) return NotFound();

                model.ApplicationUserId = insuredPerson.ApplicationUserId;
                model.ApplicationUser = insuredPerson.ApplicationUser;

                // 🔹 Aktualizuješ vlastnosti
                insuredPerson.FullName = model.FullName;
                insuredPerson.DateOfBirth = model.DateOfBirth;
                insuredPerson.PhoneNumber = model.PhoneNumber;
                insuredPerson.Street = model.Street;
                insuredPerson.City = model.City;
                insuredPerson.Country = model.Country;
                insuredPerson.PostCode = model.PostCode;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InsuredPersonExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["SuccessMessage"] = "Saved successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }
        [Authorize(Roles = "Admin,Agent")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var insuredPerson = await _context.InsuredPersons
                .FirstOrDefaultAsync(m => m.Id == id);
            if (insuredPerson == null)
            {
                return NotFound();
            }

            return View(insuredPerson);
        }
        // GET: InsuredPersons/Delete/5
        [Authorize(Roles = "Admin,Agent")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var insuredPerson = await _context.InsuredPersons
                .Include(i => i.ApplicationUser)
                .Include(i => i.Insurances)
                    .ThenInclude(ins => ins.Claims)
                .Include(i => i.Insurances)
                    .ThenInclude(ins => ins.Participants)
                .Include(i => i.InsuranceParticipants)
                .Include(i => i.InsuranceClaims)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (insuredPerson != null)
            {
                foreach (var insurance in insuredPerson.Insurances)
                {
                    if (insurance.Claims.Any())
                        _context.InsuranceClaims.RemoveRange(insurance.Claims);

                    if (insurance.Participants.Any())
                        _context.InsuranceParticipants.RemoveRange(insurance.Participants);
                }

                if (insuredPerson.Insurances.Any())
                    _context.Insurances.RemoveRange(insuredPerson.Insurances);

                if (insuredPerson.InsuranceClaims.Any())
                    _context.InsuranceClaims.RemoveRange(insuredPerson.InsuranceClaims);

                if (insuredPerson.InsuranceParticipants.Any())
                    _context.InsuranceParticipants.RemoveRange(insuredPerson.InsuranceParticipants);

                if (insuredPerson.ApplicationUser != null)
                {
                    await _userManager.DeleteAsync(insuredPerson.ApplicationUser);
                }
                _context.InsuredPersons.Remove(insuredPerson);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "The person and all related data were successfully deleted.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool InsuredPersonExists(int id)
        {
            return _context.InsuredPersons.Any(e => e.Id == id);
        }
    }
}