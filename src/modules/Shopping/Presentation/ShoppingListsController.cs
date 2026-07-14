using Microsoft.AspNetCore.Mvc;
using CoeurApi.Application.Abstractions;
using CoeurApi.Modules.Shopping.Application.DTOs;
using CoeurApi.Modules.Shopping.Application.Services.ShoppingLists;
using CoeurApi.SharedKernel.Common;

namespace CoeurApi.Modules.Shopping.Presentation;

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
    [ProducesResponseType<PagedResult<ShoppingListResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
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
    [ProducesResponseType<ShoppingListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShoppingListResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var list = await getShoppingListById.ExecuteAsync(id, currentUser.Id, cancellationToken);
        return Ok(list);
    }

    [HttpPost]
    [ProducesResponseType<ShoppingListResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ShoppingListResponse>> Create([FromBody] CreateShoppingListDto dto, CancellationToken cancellationToken)
    {
        var list = await createShoppingList.ExecuteAsync(dto, currentUser.Id, cancellationToken);
        return Created($"api/v1/shopping-lists/{list.Id}", list);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<ShoppingListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShoppingListResponse>> Update(Guid id, [FromBody] UpdateShoppingListDto dto, CancellationToken cancellationToken)
    {
        var list = await updateShoppingList.ExecuteAsync(id, dto, currentUser.Id, cancellationToken);
        return Ok(list);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await deleteShoppingList.ExecuteAsync(id, currentUser.Id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/items")]
    [ProducesResponseType<ListItemResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ListItemResponse>> AddItem(Guid id, [FromBody] AddListItemDto dto, CancellationToken cancellationToken)
    {
        var item = await addShoppingListItem.ExecuteAsync(id, dto, currentUser.Id, cancellationToken);
        return Created($"api/v1/shopping-lists/{id}/items/{item.Id}", item);
    }

    [HttpPatch("{id:guid}/items/{itemId:guid}")]
    [ProducesResponseType<ListItemResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ListItemResponse>> UpdateItem(Guid id, Guid itemId, [FromBody] UpdateListItemDto dto, CancellationToken cancellationToken)
    {
        var item = await updateShoppingListItem.ExecuteAsync(id, itemId, dto, currentUser.Id, cancellationToken);
        return Ok(item);
    }

    [HttpDelete("{id:guid}/items/{itemId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RemoveItem(Guid id, Guid itemId, CancellationToken cancellationToken)
    {
        await removeShoppingListItem.ExecuteAsync(id, itemId, currentUser.Id, cancellationToken);
        return NoContent();
    }
}
