using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InsuranceApp.Data;
using InsuranceApp.Models;

namespace InsuranceApp.Controllers
{
    public class InsuranceParticipantsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InsuranceParticipantsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: InsuranceParticipants
        public async Task<IActionResult> Index(int? insuranceId, InsuranceRole? role)
        {
            var query = _context.InsuranceParticipants
                .Include(i => i.Insurance)
                .Include(i => i.InsuredPerson)
                .AsQueryable();

            if (insuranceId.HasValue)
            {
                query = query.Where(p => p.InsuranceId == insuranceId.Value);
            }

            if (role.HasValue)
            {
                query = query.Where(p => p.Role == role.Value);
            }

            ViewData["InsuranceId"] = new SelectList(_context.Insurances, "InsuranceId", "InsuranceName", insuranceId);
            ViewData["RoleList"] = Enum.GetValues(typeof(InsuranceRole))
                                  .Cast<InsuranceRole>()
                                  .Select(r => new SelectListItem
                                  {
                                      Value = ((int)r).ToString(),
                                      Text = r.ToString(),
                                      Selected = role.HasValue && role.Value == r
                                  }).ToList();

            return View(await query.ToListAsync());
        }

        // GET: InsuranceParticipants/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var insuranceParticipant = await _context.InsuranceParticipants
                .Include(i => i.Insurance)
                .Include(i => i.InsuredPerson)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (insuranceParticipant == null)
            {
                return NotFound();
            }

            return View(insuranceParticipant);
        }

        // GET: InsuranceParticipants/Create
        public IActionResult Create()
        {
            ViewBag.RoleList = Enum.GetValues(typeof(InsuranceRole))
                          .Cast<InsuranceRole>()
                          .Select(r => new SelectListItem
                          {
                              Value = ((int)r).ToString(),
                              Text = r.ToString()
                          }).ToList();

            ViewData["InsuranceId"] = new SelectList(_context.Insurances, "InsuranceId", "InsuranceName");
            ViewData["InsuredPersonId"] = new SelectList(_context.InsuredPersons, "Id", "FullName");
            return View();
        }

        // POST: InsuranceParticipants/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Role,InsuranceId,InsuredPersonId")] InsuranceParticipant insuranceParticipant)
        {
            if (ModelState.IsValid)
            {
                _context.Add(insuranceParticipant);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Create successfully.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.RoleList = Enum.GetValues(typeof(InsuranceRole))
                 .Cast<InsuranceRole>()
                 .Select(r => new SelectListItem
                 {
                     Value = ((int)r).ToString(),
                     Text = r.ToString()
                 }).ToList();

            ViewData["InsuranceId"] = new SelectList(_context.Insurances, "InsuranceId", "InsuranceName", insuranceParticipant.InsuranceId);
            ViewData["InsuredPersonId"] = new SelectList(_context.InsuredPersons, "Id", "FullName", insuranceParticipant.InsuredPersonId);
            return View(insuranceParticipant);
        }

        // GET: InsuranceParticipants/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var insuranceParticipant = await _context.InsuranceParticipants.FindAsync(id);
            if (insuranceParticipant == null)
            {
                return NotFound();
            }

            ViewBag.RoleList = Enum.GetValues(typeof(InsuranceRole))
                 .Cast<InsuranceRole>()
                 .Select(r => new SelectListItem
                 {
                     Value = ((int)r).ToString(),
                     Text = r.ToString()
                 }).ToList();

            ViewData["InsuranceId"] = new SelectList(_context.Insurances, "InsuranceId", "InsuranceName", insuranceParticipant.InsuranceId);
            ViewData["InsuredPersonId"] = new SelectList(_context.InsuredPersons, "Id", "FullName", insuranceParticipant.InsuredPersonId);
            return View(insuranceParticipant);
        }

        // POST: InsuranceParticipants/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Role,InsuranceId,InsuredPersonId")] InsuranceParticipant insuranceParticipant)
        {
            if (id != insuranceParticipant.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(insuranceParticipant);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InsuranceParticipantExists(insuranceParticipant.Id))
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

            ViewBag.RoleList = Enum.GetValues(typeof(InsuranceRole))
                  .Cast<InsuranceRole>()
                  .Select(r => new SelectListItem
                  {
                      Value = ((int)r).ToString(),
                      Text = r.ToString()
                  }).ToList();

            ViewData["InsuranceId"] = new SelectList(_context.Insurances, "InsuranceId", "InsuranceName", insuranceParticipant.InsuranceId);
            ViewData["InsuredPersonId"] = new SelectList(_context.InsuredPersons, "Id", "FullName", insuranceParticipant.InsuredPersonId);
            return View(insuranceParticipant);
        }

        // GET: InsuranceParticipants/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var insuranceParticipant = await _context.InsuranceParticipants
                .Include(i => i.Insurance)
                .Include(i => i.InsuredPerson)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (insuranceParticipant == null)
            {
                return NotFound();
            }

            return View(insuranceParticipant);
        }

        // POST: InsuranceParticipants/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var insuranceParticipant = await _context.InsuranceParticipants.FindAsync(id);
            if (insuranceParticipant != null)
            {
                _context.InsuranceParticipants.Remove(insuranceParticipant);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "The record was successfully deleted.";
            return RedirectToAction(nameof(Index));
        }

        private bool InsuranceParticipantExists(int id)
        {
            return _context.InsuranceParticipants.Any(e => e.Id == id);
        }
        public async Task<IActionResult> Reports()
        {
            var reportData = await _context.InsuranceParticipants
                .Include(ip => ip.Insurance)
                .GroupBy(ip => ip.Insurance.InsuranceName)
                .Select(g => new
                {
                    InsuranceName = g.Key,
                    TotalParticipants = g.Count(),
                    TotalInsured = g.Count(x => x.Role == InsuranceRole.Insured),
                    TotalPolicyHolders = g.Count(x => x.Role == InsuranceRole.PolicyHolder)
                })
                .ToListAsync();

            return View(reportData);
        }
    }
}
