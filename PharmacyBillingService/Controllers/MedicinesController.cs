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
    public class MedicinesController : ControllerBase
    {
        private readonly IMedicineService _medicineService;

        public MedicinesController(IMedicineService medicineService)
        {
            _medicineService = medicineService;
        }

        /// <summary>
        /// Retrieves all medicines, optionally filtered by keyword (name or active ingredient).
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<MedicineDto>))]
        public async Task<IActionResult> GetAll([FromQuery] string? search)
        {
            var result = await _medicineService.GetAllAsync(search);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves detailed information of a medicine by its unique ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MedicineDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _medicineService.GetByIdAsync(id);
            if (result == null)
            {
                return NotFound(new { message = "Không tìm thấy thông tin thuốc." });
            }
            return Ok(result);
        }

        /// <summary>
        /// Adds a new medicine along with its initial stock configuration (Admin only).
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(MedicineDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateMedicineDto request)
        {
            var result = await _medicineService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// <summary>
        /// Updates an existing medicine catalog record (Admin only).
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MedicineDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMedicineDto request)
        {
            var result = await _medicineService.UpdateAsync(id, request);
            if (result == null)
            {
                return NotFound(new { message = "Không tìm thấy thuốc để cập nhật." });
            }
            return Ok(result);
        }

        /// <summary>
        /// Soft deletes/deactivates a medicine from active use (Admin only).
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _medicineService.DeleteAsync(id);
            if (!success)
            {
                return NotFound(new { message = "Không tìm thấy thuốc cần xóa." });
            }
            return Ok(new { message = "Xóa (vô hiệu hóa) thuốc thành công." });
        }

        /// <summary>
        /// Replenishes the stock of a medicine (Admin or Receptionist/Nurse).
        /// </summary>
        [HttpPost("{id}/stock")]
        [Authorize(Roles = "Admin,Receptionist")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ReplenishStock(Guid id, [FromBody] StockUpdateDto request)
        {
            var success = await _medicineService.ReplenishStockAsync(id, request.Quantity);
            if (!success)
            {
                return NotFound(new { message = "Không tìm thấy thông tin tồn kho thuốc cần nhập." });
            }
            return Ok(new { message = $"Nhập kho thêm {request.Quantity} đơn vị thuốc thành công." });
        }
    }
}
