using AutoMapper;
using CompanyEmployees.ActionFilters;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Entities.RequestFeatures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;

namespace CompanyEmployees.Controllers
{
    [Route("api/companies/{companyId}/employees")]
    [ApiController]
    public class EmployeesController : ControllerBase
    { 
        private readonly IRepositoryManager _repository; 
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper; 
        public EmployeesController(IRepositoryManager repository, ILoggerManager logger, IMapper mapper)
        {
            _repository = repository; 
            _logger = logger;
            _mapper = mapper;
        }
        //[HttpGet] 
        //public async Task< IActionResult> GetEmployeesForCompany(Guid companyId)
        //{
        //    var company = await _repository.Company.GetCompanyAsync(companyId, trackChanges: false);
        //    if (company == null)
        //    { 
        //        _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
        //        return NotFound();
        //    } 
        //    var employeesFromDb = _repository.Employee.GetEmployees(companyId, trackChanges: false);
        //    var employeesDto = _mapper.Map<IEnumerable<EmployeeDto>>(employeesFromDb);
        //    return Ok(employeesDto);
        //}


        [HttpGet]
        public async Task<IActionResult> GetEmployeesForCompany(Guid companyId, [FromQuery] EmployeeParameters employeeParameters)
        {
            if (!employeeParameters.ValidAgeRange)
                return BadRequest("Max age can't be less than min age.");
            var company = await _repository.Company.GetCompanyAsync(companyId, trackChanges: false); 
            if (company == null)
             {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database."); 
                return NotFound();
            }
            var employeesFromDb = await _repository.Employee.GetEmployeesAsync(companyId, employeeParameters, trackChanges: false);
            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(employeesFromDb.MetaData));
            var employeesDto = _mapper.Map<IEnumerable<EmployeeDto>>(employeesFromDb); 
            return Ok(employeesDto);
        }
        [HttpGet("{id}", Name = "GetEmployeeForCompany")]
        public async Task< IActionResult> GetEmployeeForCompany(Guid companyId, Guid id)
        { 
            var company = await _repository.Company.GetCompanyAsync(companyId, trackChanges: false);
            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database."); 
                return NotFound(); 
            } 
            var employeeDb = _repository.Employee.GetEmployee(companyId, id, trackChanges: false); 
            if (employeeDb == null)
            { 
                _logger.LogInfo($"Employee with id: {id} doesn't exist in the database.");
                return NotFound();
            } 
            var employee = _mapper.Map<EmployeeDto>(employeeDb); return Ok(employee); 
        }
        [HttpPost]
        public async Task<IActionResult> CreateEmployeeForCompany(Guid companyId, [FromBody] EmployeeForCreationDto employee)
        {
            if (employee == null)
            {
                _logger.LogError("EmployeeForCreationDto object sent from client is null."); 
                return BadRequest("EmployeeForCreationDto object is null");
            }
            if (!ModelState.IsValid) 
            {
               // ModelState.AddModelError(string key, string errorMessage)
                _logger.LogError("Invalid model state for the EmployeeForCreationDto object");
                return UnprocessableEntity(ModelState);
            }
            var company =await _repository.Company.GetCompanyAsync(companyId, trackChanges: false);
            if (company == null) 
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
                return NotFound();
            }
            var employeeEntity = _mapper.Map<Employee>(employee); 
            _repository.Employee.CreateEmployeeForCompany(companyId, employeeEntity);
            _repository.SaveAsync();
            var employeeToReturn = _mapper.Map<EmployeeDto>(employeeEntity);
            return CreatedAtRoute("GetEmployeeForCompany", new { companyId, id = employeeToReturn.Id }, employeeToReturn);
        }

        [HttpDelete("{id}")]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> DeleteEmployeeForCompany(Guid companyId, Guid id) 
        {
            var company =await _repository.Company.GetCompanyAsync(companyId, trackChanges: false);
            if (company == null)
            { 
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
                return NotFound();
            }
            var employeeForCompany = HttpContext.Items["employee"] as Employee;
           // var employeeForCompany = _repository.Employee.GetEmployee(companyId, id, trackChanges: false);
            if (employeeForCompany == null)
            { 
                _logger.LogInfo($"Employee with id: {id} doesn't exist in the database."); 
                return NotFound();
            }
            _repository.Employee.DeleteEmployee(employeeForCompany);
            _repository.SaveAsync(); 
            return NoContent();
        }

        [HttpPut("{id}")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public async Task< IActionResult> UpdateEmployeeForCompany(Guid companyId, Guid id, [FromBody] EmployeeForUpdateDto employee)
        { 
            if (employee == null) 
            {
                _logger.LogError("EmployeeForUpdateDto object sent from client is null.");
                return BadRequest("EmployeeForUpdateDto object is null");
            }
            var company = await _repository.Company.GetCompanyAsync(companyId, trackChanges: false);
            if (company == null)
            { 
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database."); 
                return NotFound();
            }
            if (!ModelState.IsValid)
            { 
                _logger.LogError("Invalid model state for the EmployeeForUpdateDto object"); 
                return UnprocessableEntity(ModelState);
            }
            var employeeEntity = HttpContext.Items["employee"] as Employee;
          //  var employeeEntity = _repository.Employee.GetEmployee(companyId, id, trackChanges: true);
            if (employeeEntity == null) 
            {
                _logger.LogInfo($"Employee with id: {id} doesn't exist in the database."); 
                return NotFound(); 
            } 
            _mapper.Map(employee, employeeEntity); 
            _repository.SaveAsync(); 
            return NoContent(); 
        }

    }
}
