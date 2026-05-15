using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Спротивные_достижения.Models
{
    public class ReportItem
    {
        public int Id { get; set; }
        public string OperationType { get; set; }
        public DateTime Date { get; set; }
        public string UserName { get; set; }
        public string UserRole { get; set; }
        public string ObjectName { get; set; }
        public string Details { get; set; }
    }
}
