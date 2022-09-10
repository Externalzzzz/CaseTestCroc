using Npgsql.EntityFrameworkCore;
using WebDbMonitoring.Models;
using Npgsql;
using Microsoft.EntityFrameworkCore;

namespace WebDbMonitoring.DbData
{
    public class DbContextLogs : DbContext
    {
        public DbContextLogs(DbContextOptions options) :base(options)
        {

        }
        public DbSet<LogEntity> log { get; set; }

    }
}
