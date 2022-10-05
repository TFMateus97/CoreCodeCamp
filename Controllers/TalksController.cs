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
                var talks = await _campRepository.GetTalksByMonikerAsync(moniker, true);

                return _mapper.Map<TalkModel[]>(talks);
            } catch (System.Exception) {

                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");

            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TalkModel>> Get(string moniker, int id) {
            try {
                var talk = await _campRepository.GetTalkByMonikerAsync(moniker, id, true);
                if (talk == null) return NotFound("Could not find talk");
                return _mapper.Map<TalkModel>(talk);

            } catch (System.Exception) {

                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");

            }
        }

        [HttpPost]
        public async Task<ActionResult<TalkModel>> Post(string moniker, TalkModel model) {
            try {
                var camp = await _campRepository.GetCampAsync(moniker, true);

                if (camp == null) return BadRequest("Camp does not exist");

                var talk = _mapper.Map<Talk>(model);
                talk.Camp = camp;

                if (model.Speaker == null)
                    return BadRequest("Speaker is required");

                var speaker = await _campRepository.GetSpeakerAsync(model.Speaker.Id);

                if (speaker == null)
                    return BadRequest("Speaker could not be found");

                talk.Speaker = speaker;

                _campRepository.Add(talk);

                if (await _campRepository.SaveChangesAsync()) {
                    var url = _linkGenerator.GetPathByAction(HttpContext, "Get", values: new { moniker, id = talk.TalkId });

                    return Created(url, _mapper.Map<TalkModel>(talk));
                } else {
                    return BadRequest("Failed to save new Talk");
                }

            } catch (System.Exception) {

                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<TalkModel>> Put(string moniker, int id, TalkModel model) {

            try {

                var talk = await _campRepository.GetTalkByMonikerAsync(moniker, id, true);
                if (talk == null) return NotFound("Couldn't find the talk");

                _mapper.Map(model, talk);

                if (model.Speaker != null) {
                    var speaker = await _campRepository.GetSpeakerAsync(model.Speaker.Id);
                    if (speaker != null) {
                        talk.Speaker = speaker;
                    }
                }

                if (await _campRepository.SaveChangesAsync()) {
                    return _mapper.Map<TalkModel>(talk);
                } else {
                    return BadRequest();
                }

            } catch (System.Exception) {

                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(string moniker, int id) {
            try {
                var talk = await _campRepository.GetTalkByMonikerAsync(moniker, id);
                if (talk == null) return NotFound("Failed to find the talk to delete");

                _campRepository.Delete(talk);

                if (await _campRepository.SaveChangesAsync()) {
                    return Ok();
                } else {
                    return BadRequest("Failed to delete talk");
                }

            } catch (System.Exception) {

                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }
    }
}
