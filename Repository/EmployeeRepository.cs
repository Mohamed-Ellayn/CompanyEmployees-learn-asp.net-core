using Contracts;
using Entities.Models;
using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Entities.RequestFeatures;
using Repository.Extensions;

namespace Repository
{
    public class EmployeeRepository : RepositoryBase<Employee>, IEmployeeRepository
    { 
        public EmployeeRepository(RepositoryContext repositoryContext) : base(repositoryContext) 
        {
        }
        public IEnumerable<Employee> GetEmployees(Guid companyId, bool trackChanges)
        {
           return FindByCondition(e => e.CompanyId.Equals(companyId), trackChanges).OrderBy(e => e.Name);
        }
        public Employee GetEmployee(Guid companyId, Guid id, bool trackChanges)
        {
            return FindByCondition(e => e.CompanyId.Equals(companyId) && e.Id.Equals(id), trackChanges).SingleOrDefault();
        }
        public void CreateEmployeeForCompany(Guid companyId, Employee employee) 
        {
            employee.CompanyId = companyId;
            Create(employee);
        }
        public void DeleteEmployee(Employee employee)
        {
            Delete(employee);
        }

        public async Task<Employee> GetEmployeeAsync(Guid companyId, Guid id, bool trackChanges)
        {
            return await FindByCondition(e => e.CompanyId.Equals(companyId) && e.Id.Equals(id), trackChanges).SingleOrDefaultAsync();
        }
        //public async Task<IEnumerable<Employee>> GetEmployeesAsync(Guid companyId, EmployeeParameters employeeParameters, bool trackChanges)
        //{
        //    return await FindByCondition(e => e.CompanyId.Equals(companyId), trackChanges)
        //        .OrderBy(e => e.Name)
        //        .Skip((employeeParameters.PageNumber - 1) * employeeParameters.PageSize)
        //        .Take(employeeParameters.PageSize)
        //        .ToListAsync();
        //}
        //public async Task<PagedList<Employee>> GetEmployeesAsync(Guid companyId, EmployeeParameters employeeParameters, bool trackChanges)
        //{ 
        //    var employees = await FindByCondition(e => e.CompanyId.Equals(companyId), trackChanges)
        //        .OrderBy(e => e.Name).
        //        ToListAsync(); 
        //    return PagedList<Employee>.ToPagedList(employees, employeeParameters.PageNumber, employeeParameters.PageSize); 
        //}

        //public async Task<PagedList<Employee>> GetEmployeesAsync(Guid companyId, EmployeeParameters employeeParameters, bool trackChanges) 
        //{
        //    var employees = await FindByCondition(e => e.CompanyId.Equals(companyId), trackChanges)
        //        .OrderBy(e => e.Name)
        //        .Skip((employeeParameters.PageNumber - 1) * employeeParameters.PageSize)
        //        .Take(employeeParameters.PageSize)
        //        .ToListAsync(); 
        //    var count = await FindByCondition(e => e.CompanyId.Equals(companyId), trackChanges).CountAsync();
        //    return new PagedList<Employee>(employees, employeeParameters.PageNumber, employeeParameters.PageSize, count); 
        //}
        //public async Task<PagedList<Employee>> GetEmployeesAsync(Guid companyId, EmployeeParameters employeeParameters, bool trackChanges)
        //{
        //    var employees = await FindByCondition(e => e.CompanyId.Equals(companyId) && (e.Age >= employeeParameters.MinAge && e.Age <= employeeParameters.MaxAge), trackChanges)
        //        .OrderBy(e => e.Name)
        //        .ToListAsync();
        //    return PagedList<Employee>.ToPagedList(employees, employeeParameters.PageNumber, employeeParameters.PageSize);
        //}
        public async Task<PagedList<Employee>> GetEmployeesAsync(Guid companyId, EmployeeParameters employeeParameters, bool trackChanges)
        {
            var employees = await FindByCondition(e => e.CompanyId.Equals(companyId), trackChanges)
                .FilterEmployees(employeeParameters.MinAge, employeeParameters.MaxAge)
                .Search(employeeParameters.SearchTerm)
                .Sort(employeeParameters.OrderBy)
                .ToListAsync();
            return PagedList<Employee>.ToPagedList(employees, employeeParameters.PageNumber, employeeParameters.PageSize); 
        }
    }
}
