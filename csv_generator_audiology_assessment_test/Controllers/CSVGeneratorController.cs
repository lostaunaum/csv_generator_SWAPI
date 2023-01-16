using csv_generator_audiology_assessment_test.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;

namespace csv_generator_audiology_assessment_test.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CSVGeneratorController : ControllerBase
    {
        private static Iswapi_DataService _swapiDataService;

        public CSVGeneratorController(Iswapi_DataService swapi_DataService)
        {
            _swapiDataService = swapi_DataService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await _swapiDataService.GetAllCharactersByEvenNumberedFilms();

            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            await System.IO.File.WriteAllTextAsync(path + "\\MyCSVREsults.csv", result);

            return Ok(result);
        }
    }
}
