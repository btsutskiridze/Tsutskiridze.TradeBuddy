using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types.Enums;
using Tsutskiridze.TradeBuddy.Application.Events;
using Tsutskiridze.TradeBuddy.Application.Features.StockAlerts;
using Tsutskiridze.TradeBuddy.Application.Interfaces.Database;
using Tsutskiridze.TradeBuddy.Application.Interfaces.Helpers;
using DbChat = Tsutskiridze.TradeBuddy.Core.DBEntities.Telegram.Chat;
using Tsutskiridze.TradeBuddy.Core.DBEntities.Telegram;
using Tsutskiridze.TradeBuddy.Core.Enums;
using Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Context;

namespace Tsutskiridze.TradeBuddy.Application.Tests.Features.StockAlerts
{
    public class PriceChangeAlertServiceTests
    {
        [Fact]
        public async Task Handle_WhenAlertUpdatedRecently_DoesNotUpdateAlert()
        {
            var harness = BuildHarness();
            var updatedAt = DateTime.UtcNow;
            SeedAlert(
                harness.Provider,
                alertCount: 0,
                price: 100m,
                direction: PriceAlertDirection.Above,
                updatedAt: updatedAt);

            await harness.Service.Handle(new PricingUpdated("AAPL", 150m), CancellationToken.None);

            using var verifyScope = harness.Provider.CreateScope();
            var db = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
            var alert = await db.PriceAlerts.SingleAsync();

            Assert.Equal(0, alert.AlertCount);
            Assert.Equal(updatedAt, alert.UpdatedAt);
        }

        [Fact]
        public async Task Handle_WhenAlertTriggered_SendsMessageAndUpdatesAlert()
        {
            var harness = BuildHarness();
            SeedAlert(harness.Provider, alertCount: 0, price: 100m, direction: PriceAlertDirection.Above);

            await harness.Service.Handle(new PricingUpdated("AAPL", 150m), CancellationToken.None);

            using var verifyScope = harness.Provider.CreateScope();
            var db = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
            var alert = await db.PriceAlerts.SingleAsync();

            Assert.Equal(1, alert.AlertCount);
            Assert.NotNull(alert.UpdatedAt);
        }

        [Fact]
        public async Task Handle_WhenMaxCountReached_RemovesAlertAndUnwatchesStock()
        {
            var harness = BuildHarness();
            SeedAlert(harness.Provider, alertCount: 4, price: 100m, direction: PriceAlertDirection.Above);

            await harness.Service.Handle(new PricingUpdated("AAPL", 150m), CancellationToken.None);

            using var verifyScope = harness.Provider.CreateScope();
            var db = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();

            Assert.False(await db.PriceAlerts.AnyAsync());

            var stock = await db.Stocks.SingleAsync();
            Assert.False(stock.IsWatched);
        }

        private static TestHarness BuildHarness()
        {
            var services = new ServiceCollection();
            var dbRoot = new InMemoryDatabaseRoot();
            services.AddDbContext<AppDbContext>(
                options => options.UseInMemoryDatabase($"tradebuddy-tests-{Guid.NewGuid()}", dbRoot),
                ServiceLifetime.Singleton,
                ServiceLifetime.Singleton);
            services.AddSingleton<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

            var provider = services.BuildServiceProvider();
            var botMock = CreateBotMock();
            var currencyMock = new Mock<ICurrencySymbolProvider>();
            currencyMock.Setup(x => x.GetSymbol(It.IsAny<string>())).Returns("$");

            var logger = new Mock<ILogger<PriceChangeAlertService>>();
            var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
            var service = new PriceChangeAlertService(scopeFactory, botMock.Object, logger.Object, currencyMock.Object);

            return new TestHarness(provider, service, botMock);
        }

        private static Mock<ITelegramBotClient> CreateBotMock()
        {
            var botMock = new Mock<ITelegramBotClient>();
            botMock
                .Setup(x => x.SendRequest(
                    It.IsAny<IRequest<Message>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Message)null!);

            return botMock;
        }

        private static void SeedAlert(
            IServiceProvider provider,
            int alertCount,
            decimal price,
            PriceAlertDirection direction,
            DateTime? updatedAt = null)
        {
            using var seedScope = provider.CreateScope();
            var db = seedScope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();

            var chat = new DbChat
            {
                ID = Guid.NewGuid(),
                TelegramChatID = 123456789
            };

            var stock = new Stock
            {
                ID = Guid.NewGuid(),
                Symbol = "AAPL",
                Currency = "USD",
                Name = "Apple",
                IsWatched = true
            };

            var alert = new PriceAlert
            {
                ID = Guid.NewGuid(),
                ChatID = chat.ID,
                StockID = stock.ID,
                Chat = chat,
                Stock = stock,
                Price = price,
                Direction = direction,
                AlertCount = alertCount,
                UpdatedAt = updatedAt
            };

            db.Chats.Add(chat);
            db.Stocks.Add(stock);
            db.PriceAlerts.Add(alert);
            db.SaveChanges();
        }

        private sealed record TestHarness(
            IServiceProvider Provider,
            PriceChangeAlertService Service,
            Mock<ITelegramBotClient> Bot);
    }
}
