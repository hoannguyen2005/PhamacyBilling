using System;
using System.Collections.Generic;
using System.Security.Claims;
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
    public class BillsController : ControllerBase
    {
        private readonly IBillService _billService;

        public BillsController(IBillService billService)
        {
            _billService = billService;
        }

        /// <summary>
        /// Retrieves all bills, optionally filtered by status (Pending, Paid, Cancelled).
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<BillDto>))]
        public async Task<IActionResult> GetAll([FromQuery] string? status)
        {
            var result = await _billService.GetAllAsync(status);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a detailed invoice by its ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BillDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _billService.GetByIdAsync(id);
            if (result == null)
            {
                return NotFound(new { message = "Không tìm thấy thông tin hóa đơn." });
            }
            return Ok(result);
        }

        /// <summary>
        /// Creates a manual bill for custom clinical procedures/fees (Receptionist or Admin only).
        /// </summary>
        [HttpPost("create-manual")]
        [Authorize(Roles = "Admin,Receptionist")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(BillDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateManual([FromBody] ManualBillRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _billService.CreateManualBillAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// <summary>
        /// Finalizes payment for an invoice, updates stock status, and logs transaction (Receptionist or Admin only).
        /// </summary>
        [HttpPost("{id}/pay")]
        [Authorize(Roles = "Admin,Receptionist")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Pay(Guid id, [FromBody] PaymentRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid receivedById))
            {
                return Unauthorized(new { message = "Không thể lấy thông tin nhân viên thực hiện giao dịch." });
            }

            var response = await _billService.PayBillAsync(id, request, receivedById);
            if (!response.Success)
            {
                return BadRequest(new { message = response.Message });
            }

            return Ok(response);
        }
    }
}
