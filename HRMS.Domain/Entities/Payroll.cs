using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Domain.Entities
{
    public class Payroll
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }

        public decimal HRA { get; set; }
        public decimal DA { get; set; }
        public decimal PF { get; set; }
        public decimal TDS { get; set; }
        public decimal NetSalary { get; set; }

    }
}
