using Microsoft.AspNetCore.Mvc;
using CoeurApi.Application.Abstractions;
using CoeurApi.Modules.Shopping.Application.UseCases.ShoppingLists;
using CoeurApi.Modules.Shopping.Application.UseCases.ShoppingLists.AddItem;
using CoeurApi.Modules.Shopping.Application.UseCases.ShoppingLists.Create;
using CoeurApi.Modules.Shopping.Application.UseCases.ShoppingLists.Delete;
using CoeurApi.Modules.Shopping.Application.UseCases.ShoppingLists.GetAll;
using CoeurApi.Modules.Shopping.Application.UseCases.ShoppingLists.GetById;
using CoeurApi.Modules.Shopping.Application.UseCases.ShoppingLists.RemoveItem;
using CoeurApi.Modules.Shopping.Application.UseCases.ShoppingLists.Update;
using CoeurApi.Modules.Shopping.Application.UseCases.ShoppingLists.UpdateItem;
using CoeurApi.SharedKernel.Common;

namespace CoeurApi.Modules.Shopping.Presentation.Controller;

[ApiController]
[Route("api/v1/shopping-lists")]
public class ShoppingListsController(
    GetAllShoppingListsUseCase getAllShoppingLists,
    GetShoppingListByIdUseCase getShoppingListById,
    CreateShoppingListUseCase createShoppingList,
    UpdateShoppingListUseCase updateShoppingList,
    DeleteShoppingListUseCase deleteShoppingList,
    AddShoppingListItemUseCase addShoppingListItem,
    UpdateShoppingListItemUseCase updateShoppingListItem,
    RemoveShoppingListItemUseCase removeShoppingListItem,
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
    public async Task<ActionResult<ShoppingListResponse>> Create([FromBody] CreateShoppingListRequest request, CancellationToken cancellationToken)
    {
        var list = await createShoppingList.ExecuteAsync(request, currentUser.Id, cancellationToken);
        return Created($"api/v1/shopping-lists/{list.Id}", list);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<ShoppingListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShoppingListResponse>> Update(Guid id, [FromBody] UpdateShoppingListRequest request, CancellationToken cancellationToken)
    {
        var list = await updateShoppingList.ExecuteAsync(id, request, currentUser.Id, cancellationToken);
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
    public async Task<ActionResult<ListItemResponse>> AddItem(Guid id, [FromBody] AddShoppingListItemRequest request, CancellationToken cancellationToken)
    {
        var item = await addShoppingListItem.ExecuteAsync(id, request, currentUser.Id, cancellationToken);
        return Created($"api/v1/shopping-lists/{id}/items/{item.Id}", item);
    }

    [HttpPatch("{id:guid}/items/{itemId:guid}")]
    [ProducesResponseType<ListItemResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ListItemResponse>> UpdateItem(Guid id, Guid itemId, [FromBody] UpdateShoppingListItemRequest request, CancellationToken cancellationToken)
    {
        var item = await updateShoppingListItem.ExecuteAsync(id, itemId, request, currentUser.Id, cancellationToken);
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
