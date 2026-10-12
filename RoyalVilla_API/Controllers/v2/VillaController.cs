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
using System.Text;
using RoyalVilla_API.Services.IServices;

namespace RoyalVilla_API.Controllers.v2
{
    [ApiController]
    //[ApiExplorerSettings(GroupName = "v1")]

    [Route("api/v{version:apiVersion}/Villa")]
    [ApiVersion("2.0")]// if multiple we can have here get post...Not two gets...
    public class VillaController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;
        public VillaController(ApplicationDbContext dbContext, IMapper mapper, IImageService imageService)
        {
            _db = dbContext;
            _mapper = mapper;
            _imageService = imageService;
        }

        [HttpGet]
        //[Authorize]
        //[Route("/GetVillas")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<VillaDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<VillaDTO>>>> getVilla([FromQuery] string? filterby, [FromQuery] string? filterQuery,
            [FromQuery] string? sortBy, [FromQuery] string? sortOrder, [FromQuery] int page = 1, [FromQuery] int pageSize=10)
        {
            if (page < 1) page=1;
            //if (pageSize < 10) pageSize = 10;
            if (pageSize > 100) pageSize = 100;


            var villaQuery = _db.Villa.AsQueryable();

            if (!string.IsNullOrEmpty(filterby) && !string.IsNullOrEmpty(filterQuery))
            {
                switch (filterby.ToLower())
                {
                    case "name":
                        villaQuery = villaQuery.Where(x => x.Name.ToLower().Contains(filterQuery.ToLower()));
                        break;
                    case "details":
                        villaQuery = villaQuery.Where(x => x.Details.ToLower().Contains(filterQuery.ToLower()));
                        break;
                    case "rate":
                        if (int.TryParse(filterQuery, out int rate))
                        {
                            villaQuery = villaQuery.Where(x => x.Rate == rate);
                        }
                        break;
                    case "sqft":
                        if (int.TryParse(filterQuery, out int sqft))
                        {
                            villaQuery = villaQuery.Where(x => x.Sqft == sqft);
                        }
                        break;
                    case "minrate":
                        if (int.TryParse(filterQuery, out int minrate))
                        {
                            villaQuery = villaQuery.Where(x => x.Rate >= minrate);
                        }
                        break;
                    case "maxrate":
                        if (int.TryParse(filterQuery, out int maxrate))
                        {
                            villaQuery = villaQuery.Where(x => x.Rate <= maxrate);
                        }
                        break;

                    case "occupancy":
                        if (int.TryParse(filterQuery, out int occupancy))
                        {
                            villaQuery = villaQuery.Where(x => x.Occupancy == occupancy);
                        }
                        break;
                }


            }

            //sorting logic
            if (!string.IsNullOrEmpty(sortBy))
            {
                var isDescending = sortOrder?.ToLower() == "desc";

                villaQuery = sortBy.ToLower() switch
                {
                   "name" => isDescending ? villaQuery.OrderByDescending(x => x.Name) : villaQuery.OrderBy(x => x.Name),
                   "rate" => isDescending ? villaQuery.OrderByDescending(x => x.Rate) : villaQuery.OrderBy(x => x.Rate),
                   "occupancy" => isDescending ? villaQuery.OrderByDescending(x => x.Occupancy) : villaQuery.OrderBy(x => x.Occupancy),
                   "sqft" => isDescending ? villaQuery.OrderByDescending(x => x.Sqft) : villaQuery.OrderBy(x => x.Sqft),
                   "id" => isDescending ? villaQuery.OrderByDescending(x => x.Id) : villaQuery.OrderBy(x => x.Id),
                   _ => villaQuery.OrderBy(x => x.Id)
                };
            }
            else
            {
                villaQuery.OrderBy(x => x.Name);
            }

            var skip = (page - 1) * pageSize;

            var totalCount = await villaQuery.CountAsync();

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var villa = await villaQuery.Skip(skip).Take(pageSize).ToListAsync();

            var dtoResponseVilla = _mapper.Map<List<VillaDTO>>(villa);

            var messageBuilder = new StringBuilder();
            messageBuilder.Append($"Successfully retrived {dtoResponseVilla.Count} ");
            messageBuilder.Append($"page {page} of {totalPages}, {totalCount} total records");

            if (!string.IsNullOrEmpty(filterby) && !string.IsNullOrEmpty(filterQuery))
            {
                messageBuilder.Append($" filtered by {filterby}: '{filterQuery}'");
            }
            if (!string.IsNullOrEmpty(sortOrder))
            {
                messageBuilder.Append($" Sorted by {sortBy}: '{sortOrder?.ToLower()?? "asc" }'");
            }

            //professional approach

            Response.Headers.Append("X-pagination-CurrentPage", page.ToString());
            Response.Headers.Append("X-pagination-PageSize", pageSize.ToString());
            Response.Headers.Append("X-pagination-totalCount", totalCount.ToString());
            Response.Headers.Append("X-pagination-totalPage", totalPages.ToString());


            var response = ApiResponse<IEnumerable<VillaDTO>>.Ok(dtoResponseVilla, messageBuilder.ToString());
            return Ok(response);
        }

        [HttpGet("{id:int}")]
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
                    return NotFound(ApiResponse<object>.NotFound($"Villa with ID {id} is not found"));
                }
                var response = ApiResponse<VillaDTO>.Ok(_mapper.Map<VillaDTO>(Villa), "Retrived successfully");

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, $"An error occured while retriving the id {id}", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        //[HttpGet("{id:int}/{name}")]
        //public string getVillaByAndName([FromQuery]int id, [FromQuery]string name)
        //{
        //    return "Got Villa " + id + " " + name;
        //} 

        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status502BadGateway)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<VillaDTO>>> CreateVilla([FromForm]VillaCreateDTO villaDTO)
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


                if (villaDTO.Image != null)
                {
                    if (!_imageService.ValidateImage(villaDTO.Image))
                    {
                        return BadRequest(ApiResponse<object>.BadRequest("Invalid image file. Allowed format .jpg, .jpeg, .png"));
                    }

                    villa.ImageUrl = await _imageService.UploadImageAsync(villaDTO.Image);
                }
                await _db.Villa.AddAsync(villa);

                await _db.SaveChangesAsync();

                var response = ApiResponse<VillaDTO>.CreatedAt(_mapper.Map<VillaDTO>(villa), "Villa created successfully");
                return CreatedAtAction(nameof(getVillaById), new { id = villa.Id }, response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, $"An error occured while creating a Villa", ex.Message);
                return StatusCode(500, errorResponse);
                //return StatusCode(StatusCodes.Status500InternalServerError, $"An error occured while creating a Villa : {ex.Message}");
            }
        }

        [HttpPut("{id:int}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status502BadGateway)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<VillaDTO>>> UpdateVilla(int id, [FromForm]VillaUpdateDTO villaUpdateDTO)
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
                if (villaUpdateDTO.Image != null && !_imageService.ValidateImage(villaUpdateDTO.Image))
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Invalid image file. Allowed format .jpg, .jpeg, .png"));
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

                var OldImageUrl = existingVilla.ImageUrl;

                if(villaUpdateDTO.Image != null)
                {
                    existingVilla.ImageUrl = await _imageService.UploadImageAsync(villaUpdateDTO.Image);
                    villaUpdateDTO.ImageUrl = existingVilla.ImageUrl;   

                    if (!string.IsNullOrEmpty(OldImageUrl) && OldImageUrl != existingVilla.ImageUrl) 
                    {
                        await _imageService.DeleteImageAsync(OldImageUrl);
                    }
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


                    if (!string.IsNullOrEmpty(existingVilla.ImageUrl))
                    {
                        await _imageService.DeleteImageAsync(existingVilla.ImageUrl);
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
