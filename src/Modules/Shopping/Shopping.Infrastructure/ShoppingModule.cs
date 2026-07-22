using FluentValidation;
using CoeurApi.Modules.Shopping.Application.Abstractions;
using CoeurApi.Modules.Shopping.Application.UseCases.Products.Create;
using CoeurApi.Modules.Shopping.Application.UseCases.Products.Delete;
using CoeurApi.Modules.Shopping.Application.UseCases.Products.GetAll;
using CoeurApi.Modules.Shopping.Application.UseCases.Products.GetById;
using CoeurApi.Modules.Shopping.Application.UseCases.Products.Update;
using CoeurApi.Modules.Shopping.Application.UseCases.ShoppingLists.AddItem;
using CoeurApi.Modules.Shopping.Application.UseCases.ShoppingLists.Create;
using CoeurApi.Modules.Shopping.Application.UseCases.ShoppingLists.Delete;
using CoeurApi.Modules.Shopping.Application.UseCases.ShoppingLists.GetAll;
using CoeurApi.Modules.Shopping.Application.UseCases.ShoppingLists.GetById;
using CoeurApi.Modules.Shopping.Application.UseCases.ShoppingLists.GetOwned;
using CoeurApi.Modules.Shopping.Application.UseCases.ShoppingLists.RemoveItem;
using CoeurApi.Modules.Shopping.Application.UseCases.ShoppingLists.Update;
using CoeurApi.Modules.Shopping.Application.UseCases.ShoppingLists.UpdateItem;
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
