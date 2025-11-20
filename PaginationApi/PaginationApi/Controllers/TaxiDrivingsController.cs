using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using PaginationApi.Data;
using PaginationApi.Models;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;


namespace PaginationApi.Controllers
{
    public class TaxiDrivingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PaginationSettings _paginationSettings;
        public TaxiDrivingsController(ApplicationDbContext context, 
            IOptions<PaginationSettings> paginationSettings)
        {
            _context = context;
            _paginationSettings = paginationSettings.Value;
        }

        public async Task<IActionResult> Index(
            int page = 1,
            int? pageSize = null,
            string sortColumn = "medallion",
            string sortOrder = "asc",
            string? searchString = null,
            string? searchColumn = null,
            string? searchText = null
            )
        {

            var pagination = new PaginationParams(
                _paginationSettings.DefaultPageSize,
                _paginationSettings.MaxPageSize
                )
            {
                PageNumber = page,
                PageSize = pageSize ?? _paginationSettings.DefaultPageSize
            };

            var query = _context.TaxiDrivings.AsQueryable();
            query = ApplyFilteringSorting(query, sortColumn, sortOrder, searchString, searchColumn, searchText);
          

            int totalCount = query.Count();
            
            var pagedData = query
                .Skip((page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToList();

            ViewBag.CurrentPage = pagination.PageNumber;

            ViewBag.PageSize = pagination.PageSize;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pagination.PageSize);
            ViewBag.SortColumn = sortColumn;
            ViewBag.SortOrder = sortOrder;
            ViewBag.TotalRecords = totalCount;
            return View(pagedData);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var result = await _context.TaxiDrivings.FindAsync(id);
            
            if(result == null)
            {
                return NotFound();
            }

            return View(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaxiDrivings model)
        {
            if (id != model.id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingEntity = await _context.TaxiDrivings.FindAsync(id);

                    if (existingEntity == null)
                    {
                        return NotFound();
                    }

                    existingEntity.medallion = model.medallion;
                    existingEntity.hashLicense = model.hashLicense;
                    existingEntity.pickupTime = model.pickupTime;
                    existingEntity.dropOffTime = model.dropOffTime;
                    existingEntity.duration = model.duration;
                    existingEntity.distance = model.distance;
                    existingEntity.pLongitude = model.pLongitude;
                    existingEntity.pLatitude = model.pLatitude;
                    existingEntity.dLongitude = model.dLongitude;
                    existingEntity.dLatitude = model.dLatitude;
                    existingEntity.paymentType = model.paymentType;
                    existingEntity.fareAmount = model.fareAmount;
                    existingEntity.surchange = model.surchange;
                    existingEntity.tax = model.tax;
                    existingEntity.tipAmount = model.tipAmount;
                    existingEntity.tollsAmount = model.tollsAmount;
                    existingEntity.totalAmount = model.totalAmount;

                    _context.Entry(existingEntity).State = EntityState.Modified;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaxiExists(model.id))
                    {
                        return NotFound();
                    }
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        private bool TaxiExists(int id) { 
            return _context.TaxiDrivings.Any(t=>t.id == id);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var result = await _context.TaxiDrivings.FindAsync(id);

            if(result == null)
            {
                return NotFound();
            }

            return View(result);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _context.TaxiDrivings.FindAsync(id);
            if (result != null)
            {
                _context.TaxiDrivings.Remove(result);
                await _context.SaveChangesAsync();
            }
            
            
            return RedirectToAction(nameof(Index));
        }
       
        public IActionResult ExportCsv(
            string? sortColumn,
            string? sortOrder,
            string? searchString,
            string? searchColumn,
            string? searchText)
        {
            
            var query = _context.TaxiDrivings.AsQueryable();
            query = ApplyFilteringSorting(query, sortColumn, sortOrder, searchString, searchColumn, searchText);

            var data = query.ToList();

            var csv = new StringBuilder();

            csv.AppendLine(
                "Medallion,HashLicense,PickupTime,DropOffTime,Duration,Distance,pLongitude,pLatitude,dLongitude,dLatitude,PaymentType,FareAmount," +
                "Surchange,Tax,TipAmount,TollsAmount,TotalAmount");
             

            foreach (var t in data)
            {
                csv.AppendLine($"{t.medallion},{t.hashLicense},{t.pickupTime?.ToString("yyyy-MM-dd HH:mm")},{t.dropOffTime?.ToString("yyyy-MM-dd HH:mm")}," +
                    $"{t.distance},{t.pLongitude},{t.pLatitude},{t.dLongitude},{t.dLatitude},{t.paymentType},{t.fareAmount},{t.surchange},{t.tax}," +
                    $"{t.tipAmount},{t.tollsAmount},{t.totalAmount}");
               
            }

     
            return File(
                Encoding.UTF8.GetBytes(csv.ToString()),
                "text/csv",
                "TaxiExport.csv"
            );

        }

        public IActionResult ExportExcel(
            string? sortColumn,
            string? sortOrder,
            string? searchString,
            string? searchColumn,
            string? searchText
            )
        {
            var query = _context.TaxiDrivings.AsQueryable();
            query = ApplyFilteringSorting(query, sortColumn, sortOrder, searchString, searchColumn, searchText);

            var data = query.ToList();

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Taxi Export");

            string[] headers =
            {
                "Medallion","HashLicense","PickupTime","DropOffTime","Duration","Distance",
                "pLongitude","pLatitude","dLongitude","dLatitude","PaymentType","FareAmount",
                "Surchange","Tax","TipAmount","TollsAmount","TotalAmount"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cells[1, i+1].Value = headers[i];
            }

            int row = 2;
            foreach (var t in data)
            {
                ws.Cells[row, 1].Value = t.medallion;
                ws.Cells[row, 2].Value = t.hashLicense;
                ws.Cells[row, 3].Value = t.pickupTime?.ToString("yyyy-MM-dd HH:mm");
                ws.Cells[row, 4].Value = t.dropOffTime?.ToString("yyyy-MM-dd HH:mm");
                ws.Cells[row, 5].Value = t.duration;
                ws.Cells[row, 6].Value = t.distance;
                ws.Cells[row, 7].Value = t.pLongitude;
                ws.Cells[row, 8].Value = t.pLatitude;
                ws.Cells[row, 9].Value = t.dLongitude;
                ws.Cells[row, 10].Value = t.dLatitude;
                ws.Cells[row, 11].Value = t.paymentType;
                ws.Cells[row, 12].Value = t.fareAmount;
                ws.Cells[row, 13].Value = t.surchange;
                ws.Cells[row, 14].Value = t.tax;
                ws.Cells[row, 15].Value = t.tipAmount;
                ws.Cells[row, 16].Value = t.tollsAmount;
                ws.Cells[row, 17].Value = t.totalAmount;
                row++;
            }

            ws.Cells.AutoFitColumns();
            var fileBytes = package.GetAsByteArray();

            return File(fileBytes,
               "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
               "TaxiExport.xlsx");
        }

        private IQueryable<TaxiDrivings> ApplyFilteringSorting(
            IQueryable<TaxiDrivings> query,
            string? sortColumn,
            string? sortOrder,
            string? searchString,
            string? searchColumn,
            string? searchText)
        {
            if (!string.IsNullOrWhiteSpace(searchColumn) &&
              !string.IsNullOrWhiteSpace(searchText))
            {
                string lowerSearch = searchText.ToLower();

                var list = query.AsEnumerable();

                list = searchColumn switch
                {
                    "medallion" => list.Where(t => t.medallion.ToLower().Contains(lowerSearch)),
                    "hashLicense" => list.Where(t => t.hashLicense.ToLower().Contains(lowerSearch)),
                    "paymentType" => list.Where(t => t.paymentType.ToLower().Contains(lowerSearch)),
                    "duration" => list.Where(t => t.duration.ToString().Contains(lowerSearch)),
                    "distance" => list.Where(t => t.distance.ToString().Contains(lowerSearch)),
                    "pLongitude" => list.Where(t => t.pLongitude.ToString().Contains(lowerSearch)),
                    "pLatitude" => list.Where(t => t.pLatitude.ToString().Contains(lowerSearch)),
                    "dLongitude" => list.Where(t => t.dLongitude.ToString().Contains(lowerSearch)),
                    "dLatitude" => list.Where(t => t.dLatitude.ToString().Contains(lowerSearch)),
                    "fareAmount" => list.Where(t => t.fareAmount.ToString().Contains(lowerSearch)),
                    "surchange" => list.Where(t => t.surchange.ToString().Contains(lowerSearch)),
                    "tax" => list.Where(t => t.tax.ToString().Contains(lowerSearch)),
                    "tipAmount" => list.Where(t => t.tipAmount.ToString().Contains(lowerSearch)),
                    "tollsAmount" => list.Where(t => t.tollsAmount.ToString().Contains(lowerSearch)),
                    "totalAmount" => list.Where(t => t.totalAmount.ToString().Contains(lowerSearch)),

                    "pickupTime" => list.Where(t =>
                            t.pickupTime.HasValue &&
                            t.pickupTime.Value.ToString("yyyy-MM-dd HH:mm").Contains(lowerSearch)),

                    "dropOffTime" => list.Where(t =>
                                         t.dropOffTime.HasValue &&
                                         t.dropOffTime.Value.ToString("yyyy-MM-dd HH:mm").Contains(lowerSearch)),

                    _ => list
                };
                query = list.AsQueryable();
            }

            else if (!string.IsNullOrWhiteSpace(searchString))
            {
                string lowerSearch = searchString.ToLower();
                var list = query.AsEnumerable();

                list = list.Where(t =>
                (t.medallion != null && t.medallion.ToLower().Contains(lowerSearch)) ||
                (t.hashLicense != null && t.hashLicense.ToLower().Contains(lowerSearch)) ||
                (t.paymentType != null && t.paymentType.ToLower().Contains(lowerSearch)) ||
                (t.pickupTime.HasValue && t.pickupTime.Value.ToString("yyyy-MM-dd HH:mm").Contains(lowerSearch)) ||
                (t.dropOffTime.HasValue && t.dropOffTime.Value.ToString("yyyy-MM-dd HH:mm").Contains(lowerSearch)) ||
                t.duration.ToString().Contains(lowerSearch) ||
                t.distance.ToString().Contains(lowerSearch) ||
                t.pLongitude.ToString().Contains(lowerSearch) ||
                t.pLatitude.ToString().Contains(lowerSearch) ||
                t.dLongitude.ToString().Contains(lowerSearch) ||
                t.dLatitude.ToString().Contains(lowerSearch) ||
                t.fareAmount.ToString().Contains(lowerSearch) ||
                t.surchange.ToString().Contains(lowerSearch) ||
                t.tax.ToString().Contains(lowerSearch) ||
                t.tipAmount.ToString().Contains(lowerSearch) ||
                t.tollsAmount.ToString().Contains(lowerSearch) ||
                t.totalAmount.ToString().Contains(lowerSearch)
                );
                query = list.AsQueryable();
            }
            query = (sortColumn,sortOrder) switch
            {
                ("medallion","asc") => query.OrderBy(t => t.medallion),
                ("medallion", "desc") => query.OrderByDescending(t => t.medallion),
                ("hashLicense", "asc") => query.OrderBy(t => t.hashLicense),
                ("hashLicense", "desc") => query.OrderByDescending(t => t.hashLicense),
                ("pickupTime", "asc") => query.OrderBy(t => t.pickupTime),
                ("pickupTime", "desc") => query.OrderByDescending(t => t.pickupTime),
                ("dropOffTime", "asc") =>  query.OrderBy(t => t.dropOffTime),
                ("dropOffTime", "desc") => query.OrderByDescending(t => t.dropOffTime),
                ("duration", "asc") => query.OrderBy(t => t.duration),
                ("duration", "desc") => query.OrderByDescending(t => t.duration),
                ("distance", "asc") => query.OrderBy(t => t.distance),
                ("distance", "desc") =>query.OrderByDescending(t => t.distance),
                ("pLongitude", "asc") => query.OrderBy(t => t.pLongitude),
                ("pLongitude", "desc") => query.OrderByDescending(t => t.pLongitude),
                ("pLatitude", "asc") => query.OrderBy(t => t.pLatitude),
                ("pLatitude", "desc") => query.OrderByDescending(t => t.pLatitude),
                ("dLongitude", "asc") => query.OrderBy(t => t.dLongitude),
                ("dLongitude", "desc") => query.OrderByDescending(t => t.dLongitude),
                ("dLatitude", "asc") => query.OrderBy(t => t.dLatitude),
                ("dLatitude", "desc") => query.OrderByDescending(t => t.dLatitude),
                ("paymentType", "asc") => query.OrderBy(t => t.paymentType),
                ("paymentType", "desc") => query.OrderByDescending(t => t.paymentType),
                ("fareAmount", "asc") => query.OrderBy(t => t.fareAmount),
                ("fareAmount", "desc") => query.OrderByDescending(t => t.fareAmount),
                ("surchange", "asc") => query.OrderBy(t => t.surchange),
                ("surchange", "desc") => query.OrderByDescending(t => t.surchange),
                ("tax", "asc") => query.OrderBy(t => t.tax),
                ("tax", "desc") => query.OrderByDescending(t => t.tax),
                ("tipAmount", "asc") => query.OrderBy(t => t.tipAmount),
                ("tipAmount", "desc") => query.OrderByDescending(t => t.tipAmount),
                ("tollsAmount", "asc") => query.OrderBy(t => t.tollsAmount),
                ("tollsAmount", "desc") => query.OrderByDescending(t => t.tollsAmount),
                ("totalAmount", "asc") => query.OrderBy(t => t.totalAmount),
                ("totalAmount", "desc") => query.OrderByDescending(t => t.totalAmount),
                _ => query.OrderBy(t => t.medallion),

            };
            return query;

        }

       

    }
}
