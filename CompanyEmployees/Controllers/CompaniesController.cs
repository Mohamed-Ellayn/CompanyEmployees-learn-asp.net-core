using AutoMapper;
using CompanyEmployees.ActionFilters;
using CompanyEmployees.ModelBinders;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CompanyEmployees.Controllers
{
    [ApiVersion("1.0")]
    //[ResponseCache(CacheProfileName = "120SecondsDuration")]
    [Route("api/companies")]
    [ApiController]
    public class CompaniesController : ControllerBase 
    {
        private readonly IRepositoryManager _repository; 
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        public CompaniesController(IRepositoryManager repository, ILoggerManager logger, IMapper mapper) 
        { 
            _repository = repository; 
            _logger = logger;
            _mapper = mapper;
        }
        [HttpGet(Name = "GetCompanies"), Authorize]
        //  [ResponseCache(Duration = 60)]
        public async Task< IActionResult> GetCompanies()
        {
            try
            {
                var companies = await _repository.Company.GetAllCompaniesAsync(trackChanges: false);
                var companiesDto = _mapper.Map<IEnumerable<CompanyDto>>(companies);
                return Ok(companiesDto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong in the {nameof(GetCompanies)} action {ex}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}", Name = "CompanyById"), Authorize]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = 60)]
        [HttpCacheValidation(MustRevalidate = false)]
        // [HttpGet("{id}")]
        public async Task<IActionResult> GetCompany(Guid id)
        { 
            var company = await _repository.Company.GetCompanyAsync(id, trackChanges: false); 
            if (company == null)
            {
                _logger.LogInfo($"Company with id: {id} doesn't exist in the database.");
                return NotFound(); 
            } 
            else
            {
                var companyDto = _mapper.Map<CompanyDto>(company); 
                return Ok(companyDto);
            }
        }
        [HttpPost]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public IActionResult CreateCompany([FromBody] CompanyForCreationDto company)
        {
            if (company == null)
            {
                _logger.LogError("CompanyForCreationDto object sent from client is null.");
                return BadRequest("CompanyForCreationDto object is null");
            }
            var companyEntity = _mapper.Map<Company>(company); 
            _repository.Company.CreateCompany(companyEntity); 
            _repository.SaveAsync();
            var companyToReturn = _mapper.Map<CompanyDto>(companyEntity);
            return CreatedAtRoute("CompanyById", new { id = companyToReturn.Id }, companyToReturn);
        }
        [HttpGet("collection/({ids})", Name = "CompanyCollection")]
        public async Task<IActionResult> GetCompanyCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
        {
            if (ids == null)
            {
                _logger.LogError("Parameter ids is null");
                return BadRequest("Parameter ids is null"); 
            }
            var companyEntities = await _repository.Company.GetByIdsAsync(ids, trackChanges: false);
            if (ids.Count() != companyEntities.Count()) 
            { 
                _logger.LogError("Some ids are not valid in a collection"); 
                return NotFound();
            }
            var companiesToReturn = _mapper.Map<IEnumerable<CompanyDto>>(companyEntities); 
            return Ok(companiesToReturn);
        }
        [HttpPost("collection")] 
        public IActionResult CreateCompanyCollection([FromBody] IEnumerable<CompanyForCreationDto> companyCollection)
        { 
            if (companyCollection == null) 
            { 
                _logger.LogError("Company collection sent from client is null.");
                return BadRequest("Company collection is null");
            } 
            var companyEntities = _mapper.Map<IEnumerable<Company>>(companyCollection);
            foreach (var company in companyEntities)
            { 
                _repository.Company.CreateCompany(company); 
            }
            _repository.SaveAsync(); 
            var companyCollectionToReturn = _mapper.Map<IEnumerable<CompanyDto>>(companyEntities);
            var ids = string.Join(",", companyCollectionToReturn.Select(c => c.Id)); 
            return CreatedAtRoute("CompanyCollection", new { ids }, companyCollectionToReturn);
        }
        [HttpDelete("{id}")]
        [ServiceFilter(typeof(ValidateCompanyExistsAttribute))] //get company and save to http context 
        public async Task< IActionResult> DeleteCompany(Guid id)
        {
            var company = HttpContext.Items["company"] as Company;
           // var company =await _repository.Company.GetCompanyAsync(id, trackChanges: false);
            if (company == null)
            {
                _logger.LogInfo($"Company with id: {id} doesn't exist in the database.");
                return NotFound();
            }
            _repository.Company.DeleteCompany(company);
            _repository.SaveAsync();
            return NoContent(); 
        }

        [HttpPut("{id}")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        [ServiceFilter(typeof(ValidateCompanyExistsAttribute))]
        public async Task< IActionResult> UpdateCompany(Guid id, [FromBody] CompanyForUpdateDto company) 
        {
            if (company == null)
            {
                _logger.LogError("CompanyForUpdateDto object sent from client is null.");
                return BadRequest("CompanyForUpdateDto object is null");
            }
          //  var companyEntity =await _repository.Company.GetCompanyAsync(id, trackChanges: true);
            var companyEntity = HttpContext.Items["company"] as Company;

            if (companyEntity == null) 
            {
                _logger.LogInfo($"Company with id: {id} doesn't exist in the database."); 
                return NotFound(); 
            } 
            _mapper.Map(company, companyEntity); 
            await  _repository.SaveAsync();
            return NoContent();
        }
    }
}
