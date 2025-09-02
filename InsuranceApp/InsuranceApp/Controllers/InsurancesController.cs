using InsuranceApp.Data;
using InsuranceApp.Models;
using InsuranceApp.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace InsuranceApp.Controllers
{
    public class InsurancesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public InsurancesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;

        }

        // GET: Insurances
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);
            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("Admin") || roles.Contains("Agent"))
            {
                var allPolicies = await _context.Insurances.Include(i => i.InsuredPerson).ToListAsync();
                return View(allPolicies);
            }
            else
            {
                var insuredPerson = await _context.InsuredPersons
                    .FirstOrDefaultAsync(ip => ip.ApplicationUserId == userId);

                if (insuredPerson == null)
                    return Unauthorized();

                var policies = await _context.Insurances
                    .Where(i => i.InsuredPersonId == insuredPerson.Id)
                    .Include(i => i.InsuredPerson)
                    .ToListAsync();

                return View(policies);
            }
        }

        // GET: Insurances/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var insurance = await _context.Insurances
                   .Include(i => i.InsuredPerson)
                   .Include(i => i.Claims)
                   .Include(i =>i.Participants)
                        .ThenInclude(p => p.InsuredPerson)
                   .FirstOrDefaultAsync(m => m.InsuranceId == id);

            if (insurance == null)
            {
                return NotFound();
            }

            return View(insurance);
        }

        // GET: Insurances/Create
        public async Task<IActionResult> Create(int insuredPersonId)
        {
            var insuredPerson = await _context.InsuredPersons.FindAsync(insuredPersonId);
            var model = new InsuranceCreateEditViewModel
            {
                InsuredPersonId = insuredPersonId,
                PossiblePersons = await _context.InsuredPersons.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.FullName
                }).ToListAsync(),
                
            };
            ViewData["InsuredPersonName"] = insuredPerson?.FullName ?? "Unknown";

            return View(model);
        }


        // POST: Insurances/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InsuranceCreateEditViewModel model)
        {

            if (!ModelState.IsValid)
            {
                model.PossiblePersons = await _context.InsuredPersons.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.FullName
                }).ToListAsync();
                return View(model);
            }

            var insurance = new Insurance
            {
                InsuranceName = model.InsuranceName,
                SubjectName = model.SubjectName,
                Amount = model.Amount,
                From = model.From,
                To = model.To,
                InsuredPersonId=model.PolicyHolderId,
                Participants = new List<InsuranceParticipant>()
            };

            insurance.Participants.Add(new InsuranceParticipant
            {
                Role = InsuranceRole.PolicyHolder,
                InsuredPersonId = model.PolicyHolderId
            });

            foreach (var insuredId in model.InsuredPersonIds)
            {
                insurance.Participants.Add(new InsuranceParticipant
                {
                    Role = InsuranceRole.Insured,
                    InsuredPersonId = insuredId
                });
            }

            _context.Insurances.Add(insurance);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Insurances/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var insurance = await _context.Insurances
                .Include(i => i.Participants)
                .FirstOrDefaultAsync(i => i.InsuranceId == id);

            if (insurance == null) return NotFound();

            var model = new InsuranceCreateEditViewModel
            {
                InsuranceId = insurance.InsuranceId,
                InsuranceName = insurance.InsuranceName,
                SubjectName = insurance.SubjectName,
                Amount = insurance.Amount,
                From = insurance.From,
                To = insurance.To,
                PolicyHolderId = insurance.Participants
                    .FirstOrDefault(p => p.Role == InsuranceRole.PolicyHolder)?.InsuredPersonId ?? 0,
                InsuredPersonIds = insurance.Participants
                    .Where(p => p.Role == InsuranceRole.Insured)
                    .Select(p => p.InsuredPersonId)
                    .ToList(),
                PossiblePersons = await _context.InsuredPersons.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.FullName
                }).ToListAsync()
            };
            return View(model);
        }

        // POST: Insurances/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(InsuranceCreateEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.PossiblePersons = await _context.InsuredPersons.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.FullName
                }).ToListAsync();
                return View(model);
            }
            var insurance = await _context.Insurances
                .Include(i => i.Participants)
                .FirstOrDefaultAsync(i => i.InsuranceId == model.InsuranceId);

            if (insurance == null)
                return NotFound();
            insurance.InsuranceName = model.InsuranceName;
            insurance.SubjectName = model.SubjectName;
            insurance.Amount = model.Amount;
            insurance.From = model.From;
            insurance.To = model.To;

            var policyHolderParticipant = insurance.Participants.FirstOrDefault(p => p.Role == InsuranceRole.PolicyHolder);
            if (policyHolderParticipant != null)
            {
                policyHolderParticipant.InsuredPersonId = model.PolicyHolderId;
            }
            else
            {
                insurance.Participants.Add(new InsuranceParticipant
                {
                    Role = InsuranceRole.PolicyHolder,
                    InsuredPersonId = model.PolicyHolderId
                });
            }

            var insuredParticipants = insurance.Participants.Where(p => p.Role == InsuranceRole.Insured).ToList();
            foreach (var participant in insuredParticipants)
            {
                if (!model.InsuredPersonIds.Contains(participant.InsuredPersonId))
                {
                    _context.Remove(participant);
                }
            }

            var existingIds = insuredParticipants.Select(p => p.InsuredPersonId).ToHashSet();
            foreach (var insuredId in model.InsuredPersonIds)
            {
                if (!existingIds.Contains(insuredId))
                {
                    insurance.Participants.Add(new InsuranceParticipant
                    {
                        Role = InsuranceRole.Insured,
                        InsuredPersonId = insuredId
                    });
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Edit successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InsuranceExists(model.InsuranceId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // GET: Insurances/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var insurance = await _context.Insurances
                .Include(i => i.InsuredPerson)
                .FirstOrDefaultAsync(m => m.InsuranceId == id);
            if (insurance == null)
            {
                return NotFound();
            }

            return View(insurance);
        }

        // POST: Insurances/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var insurance = await _context.Insurances.FindAsync(id);
            if (insurance != null)
            {
                _context.Insurances.Remove(insurance);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Delete successfully.";
            return RedirectToAction(nameof(Index));
        }

        private bool InsuranceExists(int id)
        {
            return _context.Insurances.Any(e => e.InsuranceId == id);
        }
    }
}
