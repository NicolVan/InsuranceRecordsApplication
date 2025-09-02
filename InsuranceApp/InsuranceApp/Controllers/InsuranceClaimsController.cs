using InsuranceApp.Data;
using InsuranceApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NETCore.MailKit.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IEmailService = InsuranceApp.Models.IEmailService;

namespace InsuranceApp.Controllers
{
    public class InsuranceClaimsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;

        public InsuranceClaimsController(ApplicationDbContext context,
                                         UserManager<ApplicationUser> userManager,
                                         IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }

        // GET: InsuranceClaims
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);
            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("Admin") || roles.Contains("Agent"))
            {
                var allClaims = await _context.InsuranceClaims
                    .Include(i => i.Insurance)
                    .Include(i => i.InsuredPerson)
                    .ToListAsync();

                return View(allClaims);
            }
            else
            {
                var insuredPerson = await _context.InsuredPersons
                    .FirstOrDefaultAsync(ip => ip.ApplicationUserId == userId);

                if (insuredPerson == null)
                    return Unauthorized();

                var claims = await _context.InsuranceClaims
                    .Where(c => c.InsuredPersonId == insuredPerson.Id)
                    .Include(c => c.InsuredPerson)
                    .Include(c => c.Insurance)
                    .ToListAsync();

                return View(claims);
            }
        }

        // GET: InsuranceClaims/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var insuranceClaim = await _context.InsuranceClaims
                .Include(i => i.Insurance)
                .Include(i => i.InsuredPerson)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (insuranceClaim == null)
            {
                return NotFound();
            }

            return View(insuranceClaim);
        }

        // GET: InsuranceClaims/Create
        public IActionResult Create(int insuranceId)
        {
            var insurance = _context.Insurances
                .Include(i => i.InsuredPerson)
                .FirstOrDefault(i => i.InsuranceId == insuranceId);

            if (insurance == null)
                return NotFound();

            var model = new InsuranceClaim
            {
                InsuranceId = insurance.InsuranceId,
                InsuredPersonId = insurance.InsuredPersonId,
                OccurredOn = DateTime.Today
            };
            ViewBag.Statuses = Enum.GetValues(typeof(InsuranceClaim.IncidentStatus))
                    .Cast<InsuranceClaim.IncidentStatus>()
                    .Select(s => new SelectListItem
                    {
                        Value = s.ToString(),
                        Text = s.ToString()
                    })
                    .ToList();
            ViewData["InsuranceName"] = insurance.InsuranceName;
            ViewData["InsuredPersonName"] = insurance.InsuredPerson.FullName;

            return View(model);
        }

        // POST: InsuranceClaims/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InsuranceClaim insuranceClaim, int insuranceId)
        {
            if (!ModelState.IsValid)
            {
                var insurance = _context.Insurances
                    .Include(i => i.InsuredPerson)
                    .FirstOrDefault(i => i.InsuranceId == insuranceId);

                if (insurance != null)
                {
                    insuranceClaim.InsuranceId = insurance.InsuranceId;
                    insuranceClaim.InsuredPersonId = insurance.InsuredPersonId;
                }

                return View(insuranceClaim);
            }

            insuranceClaim.Status = InsuranceClaim.IncidentStatus.New;

            _context.Add(insuranceClaim);
            await _context.SaveChangesAsync();

            var client = await _context.InsuredPersons.FindAsync(insuranceClaim.InsuredPersonId);
            if (client != null)
            {
                var clientUser = await _userManager.FindByIdAsync(client.ApplicationUserId);
                if (clientUser != null)
                {
                    await _emailService.SendEmailAsync(clientUser.Email,
                        "An incident has been created",
                        $"Your incident {insuranceClaim.Id} has been successfully created with status {insuranceClaim.Status}");
                }
            }

            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            foreach (var admin in adminUsers)
            {
                await _emailService.SendEmailAsync(admin.Email,
                    "A new incident",
                    $"A new incident no. {insuranceClaim.Id} has been created for customer {client.FullName}");
            }

            TempData["SuccessMessage"] = "An incident has been created.";
            return RedirectToAction("Index");
        }


        // GET: InsuranceClaims/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var insuranceClaim = await _context.InsuranceClaims.FindAsync(id);
            if (insuranceClaim == null)
            {
                return NotFound();
            }

            ViewData["InsuranceId"] = new SelectList(_context.Insurances, "InsuranceId", "InsuranceName", insuranceClaim.InsuranceId);
            ViewData["InsuredPersonId"] = new SelectList(_context.InsuredPersons, "Id", "FullName", insuranceClaim.InsuredPersonId);
            ViewData["Status"] = Enum.GetValues(typeof(InsuranceClaim.IncidentStatus))
                                      .Cast<InsuranceClaim.IncidentStatus>()
                                      .Select(s => new SelectListItem
                                      {
                                          Value = s.ToString(),
                                          Text = s.ToString(),
                                          Selected = insuranceClaim.Status == s
                                      });

            return View(insuranceClaim);
        }

        // POST: InsuranceClaims/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Description,OccurredOn,EstimatedDamage,InsuranceId,InsuredPersonId,Status")] InsuranceClaim insuranceClaim)
        {
            if (id != insuranceClaim.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var original = await _context.InsuranceClaims.AsNoTracking()
                        .FirstOrDefaultAsync(i => i.Id == id);

                    _context.Update(insuranceClaim);
                    await _context.SaveChangesAsync();

                    if (original.Status != insuranceClaim.Status)
                    {
                        var client = await _context.InsuredPersons.FindAsync(insuranceClaim.InsuredPersonId);
                        if (client != null)
                        {
                            var clientUser = await _userManager.FindByIdAsync(client.ApplicationUserId);
                            if (clientUser != null)
                            {
                                await _emailService.SendEmailAsync(clientUser.Email,
                                    "Change incident status",
                                    $"The status of your incident no. {insuranceClaim.Id} has been changed to {insuranceClaim.Status}");
                            }
                        }

                        var currentUser = await _userManager.GetUserAsync(User);
                        if (await _userManager.IsInRoleAsync(currentUser, "Agent"))
                        {
                            var admins = await _userManager.GetUsersInRoleAsync("Admin");
                            foreach (var admin in admins)
                            {
                                await _emailService.SendEmailAsync(admin.Email,
                                    "The incident has been changed",
                                    $"Agent {currentUser.FullName} changed the status of incident no.{insuranceClaim.Id} to {insuranceClaim.Status}");
                            }
                        }
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InsuranceClaimExists(insuranceClaim.Id))
                        return NotFound();
                    throw;
                }

                TempData["SuccessMessage"] = "Edit successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["InsuranceId"] = new SelectList(_context.Insurances, "InsuranceId", "InsuranceName", insuranceClaim.InsuranceId);
            ViewData["InsuredPersonId"] = new SelectList(_context.InsuredPersons, "Id", "FullName", insuranceClaim.InsuredPersonId);
            return View(insuranceClaim);
        }


        // GET: InsuranceClaims/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var insuranceClaim = await _context.InsuranceClaims
                .Include(i => i.Insurance)
                .Include(i => i.InsuredPerson)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (insuranceClaim == null)
            {
                return NotFound();
            }

            return View(insuranceClaim);
        }

        // POST: InsuranceClaims/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var insuranceClaim = await _context.InsuranceClaims.FindAsync(id);
            if (insuranceClaim != null)
            {
                _context.InsuranceClaims.Remove(insuranceClaim);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Delete successfully.";
            return RedirectToAction(nameof(Index));
        }

        private bool InsuranceClaimExists(int id)
        {
            return _context.InsuranceClaims.Any(e => e.Id == id);
        }
    }
}
