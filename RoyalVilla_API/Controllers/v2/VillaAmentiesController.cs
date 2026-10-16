using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoyalVilla.DTO;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using System.Data.SqlTypes;

namespace RoyalVilla_API.Controllers.v2
{
    //[Route("api/v1/Villa-amenities")]
    [Route("api/v{version:apiVersion}/Villa-amenities")]
    [ApiVersion("2.0")]
    [ApiController]
    public class VillaAmentiesController : ControllerBase
    {
        public readonly ApplicationDbContext _db;
        public readonly IMapper _mapper;
        public VillaAmentiesController(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<VillaAmentiesDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<VillaAmentiesDTO>>> GetVillaAmenities() 
        {
            var VillaAmenities = await _db.VillaAmenities.Include(x=>x.villa).ToListAsync();

            var response = _mapper.Map<List<VillaAmentiesDTO>>(VillaAmenities);

            return Ok(ApiResponse<IEnumerable<VillaAmentiesDTO>>.Ok(response, "Villa Amenities retrived successfully")); 
            
        }

        [HttpGet("{id:int}")]
        //[AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<VillaAmentiesDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<VillaAmentiesDTO>>> GetVillaAmenitiesById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return NotFound(ApiResponse<object>.NotFound("VillaAmenities ID must be greater than 0"));
                }

                var villaAmenities = await _db.VillaAmenities.Include(x => x.villa).FirstOrDefaultAsync(u => u.Id == id);
                if (villaAmenities == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"VillaAmenities with ID {id} was not found"));
                }
                return Ok(ApiResponse<VillaAmentiesDTO>.Ok(_mapper.Map<VillaAmentiesDTO>(villaAmenities), "Records retrieved successfully"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, "An error occurred while creating the villa:", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }


        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<VillaAmentiesDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<VillaAmentiesDTO>> CreateAmenities(VillaAmentiesCreateDTO villaCreateAmenities)
        {
            try
            {
                if (villaCreateAmenities == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Villa Amenities data is required"));
                }

                var villaExists = await _db.Villa.FirstOrDefaultAsync(u => u.Id == villaCreateAmenities.VillaId);

                if (villaExists == null)
                {
                    return Conflict(ApiResponse<object>.Conflict($"Villa with the ID '{villaCreateAmenities.VillaId}'does not exist."));
                }

                VillaAmenities villaAmenities = _mapper.Map<VillaAmenities>(villaCreateAmenities);
                await _db.VillaAmenities.AddAsync(villaAmenities);
                await _db.SaveChangesAsync();

                var response = ApiResponse<VillaAmentiesDTO>.CreatedAt(_mapper.Map<VillaAmentiesDTO>(villaAmenities), "VillaAmenities created successfully");

                return CreatedAtAction(nameof(CreateAmenities), new { id = villaAmenities.Id }, response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, $"An error occured while creating a Villa Amenities", ex.Message);
                return StatusCode(500, errorResponse);
            }

        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<VillaAmentiesDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<VillaAmentiesDTO>> UpdateAmenities(int id, VillaAmentiesUpdateDTO villaUpdateAmenities)
        {
            try
            {
                if (villaUpdateAmenities == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Villa Amenities data is required"));
                }

                if (id != villaUpdateAmenities.Id)
                {
                    return NotFound(ApiResponse<object>.BadRequest($"Villa Amenities ID in URL does not match VillaAmenities ID in request body"));
                }

                var villaExists = await _db.Villa.FirstOrDefaultAsync(u => u.Id == villaUpdateAmenities.VillaId);

                if (villaExists == null)
                {
                    return Conflict(ApiResponse<object>.Conflict($"Villa with the ID '{villaUpdateAmenities.VillaId}'does not exist."));
                }

                var existingVillaAmenities = await _db.VillaAmenities.FirstOrDefaultAsync(x => x.Id == id);

                if (existingVillaAmenities == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Villa with ID {id} is not found"));
                }

                _mapper.Map(villaUpdateAmenities, existingVillaAmenities); // take from villaUpdateAmenities request to put in existingVillaAmenities
                existingVillaAmenities.UpdatedDate = DateTime.Now;
                await _db.SaveChangesAsync();

                var response = ApiResponse<VillaAmentiesDTO>.Ok(_mapper.Map<VillaAmentiesDTO>(existingVillaAmenities), "VillaAmenities updated successfully");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, $"An error occured while updating a VillaAmenities", ex.Message);
                return StatusCode(500, errorResponse);
            }

        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteVillaAmenities(int id)
        {
            try
            {
                var existingVillaAmenities = await _db.VillaAmenities.FirstOrDefaultAsync(u => u.Id == id);

                if (existingVillaAmenities == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Villa Amenities with ID {id} was not found"));
                }

                _db.VillaAmenities.Remove(existingVillaAmenities);
                await _db.SaveChangesAsync();

                var response = ApiResponse<object>.NoContent("Villa Amenities deleted successfully");
                return Ok(response);

            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, "An error occurred while creating the villa amenities:", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }


        [HttpGet("villa/{id:int}")]

        [ProducesResponseType(typeof(ApiResponse<VillaAmentiesDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<List<VillaAmentiesDTO>>>> GetVillasAmenitiesById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return NotFound(ApiResponse<object>.NotFound("Villa ID must be greater than 0"));
                }
                var villaExist = await _db.Villa.AnyAsync(u => u.Id.Equals(id));
                if (!villaExist)
                {
                    return NotFound(ApiResponse<object>.NotFound("Villa doesn't exist"));
                }
                var listOfAmenities = await _db.VillaAmenities.Include(x => x.villa).Where(u => u.VillaId == id).ToListAsync();



                if (!listOfAmenities.Any())
                {
                    return Ok(ApiResponse<List<VillaAmentiesDTO>>.Ok(_mapper.Map<List<VillaAmentiesDTO>>(listOfAmenities), "No amenities found"));
                }

                return Ok(ApiResponse<List<VillaAmentiesDTO>>.Ok(_mapper.Map<List<VillaAmentiesDTO>>(listOfAmenities), "Records retrieved successfully"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, "An error occurred while creating the villa:", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }
    }
}
