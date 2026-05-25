using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PharmacyBillingService.DTOs;
using PharmacyBillingService.Services;

namespace PharmacyBillingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PrescriptionsController : ControllerBase
    {
        private readonly IPrescriptionService _prescriptionService;

        public PrescriptionsController(IPrescriptionService prescriptionService)
        {
            _prescriptionService = prescriptionService;
        }

        /// <summary>
        /// Retrieves all prescriptions local copies, optionally filtered by status.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PrescriptionDto>))]
        public async Task<IActionResult> GetAll([FromQuery] string? status)
        {
            var result = await _prescriptionService.GetAllAsync(status);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a local prescription details along with its individual items by ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PrescriptionDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _prescriptionService.GetByIdAsync(id);
            if (result == null)
            {
                return NotFound(new { message = "Không tìm thấy thông tin đơn thuốc." });
            }
            return Ok(result);
        }

        /// <summary>
        /// Simulates receiving the 'prescription.created' event published by the Medical Record service.
        /// </summary>
        [HttpPost("simulate-event")]
        [AllowAnonymous] // Allow simulation without strict login, or keep standard authorization
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PrescriptionDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SimulateEvent([FromBody] PrescriptionCreatedEventDto eventDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _prescriptionService.ProcessPrescriptionEventAsync(eventDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi xử lý sự kiện kê đơn: {ex.Message}" });
            }
        }
    }
}
