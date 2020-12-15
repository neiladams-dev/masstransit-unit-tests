using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Sample.Domain.Services;
using Sample.Domain.Entities;

namespace Sample.Api.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Route("things")]
    public class ThingsController : ControllerBase

    {
        private readonly IThingService _thingService;
        private readonly ILogger<ThingsController> _logger;

        public ThingsController(ILogger<ThingsController> logger, IThingService thingService)
        {
            _logger = logger;
            _thingService = thingService;
        }

        /// <summary>
        /// Handles posted Thing
        /// </summary>
        /// <param name="resource">Resource instance</param>
        [HttpGet("{thingId}", Name = "GetThingAsync")]
        public async Task<IActionResult> GetThingAsync([FromRoute][Required] int thingId)
        {
            var thing = new Thing
            {
                Id = thingId,
                Name = $"Test thing {thingId}"
            };

            return Ok(thing);
        }

        /// <summary>
        /// Handles posted Thing
        /// </summary>
        /// <param name="resource">Resource instance</param>
        [HttpGet("", Name = "GetThingsAsync")]
        public async Task<IActionResult> GetThingsAsync()

        {
            var things = new List<Thing>
            {
                new Thing
                {
                    Id = 1,
                    Name = "thing one"
                },
                new Thing
                {
                    Id = 2,
                    Name = "thing two"
                }
            };

            return Ok(things);
        }


        /// <summary>
        /// Handles posted Thing
        /// </summary>
        /// <param name="resource">Resource instance</param>
        [HttpPost("", Name = "CreateThingAsync")]
        public async Task<IActionResult> CreateThingAsync([FromBody][Required] Domain.Entities.Thing thing)
        {
            _logger.LogDebug("Start Post to /things");

            await _thingService.SendCreateThingCommandAsync(thing);

            _logger.LogDebug("Queued command to create thing: {thingName}", thing.Name);

            return Accepted();
        }
    }
}
