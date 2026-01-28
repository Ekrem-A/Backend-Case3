using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Products.Application.Commands;
using Products.Application.DTOs;
using Products.Application.Queries;

namespace Products.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IMediator mediator, ILogger<ProductsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    
    /// Tüm ürünleri sayfalı olarak listeler
     
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ProductListDto>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] string? search = null)
    {
        var query = new GetAllProductsQuery(pageNumber, pageSize, categoryId, search);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    
    /// ID'ye göre ürün detayı getirir
     
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetById(Guid id)
    {
        var query = new GetProductByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound(new { message = $"Product with ID {id} not found" });

        return Ok(result);
    }

    
    /// Kategoriye göre ürünleri listeler
     
    [HttpGet("category/{categoryId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<ProductListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductListDto>>> GetByCategory(Guid categoryId)
    {
        var query = new GetProductsByCategoryQuery(categoryId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    
    /// Ürün arama
     
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<ProductListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductListDto>>> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { message = "Search term is required" });

        var query = new SearchProductsQuery(q);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    
    /// Yeni ürün ekler (Admin yetkisi gerekli)
     
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductDto dto)
    {
        var command = new CreateProductCommand(
            dto.Name,
            dto.Description,
            dto.SKU,
            dto.Brand,
            dto.Model,
            dto.Price,
            dto.Stock,
            dto.ImageUrl,
            dto.Specifications,
            dto.CategoryId
        );

        try
        {
            var result = await _mediator.Send(command);
            _logger.LogInformation("Product created: {ProductId} - {ProductName}", result.Id, result.Name);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    
    /// Ürün günceller (Admin yetkisi gerekli)
     
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> Update(Guid id, [FromBody] UpdateProductDto dto)
    {
        var command = new UpdateProductCommand(
            id,
            dto.Name,
            dto.Description,
            dto.Brand,
            dto.Model,
            dto.Price,
            dto.ImageUrl,
            dto.Specifications,
            dto.CategoryId
        );

        try
        {
            var result = await _mediator.Send(command);
            _logger.LogInformation("Product updated: {ProductId}", id);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    
    /// Ürün siler (Admin yetkisi gerekli)
     
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteProductCommand(id);
        var result = await _mediator.Send(command);

        if (!result)
            return NotFound(new { message = $"Product with ID {id} not found" });

        _logger.LogInformation("Product deleted: {ProductId}", id);
        return NoContent();
    }

    
    /// Stok günceller (Admin yetkisi gerekli)
     
    [HttpPatch("{id:guid}/stock")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> UpdateStock(Guid id, [FromBody] UpdateStockDto dto)
    {
        var command = new UpdateStockCommand(id, dto.Quantity);

        try
        {
            var result = await _mediator.Send(command);
            _logger.LogInformation("Product stock updated: {ProductId}, Quantity: {Quantity}", id, dto.Quantity);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

