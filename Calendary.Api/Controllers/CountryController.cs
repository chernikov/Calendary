using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Calendary.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CountryController(ICountryService countryService, IMapper mapper) : Controller
    {
        
     

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var countries = await countryService.GetAllCountriesAsync();
            var result = mapper.Map<IEnumerable<CountryDto>>(countries);
            return Ok(result);
        }
    }
}
