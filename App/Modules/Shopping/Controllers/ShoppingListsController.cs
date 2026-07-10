using Microsoft.AspNetCore.Mvc;
using CoeurApi.App.Modules.Shopping.DTOs;
using CoeurApi.App.Modules.Shopping.Services;
using CoeurApi.App.Shared.Interfaces;

namespace CoeurApi.App.Modules.Shopping.Controllers;

[ApiController]
[Route("api/shopping-lists")]
public class ShoppingListsController(ShoppingListsService service, ICurrentUser currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var lists = await service.GetAllAsync(currentUser.Id);
        return Ok(lists);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var list = await service.GetByIdAsync(id, currentUser.Id);
        return Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateShoppingListDto dto)
    {
        var list = await service.CreateAsync(dto, currentUser.Id);
        return Created($"api/shopping-lists/{list.Id}", list);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateShoppingListDto dto)
    {
        var list = await service.UpdateAsync(id, dto, currentUser.Id);
        return Ok(list);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await service.DeleteAsync(id, currentUser.Id);
        return NoContent();
    }

    [HttpPost("{id:guid}/items")]
    public async Task<IActionResult> AddItem(Guid id, [FromBody] AddListItemDto dto)
    {
        var item = await service.AddItemAsync(id, dto, currentUser.Id);
        return Created($"api/shopping-lists/{id}/items/{item.Id}", item);
    }

    [HttpPatch("{id:guid}/items/{itemId:guid}")]
    public async Task<IActionResult> UpdateItem(Guid id, Guid itemId, [FromBody] UpdateListItemDto dto)
    {
        var item = await service.UpdateItemAsync(id, itemId, dto, currentUser.Id);
        return Ok(item);
    }

    [HttpDelete("{id:guid}/items/{itemId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid id, Guid itemId)
    {
        await service.RemoveItemAsync(id, itemId, currentUser.Id);
        return NoContent();
    }
}
