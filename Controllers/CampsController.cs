using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCodeCamp.Controllers {
    [Route("api/[controller]/")]
    [ApiController]
    public class CampsController : ControllerBase {
        private readonly ICampRepository _campRepository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;

        public CampsController(ICampRepository campRepository, IMapper mapper, LinkGenerator linkGenerator) {
            _campRepository = campRepository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
        }


        [HttpGet]
        public async Task<ActionResult<CampModel[]>> Get(bool includeTalks = false) {
            try {
                var results = await _campRepository.GetAllCampsAsync();

                return _mapper.Map<CampModel[]>(results);

            } catch (Exception) {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet("{moniker}")]
        public async Task<ActionResult<CampModel>> Get(string moniker) {
            try {
                var result = await _campRepository.GetCampAsync(moniker);

                if (result == null) return NotFound();

                return _mapper.Map<CampModel>(result);
            } catch (Exception) {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<CampModel[]>> SearchByDate(DateTime date, bool includeTalks = false) {

            try {
                var results = await _campRepository.GetAllCampsByEventDate(date, includeTalks);

                if (!results.Any()) return NotFound();

                return _mapper.Map<CampModel[]>(results);

            } catch (Exception) {

                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }


        public async Task<ActionResult<CampModel>> Post(CampModel model) {
            try {

                var existingCamp = await _campRepository.GetCampAsync(model.Moniker);
                if (existingCamp != null) {
                    return BadRequest("Moniker in use");
                }

                var location = _linkGenerator.GetPathByAction("Get", "Camps", new { moniker = model.Moniker });

                if (string.IsNullOrWhiteSpace(location))
                    return BadRequest("Could not use current moniker");

                var camp = _mapper.Map<Camp>(model);
                _campRepository.Add(existingCamp);

                if (await _campRepository.SaveChangesAsync()) {

                    return Created(location, _mapper.Map<CampModel>(existingCamp));
                }
            } catch (Exception) {

                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

            return BadRequest();
        }

        [HttpPut("{moniker}")]
        public async Task<ActionResult<CampModel>> Put(string moniker, CampModel model) {
            try {
                var oldCamp = await _campRepository.GetCampAsync(model.Moniker);
                if (oldCamp == null)
                    return NotFound($"Could not find camp with moniker of {moniker}");

                _mapper.Map(model, oldCamp);

                if (await _campRepository.SaveChangesAsync()) {
                    return _mapper.Map<CampModel>(oldCamp);
                }

            } catch (Exception) {

                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

            return BadRequest();
        }

        [HttpDelete("{moniker}")]

        public async Task<IActionResult> Delete(string moniker) {
            try {
                var oldCamp = await _campRepository.GetCampAsync(moniker);
                if (oldCamp == null)
                    return NotFound($"Could not find camp with moniker of {moniker}");

                _campRepository.Delete(oldCamp);

                if (await _campRepository.SaveChangesAsync()) {
                    return Ok();
                }


            } catch (Exception) {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");

            }

            return BadRequest();

        }
    }
}
