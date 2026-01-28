using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Products.Application.Commands;
using Products.Application.DTOs;
using Products.Application.Queries;

namespace Products.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(IMediator mediator, ILogger<CategoriesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    
    /// Tüm kategorileri listeler
     
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll()
    {
        var query = new GetAllCategoriesQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    
    /// Kategori ağacını getirir (hiyerarşik)
     
    [HttpGet("tree")]
    [ProducesResponseType(typeof(IEnumerable<CategoryTreeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CategoryTreeDto>>> GetCategoryTree()
    {
        var query = new GetCategoryTreeQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    
    /// Yeni kategori oluşturur (Admin yetkisi gerekli)
     
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CategoryDto>> Create([FromBody] CreateCategoryDto dto)
    {
        var command = new CreateCategoryCommand(
            dto.Name,
            dto.Description,
            dto.ImageUrl,
            dto.ParentCategoryId
        );

        try
        {
            var result = await _mediator.Send(command);
            _logger.LogInformation("Category created: {CategoryId} - {CategoryName}", result.Id, result.Name);
            return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

