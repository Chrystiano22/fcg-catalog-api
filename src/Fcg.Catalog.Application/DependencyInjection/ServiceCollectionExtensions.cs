using Fcg.Catalog.Application.Games.CreateGame;
using Fcg.Catalog.Application.Games.DeleteGame;
using Fcg.Catalog.Application.Games.ListGames;
using Fcg.Catalog.Application.Games.UpdateGame;
using Fcg.Catalog.Application.Libraries.AcquireGameForUser;
using Fcg.Catalog.Application.Libraries.GetUserLibrary;
using Fcg.Catalog.Application.Promotions.CreatePromotion;
using Fcg.Catalog.Application.Promotions.ListPromotions;
using Fcg.Catalog.Application.Purchases.ProcessPayment;
using Fcg.Catalog.Application.Purchases.PlaceOrder;
using Microsoft.Extensions.DependencyInjection;

namespace Fcg.Catalog.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICreateGameUseCase, CreateGameUseCase>();
        services.AddScoped<IDeleteGameUseCase, DeleteGameUseCase>();
        services.AddScoped<IAcquireGameForUserUseCase, AcquireGameForUserUseCase>();
        services.AddScoped<ICreatePromotionUseCase, CreatePromotionUseCase>();
        services.AddScoped<IGetUserLibraryUseCase, GetUserLibraryUseCase>();
        services.AddScoped<IListGamesUseCase, ListGamesUseCase>();
        services.AddScoped<IListPromotionsUseCase, ListPromotionsUseCase>();
        services.AddScoped<IUpdateGameUseCase, UpdateGameUseCase>();
        services.AddScoped<IPlaceOrderUseCase, PlaceOrderUseCase>();
        services.AddScoped<IProcessPaymentUseCase, ProcessPaymentUseCase>();

        return services;
    }
}
