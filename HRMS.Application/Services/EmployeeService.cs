using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _repo;

        public EmployeeService(IEmployeeRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<Employee>> GetEmployees()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<Employee> GetEmployee(int id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async Task AddEmployee(Employee employee)
        {
            await _repo.AddAsync(employee);
        }

        public async Task UpdateEmployee(Employee employee)
        {
            await _repo.UpdateAsync(employee);
        }

        public async Task DeleteEmployee(int id)
        {
            await _repo.DeleteAsync(id);
        }
    }
}
