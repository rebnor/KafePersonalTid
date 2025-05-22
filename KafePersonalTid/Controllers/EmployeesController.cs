using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KafePersonalTid.Models;
using KafePersonalTid.Services;

namespace KafePersonalTid.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PdfService _pdfService;

        public EmployeesController(AppDbContext context, PdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
        }

        // GET: Employees
        public async Task<IActionResult> Index()
        {
            var employees = await _context.Employees
                .OrderBy(e => e.Name)
                .Include(e => e.WorkEntries)  // Se till att vi hämtar relaterade WorkEntries
                .ToListAsync();

            // Skapa en lista med anonyma objekt som innehåller Employee och de beräknade totala timmarna
            var employeeViewModel = employees.Select(employee => new
            {
                employee.Id,
                employee.Name,
                WorkEntries = employee.WorkEntries,
                // Beräkna vecko-totalen
                WeekTotal = employee.WorkEntries
                    .Where(w => w.Date >= DateTime.Now.StartOfWeek(DayOfWeek.Monday) && w.Date <= DateTime.Now.StartOfWeek(DayOfWeek.Monday).AddDays(6))
                    .Sum(w => w.HoursWorked),
                // Beräkna månad-totalen
                MonthTotal = employee.WorkEntries
                    .Where(w => w.Date.Month == DateTime.Now.Month && w.Date.Year == DateTime.Now.Year)
                    .Sum(w => w.HoursWorked)
            }).ToList();

            return View(employeeViewModel);  // Skicka den anonyma listan till vyn
        }




        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // GET: Employees/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Employees/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,HourlyWage,PersonalNumber,PhoneNumber,Address,EmergencyContact")] Employee employee)
        {
            if (!ModelState.IsValid)
            {
                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key]?.Errors;
                    if (errors != null && errors.Count > 0)
                    {
                        foreach (var error in errors)
                        {
                            Console.WriteLine($"Key: {key}, Error: {error.ErrorMessage}");
                        }
                    }
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }




        // GET
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            return View(employee);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,PersonalNumber,PhoneNumber,Address,EmergencyContact,HourlyWage")] Employee employee)
        {
            if (id != employee.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(DetailedView), new { id = employee.Id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Employees.Any(e => e.Id == employee.Id))
                        return NotFound();
                    else throw;
                }
            }
            return View(employee);
        }

        // GET: Employee/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees
                .FirstOrDefaultAsync(m => m.Id == id);

            if (employee == null) return NotFound();

            return View(employee);
        }

        // POST: Employee/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.WorkEntries) // Ta bort även arbetspass!
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee != null)
            {
                _context.WorkEntries.RemoveRange(employee.WorkEntries); // Tar bort kopplade arbetspass
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }

       
        [HttpPost]
        public async Task<IActionResult> AddHours(int employeeId, DateTime date, int hours, int minutes)
        {
            var employee = await _context.Employees
                .Include(e => e.WorkEntries)
                .FirstOrDefaultAsync(e => e.Id == employeeId);

            if (employee != null)
            {
                // Omvandla minuter till timmar
                double totalHours = hours + (minutes / 60.0);

                var workEntry = new WorkEntry
                {
                    EmployeeId = employee.Id,
                    Date = date,
                    HoursWorked = totalHours,
                    StartTime = DateTime.Now.TimeOfDay
                };

                // Lägg till nya timmar i WorkEntries-tabellen
                _context.WorkEntries.Add(workEntry);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View();  // Om vi inte hittar anställd, returnera en felvy
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> DetailedView(int? id, int? year, int? month)
        {
            if (id == null)
                return NotFound();

            var employee = await _context.Employees
                .Include(e => e.WorkEntries)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
                return NotFound();

            var workEntries = employee.WorkEntries.AsQueryable();

            if (year.HasValue && month.HasValue && month.Value > 0)
            {
                workEntries = workEntries.Where(w => w.Date.Year == year && w.Date.Month == month);
            }

            var monthGroups = workEntries
    .ToList() // 💡 Detta är nyckeln!
    .GroupBy(w => new { w.Date.Year, w.Date.Month })
    .OrderBy(g => g.Key.Year)
    .ThenBy(g => g.Key.Month)
    .Select(monthGroup =>
    {
        var monthKey = $"{monthGroup.Key.Year}-{monthGroup.Key.Month:D2}";
        var monthName = new DateTime(monthGroup.Key.Year, monthGroup.Key.Month, 1)
                            .ToString("MMMM yyyy", new System.Globalization.CultureInfo("sv-SE"));

        var weeks = monthGroup
            .GroupBy(w => w.Date.StartOfWeek(DayOfWeek.Monday))
            .OrderBy(wg => wg.Key)
            .Select(weekGroup => new WeekGroup
            {
                WeekStart = weekGroup.Key,
                TotalHours = weekGroup.Sum(w => w.HoursWorked),
                Entries = weekGroup.OrderBy(w => w.Date).ToList()
            })
            .ToList();

        return new MonthGroup
        {
            MonthKey = monthKey,
            MonthName = monthName,
            MonthDate = new DateTime(monthGroup.Key.Year, monthGroup.Key.Month, 1),
            TotalHours = monthGroup.Sum(w => w.HoursWorked),
            Weeks = weeks
        };
    })
    .ToList();


            var viewModel = new EmployeeDetailsViewModel
            {
                Employee = employee,
                GroupedByMonthAndWeek = monthGroups
            };

            return View(viewModel);
        }


        // GET: Employee/AddWorkEntry/5
        public IActionResult AddWorkEntry(int id)
        {
            var workEntry = new WorkEntry
            {
                EmployeeId = id,
                Date = DateTime.Today,
                StartTime = DateTime.Now.TimeOfDay
            };

            return View(workEntry);


        }

        // POST: Employee/AddWorkEntry/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddWorkEntry(int employeeId, int Hours, int Minutes, DateTime Date, TimeSpan StartTime)
        {
            var workEntry = new WorkEntry
            {
                EmployeeId = employeeId,
                Date = Date.Date + StartTime,
                HoursWorked = Hours + (Minutes / 60.0),
                StartTime = StartTime
            };

            if (!ModelState.IsValid)
            {
                return View(workEntry);
            }

            _context.WorkEntries.Add(workEntry);
            await _context.SaveChangesAsync();

            return RedirectToAction("DetailedView", new { id = employeeId });
        }



        public async Task<IActionResult> EditWorkEntry(int id)
        {
            var entry = await _context.WorkEntries.FindAsync(id);
            if (entry == null)
                return NotFound();

            return View("~/Views/WorkEntries/EditWorkEntry.cshtml", entry);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditWorkEntry(int id, [Bind("Id,EmployeeId,Date,HoursWorked,StartTime")] WorkEntry entry)
        {
            if (id != entry.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(entry);
                await _context.SaveChangesAsync();
                return RedirectToAction("DetailedView", new { id = entry.EmployeeId });
            }

            // Felsökning
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine("Model error: " + error.ErrorMessage);
            }

            return View("~/Views/WorkEntries/EditWorkEntry.cshtml", entry);
        }



        public async Task<IActionResult> DeleteWorkEntry(int id)
        {
            var entry = await _context.WorkEntries.FindAsync(id);
            if (entry == null)
                return NotFound();

            return View("~/Views/WorkEntries/DeleteWorkEntry.cshtml", entry);

        }

        [HttpPost, ActionName("DeleteWorkEntry")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteWorkEntryConfirmed(int id)
        {
            var entry = await _context.WorkEntries.FindAsync(id);

            if (entry == null)
                return NotFound();

            int employeeId = entry.EmployeeId;

            _context.WorkEntries.Remove(entry);
            await _context.SaveChangesAsync();

            return RedirectToAction("DetailedView", new { id = employeeId });
        }


        [HttpGet]
        public IActionResult ExportToPdf(int id, int year, int month)
        {
            var employee = _context.Employees
                .Include(e => e.WorkEntries)
                .FirstOrDefault(e => e.Id == id);

            if (employee == null)
            {
                return NotFound();
            }

            int? selectedMonth = (month == 0) ? null : month;

            var pdfBytes = _pdfService.GenerateEmployeeReportPdf(employee, year, selectedMonth);

            return File(pdfBytes, "application/pdf", $"Arbetsrapport_{employee.Name}_{year}_{month}.pdf");
        }




    }
}
