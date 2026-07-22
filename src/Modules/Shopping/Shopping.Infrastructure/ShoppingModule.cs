using FluentValidation;
using CoeurApi.Modules.Shopping.Application.Abstractions;
using CoeurApi.Modules.Shopping.Application.UseCases.Products;
using CoeurApi.Modules.Shopping.Application.UseCases.ShoppingLists;
using CoeurApi.Modules.Shopping.Infrastructure;

namespace CoeurApi.Modules.Shopping;

public static class ShoppingModule
{
    public static IServiceCollection AddShoppingModule(this IServiceCollection services)
    {
        services.AddScoped<IShoppingListRepository, ShoppingListRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();

        services.AddScoped<GetAllProductsUseCase>();
        services.AddScoped<GetProductByIdUseCase>();
        services.AddScoped<CreateProductUseCase>();
        services.AddScoped<UpdateProductUseCase>();
        services.AddScoped<DeleteProductUseCase>();

        services.AddScoped<GetOwnedShoppingListUseCase>();
        services.AddScoped<GetAllShoppingListsUseCase>();
        services.AddScoped<GetShoppingListByIdUseCase>();
        services.AddScoped<CreateShoppingListUseCase>();
        services.AddScoped<UpdateShoppingListUseCase>();
        services.AddScoped<DeleteShoppingListUseCase>();
        services.AddScoped<AddShoppingListItemUseCase>();
        services.AddScoped<UpdateShoppingListItemUseCase>();
        services.AddScoped<RemoveShoppingListItemUseCase>();

        services.AddValidatorsFromAssemblyContaining<CreateProductValidator>();

        return services;
    }
}
