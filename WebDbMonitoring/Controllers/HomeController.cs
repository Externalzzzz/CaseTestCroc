using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebDbMonitoring.Models;
using WebDbMonitoring.DbData;
using System.Linq;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;
using System.Net.WebSockets;
using System.Globalization;

namespace WebDbMonitoring.Controllers
{
    public class HomeController : Controller
    {
        private readonly DbContextLogs _context;
        public HomeController( DbContextLogs context)
        {
            _context = context;        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public List<object> GetLogs()
        {
            List<object> data = new List<object>();
            DateTime tempDate = DateTime.Now.AddSeconds(-1);
            List<string> labels =  new List<string>() { };
            for (int i = 10; i >= 1; i--)
            {
                labels.Add(tempDate.AddSeconds(-i).ToString("yyyy-MM-dd HH:mm:ss"));
            }
            tempDate.ToString("yyyy-MM-dd HH:mm:ss");
            data.Add(labels);

            List<int> LogsCount = (from l in _context.log
                                   where l.ts >= tempDate.AddSeconds(-10)
                                   where l.ts <= tempDate
                                   orderby l.ts
                                   group l by l.ts into gr
                                   select gr.Count()) 
                            .ToList();
            data.Add(LogsCount);

            return data;
        }
        [HttpPost]
        public List<object> GetCertainLogs(DateRange dateRange)

        {
            var data = new List<object>();
            var labels = new List<DateTime>();
            labels.Add(dateRange.StartDate);
            DateTime i = dateRange.StartDate;
            while (i <= dateRange.EndDate)
            {
                i = i.AddMinutes(1);
                labels.Add(i);
            }
            //List<int> LogsCount = (from l in _context.log
            //                       where l.ts >= dateRange.StartDate
            //                       where l.ts < dateRange.EndDate
            //                       orderby l.ts
            //                       group l by l.ts into gr
            //                       select gr.Count())
            //               .ToList();
            var tempGroupBY = (from l in _context.log orderby l.ts group l by l.ts into gr select new { ts = gr.Key, Count = gr.Count() }).ToList();
            List<int> LogsCount = (from label in labels
                                   join t in tempGroupBY on label equals t.ts into sp
                                   orderby sp
                                   group sp by sp into gr
                                   select gr.Count()).ToList();
            data.Add(new List<string> ( labels.Select(x => x.ToString("yyyy-MM-dd HH:mm:ss"))));
            data.Add(LogsCount);
            return data;
        }

    }
}