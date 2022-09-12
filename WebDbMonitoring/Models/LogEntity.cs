using System.ComponentModel.DataAnnotations;

namespace WebDbMonitoring.Models
{

    public class LogEntity
    {
        [Key]
        public int ID { get; set; }
        public string tablename { get; set; }
        public DateTime ts { get; set; }
        public string valuejson { get; set; }

    }
}
