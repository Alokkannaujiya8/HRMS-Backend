using System;
using System.Threading.Tasks;
using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [Authorize(Roles = AppRoles.AdminOrHr)]
    [Route("api/assets")]
    public class AssetController : ApiControllerBase
    {
        private readonly IAssetService _assetService;

        public AssetController(IAssetService assetService)
        {
            _assetService = assetService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var assets = await _assetService.GetAllAssetsAsync();
            return Ok(assets);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var asset = await _assetService.GetAssetByIdAsync(id);
            if (asset == null)
            {
                return NotFound(new { Message = "Asset not found." });
            }
            return Ok(asset);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AssetCreateDto dto)
        {
            try
            {
                var asset = await _assetService.CreateAssetAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = asset.Id }, asset);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] AssetUpdateDto dto)
        {
            try
            {
                var asset = await _assetService.UpdateAssetAsync(id, dto);
                if (asset == null)
                {
                    return NotFound(new { Message = "Asset not found." });
                }
                return Ok(asset);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _assetService.DeleteAssetAsync(id);
                if (!deleted)
                {
                    return NotFound(new { Message = "Asset not found." });
                }
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("{id}/assign")]
        public async Task<IActionResult> Assign(int id, [FromBody] AssetAssignDto dto)
        {
            try
            {
                var result = await _assetService.AssignAssetAsync(id, dto);
                if (!result)
                {
                    return NotFound(new { Message = "Asset not found." });
                }
                return Ok(new { Message = "Asset assigned successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("{id}/return")]
        public async Task<IActionResult> Return(int id, [FromBody] AssetReturnDto dto)
        {
            try
            {
                var result = await _assetService.ReturnAssetAsync(id, dto);
                if (!result)
                {
                    return NotFound(new { Message = "Asset not found." });
                }
                return Ok(new { Message = "Asset returned successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("employee/{employeeId}")]
        public async Task<IActionResult> GetEmployeeAssets(int employeeId)
        {
            var assignments = await _assetService.GetEmployeeAssetsAsync(employeeId);
            return Ok(assignments);
        }
    }
}
