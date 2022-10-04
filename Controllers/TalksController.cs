using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;

namespace CoreCodeCamp.Controllers {
    [ApiController]
    [Route("api/camps/{moniker}/talks")]
    public class TalksController : ControllerBase {
        private readonly ICampRepository _campRepository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;

        public TalksController(ICampRepository campRepository, IMapper mapper, LinkGenerator linkGenerator) {
            _campRepository = campRepository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
        }

        [HttpGet]
        public async Task<ActionResult<TalkModel[]>> Get(string moniker) {
            try {
                var talks = await _campRepository.GetTalksByMonikerAsync(moniker);

                return _mapper.Map<TalkModel[]>(talks);
            } catch (System.Exception) {

                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");

            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TalkModel>> Get(string moniker, int id) {
            try {
                var talk = await _campRepository.GetTalkByMonikerAsync(moniker, id);
                
                return _mapper.Map<TalkModel>(talk);

            } catch (System.Exception) {

                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
                
            }
        }
    }
}
