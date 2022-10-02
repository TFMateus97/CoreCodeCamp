using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCodeCamp.Controllers {
    [Route("api/[controller/")]
    public class CampsController : ControllerBase {
        private readonly ICampRepository _campRepository;
        private readonly IMapper _mapper;

        public CampsController(ICampRepository campRepository, IMapper mapper) {
            this._campRepository = campRepository;
            this._mapper = mapper;
        }


        [HttpGet]
        public async Task<ActionResult<CampModel[]>> Get() {
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
    }
}
