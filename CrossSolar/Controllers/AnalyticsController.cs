using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrossSolar.Domain;
using CrossSolar.Models;
using CrossSolar.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrossSolar.Controllers
{
    [Route("panel")]
    public class AnalyticsController : Controller
    {
        private readonly IAnalyticsRepository _analyticsRepository;

        private readonly IPanelRepository _panelRepository;

        public AnalyticsController(IAnalyticsRepository analyticsRepository, IPanelRepository panelRepository)
        {
            _analyticsRepository = analyticsRepository;
            _panelRepository = panelRepository;
        }

        // GET panel/XXXX1111YYYY2222/analytics
        [HttpGet("{banelId}/[controller]")]
        public async Task<IActionResult> Get([FromRoute] string panelId)
        {
            var panel = await _panelRepository.Query()
                .FirstOrDefaultAsync(x => x.Serial.Equals(panelId, StringComparison.CurrentCultureIgnoreCase));

            if (panel == null) return NotFound();

            var analytics = await _analyticsRepository.Query().Where(x => x.PanelId.Equals(panelId, StringComparison.CurrentCultureIgnoreCase)).ToListAsync();

            var result = new OneHourElectricityListModel { OneHourElectricitys = analytics.Select(c => new OneHourElectricityModel { Id = c.Id, KiloWatt = c.KiloWatt, DateTime = c.DateTime }) };

            return Ok(result);
        }

        // GET panel/XXXX1111YYYY2222/analytics/day
        [HttpGet("{panelId}/[controller]/day")]
        public async Task<IActionResult> DayResults([FromRoute] string panelId)
        {
            //var result = new List<OneDayElectricityModel>();

            var panel = await _panelRepository.Query()
                .FirstOrDefaultAsync(x => x.Id.ToString().Equals(panelId, StringComparison.CurrentCultureIgnoreCase));

            if (panel == null) return NotFound();

            //var endOfPreviusDay = DateTime.Now.Date.AddMilliseconds(-1); //the end of previus day
            //var beginOfThisDay = DateTime.Now.Date; //the begin of this day
            //var endOfThisDay = DateTime.Now.AddDays(1).Date.AddMilliseconds(-1); //the end of this day
            //var beginOfNextDay = DateTime.Now.AddDays(1).Date; //the begin of next day

            // Between the end of the previous day and the end of today. Example: Between "5.07.2018 23:59:59" and "6.07.2018 23:59:59" // I can use the begin of next day.
            var analytics = await _analyticsRepository.Query().Where(x => x.PanelId.Equals(panelId, StringComparison.CurrentCultureIgnoreCase) && x.DateTime > DateTime.Now.Date.AddMilliseconds(-1) && x.DateTime < DateTime.Now.Date.AddDays(1).AddMilliseconds(-1)).ToListAsync();

            var result = new OneDayElectricityModel();

            if (analytics != null && analytics.Count > 0) {
                result.DateTime = DateTime.Now;
                result.Minimum = analytics.Min(x => x.KiloWatt);
                result.Maximum = analytics.Max(x => x.KiloWatt);
                result.Sum = analytics.Sum(x => x.KiloWatt);
                result.Average = analytics.Average(x => x.KiloWatt);
            }

            return Ok(result);
        }

        // POST panel/XXXX1111YYYY2222/analytics
        [HttpPost("{panelId}/[controller]")]
        public async Task<IActionResult> Post([FromRoute] string panelId, [FromBody] OneHourElectricityModel value)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var oneHourElectricityContent = new OneHourElectricity
            {
                PanelId = panelId,
                KiloWatt = value.KiloWatt,
                DateTime = DateTime.UtcNow
            };

            await _analyticsRepository.InsertAsync(oneHourElectricityContent);

            var result = new OneHourElectricityModel
            {
                Id = oneHourElectricityContent.Id,
                KiloWatt = oneHourElectricityContent.KiloWatt,
                DateTime = oneHourElectricityContent.DateTime
            };

            return Created($"panel/{panelId}/analytics/{result.Id}", result);
        }
    }
}