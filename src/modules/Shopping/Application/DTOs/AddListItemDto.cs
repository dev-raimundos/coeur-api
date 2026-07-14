namespace CoeurApi.Modules.Shopping.Application.DTOs;

public record AddListItemDto(
    string Name,
    int Quantity = 1,
    string? Unit = null,
    Guid? ProductId = null
);
