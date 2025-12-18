using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkshopManager.DAL.EF;
using WorkshopManager.Services;
using WorkshopManager.ViewModels;

namespace WorkshopManager.Web.Controllers;

[Authorize(Roles = "Owner")]
public class MechanicStatisticsController : Controller
{
    private readonly IMechanicStatisticsService _statisticsService;
    private readonly IPdfGeneratorService _pdfGenerator;
    private readonly ApplicationDbContext _context;

    public MechanicStatisticsController(
        IMechanicStatisticsService statisticsService,
        IPdfGeneratorService pdfGenerator,
        ApplicationDbContext context)
    {
        _statisticsService = statisticsService;
        _pdfGenerator = pdfGenerator;
        _context = context;
    }

    // GET: MechanicStatistics
    public async Task<IActionResult> Index()
    {
        var mechanics = await _context.Mechanics
            .OrderBy(m => m.LastName)
            .ThenBy(m => m.FirstName)
            .ToListAsync();

        return View(mechanics);
    }

    // POST: MechanicStatistics/Generate
    [HttpPost]
    public async Task<IActionResult> Generate([FromForm] GenerateStatisticsRequest request)
    {
        try
        {
            var statistics = await _statisticsService.GenerateStatisticsAsync(request);
            return View("Preview", statistics);
        }
        catch (ArgumentException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: MechanicStatistics/DownloadPdf
    [HttpPost]
    public async Task<IActionResult> DownloadPdf([FromForm] GenerateStatisticsRequest request)
    {
        try
        {
            var statistics = await _statisticsService.GenerateStatisticsAsync(request);
            var pdfBytes = _pdfGenerator.GenerateMechanicStatisticsPdf(statistics);

            var fileName = $"Statystyki_{statistics.MechanicFullName.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (ArgumentException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: MechanicStatistics/Compare
    public async Task<IActionResult> Compare()
    {
        var mechanics = await _context.Mechanics
            .OrderBy(m => m.LastName)
            .ThenBy(m => m.FirstName)
            .ToListAsync();

        return View(mechanics);
    }

    // POST: MechanicStatistics/CompareGenerate
    [HttpPost]
    public async Task<IActionResult> CompareGenerate([FromForm] CompareMechanicsRequest request)
    {
        try
        {
            var comparison = await _statisticsService.CompareMechanicsAsync(request);
            return View("ComparePreview", comparison);
        }
        catch (ArgumentException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Compare));
        }
    }

    // POST: MechanicStatistics/CompareDownloadPdf
    [HttpPost]
    public async Task<IActionResult> CompareDownloadPdf([FromForm] CompareMechanicsRequest request)
    {
        try
        {
            var comparison = await _statisticsService.CompareMechanicsAsync(request);
            var pdfBytes = _pdfGenerator.GenerateMechanicsComparisonPdf(comparison);

            var fileName = $"Porownanie_Mechanikow_{DateTime.Now:yyyyMMdd}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (ArgumentException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Compare));
        }
    }
}
