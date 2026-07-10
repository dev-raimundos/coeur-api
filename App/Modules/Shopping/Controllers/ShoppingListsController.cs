using Microsoft.AspNetCore.Mvc;
using CoeurApi.App.Modules.Shopping.DTOs;
using CoeurApi.App.Modules.Shopping.Services.ShoppingLists;
using CoeurApi.App.Shared.DTOs;
using CoeurApi.App.Shared.Interfaces;

namespace CoeurApi.App.Modules.Shopping.Controllers;

[ApiController]
[Route("api/v1/shopping-lists")]
public class ShoppingListsController(
    GetAllShoppingListsService getAllShoppingLists,
    GetShoppingListByIdService getShoppingListById,
    CreateShoppingListService createShoppingList,
    UpdateShoppingListService updateShoppingList,
    DeleteShoppingListService deleteShoppingList,
    AddShoppingListItemService addShoppingListItem,
    UpdateShoppingListItemService updateShoppingListItem,
    RemoveShoppingListItemService removeShoppingListItem,
    ICurrentUser currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ShoppingListResponse>>> GetAll(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var (normalizedPage, normalizedPageSize) = Pagination.Normalize(page, pageSize);
        var lists = await getAllShoppingLists.ExecuteAsync(currentUser.Id, normalizedPage, normalizedPageSize, cancellationToken);
        return Ok(lists);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ShoppingListResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var list = await getShoppingListById.ExecuteAsync(id, currentUser.Id, cancellationToken);
        return Ok(list);
    }

    [HttpPost]
    public async Task<ActionResult<ShoppingListResponse>> Create([FromBody] CreateShoppingListDto dto, CancellationToken cancellationToken)
    {
        var list = await createShoppingList.ExecuteAsync(dto, currentUser.Id, cancellationToken);
        return Created($"api/v1/shopping-lists/{list.Id}", list);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ShoppingListResponse>> Update(Guid id, [FromBody] UpdateShoppingListDto dto, CancellationToken cancellationToken)
    {
        var list = await updateShoppingList.ExecuteAsync(id, dto, currentUser.Id, cancellationToken);
        return Ok(list);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await deleteShoppingList.ExecuteAsync(id, currentUser.Id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/items")]
    public async Task<ActionResult<ListItemResponse>> AddItem(Guid id, [FromBody] AddListItemDto dto, CancellationToken cancellationToken)
    {
        var item = await addShoppingListItem.ExecuteAsync(id, dto, currentUser.Id, cancellationToken);
        return Created($"api/v1/shopping-lists/{id}/items/{item.Id}", item);
    }

    [HttpPatch("{id:guid}/items/{itemId:guid}")]
    public async Task<ActionResult<ListItemResponse>> UpdateItem(Guid id, Guid itemId, [FromBody] UpdateListItemDto dto, CancellationToken cancellationToken)
    {
        var item = await updateShoppingListItem.ExecuteAsync(id, itemId, dto, currentUser.Id, cancellationToken);
        return Ok(item);
    }

    [HttpDelete("{id:guid}/items/{itemId:guid}")]
    public async Task<ActionResult> RemoveItem(Guid id, Guid itemId, CancellationToken cancellationToken)
    {
        await removeShoppingListItem.ExecuteAsync(id, itemId, currentUser.Id, cancellationToken);
        return NoContent();
    }
}
