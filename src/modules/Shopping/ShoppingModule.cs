using FluentValidation;
using CoeurApi.Modules.Shopping.Application.Abstractions;
using CoeurApi.Modules.Shopping.Application.Services.Products;
using CoeurApi.Modules.Shopping.Application.Services.ShoppingLists;
using CoeurApi.Modules.Shopping.Application.Validators;
using CoeurApi.Modules.Shopping.Infrastructure;

namespace CoeurApi.Modules.Shopping;

public static class ShoppingModule
{
    public static IServiceCollection AddShoppingModule(this IServiceCollection services)
    {
        services.AddScoped<IShoppingListRepository, ShoppingListRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();

        services.AddScoped<GetAllProductsService>();
        services.AddScoped<GetProductByIdService>();
        services.AddScoped<CreateProductService>();
        services.AddScoped<UpdateProductService>();
        services.AddScoped<DeleteProductService>();

        services.AddScoped<GetOwnedShoppingListService>();
        services.AddScoped<GetAllShoppingListsService>();
        services.AddScoped<GetShoppingListByIdService>();
        services.AddScoped<CreateShoppingListService>();
        services.AddScoped<UpdateShoppingListService>();
        services.AddScoped<DeleteShoppingListService>();
        services.AddScoped<AddShoppingListItemService>();
        services.AddScoped<UpdateShoppingListItemService>();
        services.AddScoped<RemoveShoppingListItemService>();

        services.AddValidatorsFromAssemblyContaining<CreateProductDtoValidator>();

        return services;
    }
}
