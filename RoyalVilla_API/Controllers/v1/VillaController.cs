using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoyalVilla_API.Models;
using RoyalVilla.DTO;
using System.Collections;
using System.Data.SqlTypes;
using RoyalVilla_API.Data;
using Asp.Versioning;

namespace RoyalVilla_API.Controllers.v1
{
    [ApiController]
    //[ApiExplorerSettings(GroupName = "v1")]

    [Route("api/v{version:apiVersion}/Villa")]
    [ApiVersion("1.0")]// if multiple we can have here get post...Not two gets...
    public class VillaController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        public VillaController(ApplicationDbContext dbContext, IMapper mapper)
        {
            _db = dbContext;
            _mapper = mapper;
        }

        [HttpGet]
        [AllowAnonymous]
        //[Route("/GetVillas")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<VillaDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<VillaDTO>>> getVilla()
        {
            var villa = await _db.Villa.ToListAsync();

            var dtoResponseVilla = _mapper.Map<List<VillaDTO>>(villa);

            var response = ApiResponse<IEnumerable<VillaDTO>>.Ok(dtoResponseVilla, "Retrived the data");
            return Ok(response);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Customer")]
        [ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<VillaDTO>>> getVillaById(int id)
        {
            try
            {
                if (id < 0)
                {
                    return BadRequest(ApiResponse<VillaDTO>.BadRequest("Bad Request", "Enter the Id greter than 0"));
                    
                    //return new ApiResponse<VillaDTO>
                    //{
                    //    Success = false,
                    //    StatusCode = 400,
                    //    Message = "Bad Request",
                    //    Errors = "Enter a Id greater than O",
                    //    Data = null //or _mapper.Map<VillaDTO>(Villa) for success
                    //};
                    //return BadRequest("Enter a Id greater than O");
                }

                var Villa = await _db.Villa.FirstOrDefaultAsync(x => x.Id == id);

                if (id == null)
                {
                    return NotFound(ApiResponse<object>.NotFound( $"Villa with ID {id} is not found"));
                }
                var response = ApiResponse<VillaDTO>.Ok(_mapper.Map<VillaDTO>(Villa), "Retrived successfully");

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, $"An error occured while retriving the id {id}",ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        //[HttpGet("{id:int}/{name}")]
        //public string getVillaByAndName([FromQuery]int id, [FromQuery]string name)
        //{
        //    return "Got Villa " + id + " " + name;
        //} 

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status502BadGateway)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<VillaDTO>>> CreateVilla(VillaCreateDTO villaDTO)
        {
            try
            {
                if (villaDTO == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Villa data is required"));
                }

                var duplicateVilla = await _db.Villa.FirstOrDefaultAsync(u => u.Name.ToLower() == villaDTO.Name.ToLower());

                if (duplicateVilla != null)
                {
                    return Conflict(ApiResponse<object>.Conflict($"Villa with name {villaDTO.Name} is already exist"));
                    //return Conflict($"Villa with name {villaDTO.Name} is already exist");
                }

                //Villa villa = new Villa
                //{
                //    Name = villaDTO.Name,
                //    Details = villaDTO.Details,
                //    Rate = villaDTO.Rate,
                //    Sqft = villaDTO.Sqft,
                //    Occupancy = villaDTO.Occupancy,
                //    CreatedDate = DateTime.Now,

                //}; instead if this

                Villa villa = _mapper.Map<Villa>(villaDTO);

                await _db.Villa.AddAsync(villa);

                await _db.SaveChangesAsync();

                var response = ApiResponse<VillaDTO>.CreatedAt(_mapper.Map<VillaDTO>(villa), "Villa created successfully");
                return CreatedAtAction(nameof(CreateVilla), new { id = villa.Id }, response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, $"An error occured while creating a Villa", ex.Message);
                return StatusCode(500, errorResponse);
                //return StatusCode(StatusCodes.Status500InternalServerError, $"An error occured while creating a Villa : {ex.Message}");
            }
        }

        [HttpPut("{id:int}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status502BadGateway)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<VillaDTO>>> UpdateVilla(int id, VillaUpdateDTO villaUpdateDTO)
        {
            try
            {
                if (villaUpdateDTO == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Villa data is required"));
                }

                if (id != villaUpdateDTO.Id)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Villa Id in the URL does not match the villa ID in request body"));
                }

                var existingVilla = await _db.Villa.FirstOrDefaultAsync(x => x.Id == id);

                if (existingVilla == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Villa with ID {id} is not found"));
                }

                var duplicateVilla = await _db.Villa.FirstOrDefaultAsync(u => u.Name.ToLower() == villaUpdateDTO.Name.ToLower() && u.Id != villaUpdateDTO.Id);

                if (duplicateVilla != null)
                {
                    return Conflict(ApiResponse<object>.Conflict($"Villa with name {villaUpdateDTO.Name} is already exist"));
                }

                _mapper.Map(villaUpdateDTO, existingVilla); // take from villaDTO request to put in existingvilla
                existingVilla.UpdatedDate = DateTime.Now;

                await _db.SaveChangesAsync();

                var response = ApiResponse<VillaDTO>.Ok(_mapper.Map<VillaDTO>(existingVilla), "Villa updated successfully");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, $"An error occured while updating a Villa", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status204NoContent)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteVilla(int id)
        {
            try
            {
                var existingVilla = await _db.Villa.FirstOrDefaultAsync(x => x.Id == id);

                if (existingVilla == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Villa was not exist"));
                }

                _db.Villa.Remove(existingVilla);

                await _db.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, $"An error occured while deleting a Villa", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }
    }
}
