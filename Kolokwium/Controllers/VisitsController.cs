using Kolokwium.Models.DTOs;
using Kolokwium.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Kolokwium.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VisitsController : ControllerBase
    {
        private readonly IVisitsService _visitsService;

        public VisitsController(IVisitsService visitsService)
        {
            _visitsService = visitsService;
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVisit(int id)
        {
            var result = await _visitsService.GetVisit(id);
            if(result is null)
            {
                return NotFound();
            }
            return Ok(result);
        }


        [HttpPost]
        public async Task<IActionResult> AddVisit([FromBody] AddVisitDTO addVisitDto)
        {
            if (addVisitDto == null)
            {
                return BadRequest();
            }

            if (addVisitDto.Services is null || addVisitDto.Services.Count < 1)
            {
                return BadRequest();
            }

            try
            {
                await _visitsService.AddVist(addVisitDto);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            return Created("", addVisitDto);
        }
    }
}
