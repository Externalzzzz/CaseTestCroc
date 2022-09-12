using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Reflection.Emit;
using WebDbMonitoring.DbData;
using WebDbMonitoring.Models;

namespace WebDbMonitoring.Controllers
{
    public class HomeController : Controller
    {
        private const string DateFormat = "yyyy-MM-dd HH:mm:ss";
        private readonly DbContextLogs _context;


        PerformanceCounter total_CPU = new PerformanceCounter("Processor Information", "% Processor Time", "_Total");
        PerformanceCounter total_Mem = new PerformanceCounter("Memory", "Available MBytes");
        PerformanceCounter total_Time = new PerformanceCounter("System", "System Up Time");

        public HomeController(DbContextLogs context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        private readonly ChartModel _errorReturn = new ChartModel(new List<string>(), new List<int>());

        [HttpPost]
        public ChartModel GetLogs()
        {
            List<object> data = new List<object>();
            DateTime tempDate = DateTime.Now.AddSeconds(-1);
            List<DateTime> labels = new List<DateTime>();
            List<int> LogsCount = new List<int>();
            for (int i = 10; i >= 0; i--)
            {
                labels.Add(tempDate.AddSeconds(-i));
            }


            try
            {
                LogsCount = (from label in labels
                        join t in (from l in _context.log
                            where l.ts >= new DateTime(labels[0].Year, labels[0].Month, labels[0].Day, labels[0].Hour,
                                labels[0].Minute, labels[0].Second)
                            where l.ts <= new DateTime(tempDate.Year, tempDate.Month, tempDate.Day, tempDate.Hour,
                                tempDate.Minute, tempDate.Second)
                            orderby l.ts descending
                            group l by l.ts
                            into gr
                            select new { ts = gr.Key, Count = gr.Count() }).AsNoTracking() on new DateTime(label.Year,
                            label.Month, label.Day, label.Hour, label.Minute, label.Second) equals t.ts into gr
                        from g in gr.DefaultIfEmpty(null)
                        select g?.Count ?? 0)
                    .ToList();
            }
            catch
            {
                return _errorReturn;
            }

            return new ChartModel(labels.Select(x => x.ToString(DateFormat)).ToList(), LogsCount);
        }

        [HttpPost]
        public ChartModel GetCertainLogs(DateRange dateRange)
        {
            var data = new List<object>();
            var labels = new List<DateTime>();
            labels.Add(dateRange.StartDate);
            DateTime dr = dateRange.StartDate;

            List<int> LogsCount = new();
            try
            {
                switch (dateRange.Accurancy)
                {
                    case 1:
                    {
                        while (dr < dateRange.EndDate.AddDays(1))
                        {
                            dr = dr.AddMinutes(1);
                            labels.Add(dr);
                        }


                        LogsCount = (from label in labels
                                join t in (from l in _context.log
                                        orderby l.ts
                                        group l by new DateTime(l.ts.Year, l.ts.Month, l.ts.Day, l.ts.Hour, l.ts.Minute,
                                            0)
                                        into gr
                                        select new { ts = gr.Key, Count = gr.Count() }).AsNoTracking()
                                    on label equals t.ts into gr
                                from g in gr.DefaultIfEmpty(null)
                                select g?.Count ?? 0)
                            .ToList();

                        break;
                    }
                    case 2:
                    {
                        while (dr < dateRange.EndDate.AddDays(1))
                        {
                            dr = dr.AddHours(1);
                            labels.Add(dr);
                        }

                        LogsCount = (from label in labels
                            join t in (from l in _context.log
                                    orderby l.ts
                                    group l by new DateTime(l.ts.Year, l.ts.Month, l.ts.Day, l.ts.Hour, 0, 0)
                                    into gr
                                    select new { ts = gr.Key, Count = gr.Count() })
                                .AsNoTracking() on label equals t.ts into gr
                            from g in gr.DefaultIfEmpty(null)
                            select g?.Count ?? 0).ToList();


                        break;
                    }
                    case 3:
                    {
                        while (dr < dateRange.EndDate)
                        {
                            dr = dr.AddDays(1);
                            labels.Add(dr);
                        }


                        LogsCount = (from label in labels
                            join t in (from l in _context.log
                                    orderby l.ts
                                    group l by new DateTime(l.ts.Year, l.ts.Month, l.ts.Day)
                                    into gr
                                    select new { ts = gr.Key, Count = gr.Count() })
                                .AsNoTracking() on label equals t.ts into gr
                            from g in gr.DefaultIfEmpty(null)
                            select g?.Count ?? 0).ToList();
                        break;
                    }
                }
            }
            catch
            {
                return _errorReturn;
            }

            return new ChartModel(labels.Select(x => x.ToString(DateFormat)).ToList(), LogsCount);
        }

        [HttpGet]
        public JsonResult GetAllLogs()
        {
            List<LogEntity> res = new();
            try
            {
                res = (from l in _context.log orderby l.ts descending select l).AsNoTracking().ToList();
            }
            catch
            {
                return Json(_errorReturn);
            }

            return Json(res);
        }

        [HttpGet]
        public JsonResult GetAllLogsFiltered(DateRange dateRange)
        {
            List<LogEntity> res = new();
            try
            {
                res = (from l in _context.log
                    where l.ts >= dateRange.StartDate
                    where l.ts < dateRange.EndDate.AddDays(1)
                    orderby l.ts descending
                    select l).AsNoTracking().ToList();
            }
            catch
            {
                return Json(_errorReturn);
            }

            return Json(res);
        }

        [HttpGet]
        public JsonResult GetMonitoringData()
        {
            List<string> res = new();
            total_CPU.NextValue();
            total_Mem.NextValue();
            total_Time.NextValue();

            Thread.Sleep(1000);
            res.Add(((int)total_CPU.NextValue()).ToString());
            res.Add(((int)total_Mem.NextValue()).ToString());
            res.Add(TimeSpan.FromSeconds(total_Time.NextValue()).ToString(@"hh\:mm"));


            return Json(res);
        }
    }
}