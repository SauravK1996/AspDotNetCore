using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.Models
{
    public static class ModelBuilderExtension
    {
        public static void Seed(this ModelBuilder model)
        {
            model.Entity<Employee>().HasData(
                    new Employee
                    {
                        Id = 1,
                        Name = "Marry",
                        Email = "marry@marry.com",
                        Department = Dept.IT
                    },
                    new Employee
                    {
                        Id = 2,
                        Name = "John",
                        Email = "john@john.com",
                        Department = Dept.HR
                    }
                );
        }
    }
}
