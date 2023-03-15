using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleSaga.Commands;
using SimpleSaga.Services;
using SimpleSaga.Shared;
using SimpleSaga.Utils;

Start();

while (true) await Task.Delay(10000);

static void Start()
{
    IServiceProvider provider = SetUpProgram();

    StartFirstServiceListeners(provider);

    StartSecondServiceListeners(provider);

    StartThirdServiceListeners(provider);

    StartOrchestrator(provider);

    IPubSubService pubSubService = provider.GetRequiredService<IPubSubService>();

    pubSubService.Subscribe<bool>(NextTransactionChannel, async (initialized) =>
    {
        if (initialized)
        {
            ConsoleHelper.WriteLineYellow("Transaction ends, press enter to restart");
            Console.ReadLine();
        }

        Console.Clear();
        ConsoleHelper.WriteLineYellow("==== SIMPLE SAGA (Orchestration) ====");
        ConsoleHelper.WriteLineYellow("Press enter to start new transaction");
        Console.ReadLine();

        EnterConfiguration();

        await StartTransaction(provider);
    });

    pubSubService.Publish(NextTransactionChannel, false);
}

static IServiceProvider SetUpProgram()
{
    IConfiguration configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build();

    IServiceCollection services = new ServiceCollection();

    services
        .AddSingleton(configuration)
        .AddSingleton<IPubSubService, PubSubService>()
        .AddSingleton<IOrchestrator, Orchestrator>()
        .AddScoped<IThirdService, ThirdService>()
        .AddScoped<ISecondService, SecondService>()
        .AddScoped<IFirstService, FirstService>();

    IServiceProvider provider = services.BuildServiceProvider();

    return provider;
}

static void EnterConfiguration()
{
    ConsoleHelper.WriteYellow("Should transaction 2 fail (Y/N) (default N): ");
    ShouldTransaction2Fail = string.Equals(Console.ReadLine(), "y", StringComparison.OrdinalIgnoreCase);

    ConsoleHelper.WriteYellow("Should transaction 3 fail (Y/N) (default N): ");
    ShouldTransaction3Fail = string.Equals(Console.ReadLine(), "y", StringComparison.OrdinalIgnoreCase);

    ConsoleHelper.WriteYellow("Enter complete action try count (default 0): ");
    CompleteActionTryCount = int.TryParse(Console.ReadLine(), out int count) ? count : 0;

    Console.WriteLine();
}

static async Task StartTransaction(IServiceProvider provider)
{
    using IServiceScope scope = provider.CreateScope();

    IFirstService firstService = scope.ServiceProvider.GetRequiredService<IFirstService>();

    await firstService.StartTransaction1();
}

static void StartFirstServiceListeners(IServiceProvider provider)
{
    IPubSubService pubSubService = provider.GetRequiredService<IPubSubService>();

    pubSubService.Subscribe<DoCompleteActionCommand>(nameof(DoCompleteActionCommand), async (command) =>
    {
        ConsoleHelper.WriteLineYellow("Press enter to continue");
        Console.ReadLine();

        using IServiceScope scope = provider.CreateScope();

        IFirstService firstService = scope.ServiceProvider.GetRequiredService<IFirstService>();

        await firstService.DoCompleteAction(command.TransactionKey);
    });

    pubSubService.Subscribe<CompensateTransaction1Command>(
        nameof(CompensateTransaction1Command),
        async (command) =>
        {
            using IServiceScope scope = provider.CreateScope();

            IFirstService firstService = scope.ServiceProvider.GetRequiredService<IFirstService>();

            await firstService.CompensateTransaction1(command.TransactionKey);
        });
}

static void StartSecondServiceListeners(IServiceProvider provider)
{
    IPubSubService pubSubService = provider.GetRequiredService<IPubSubService>();

    pubSubService.Subscribe<StartTransaction2Command>(nameof(StartTransaction2Command), async (@event) =>
    {
        ConsoleHelper.WriteLineYellow("Press enter to continue");
        Console.ReadLine();

        using IServiceScope scope = provider.CreateScope();

        ISecondService secondService = scope.ServiceProvider.GetRequiredService<ISecondService>();

        await secondService.StartTransaction2(@event.TransactionKey);
    });

    pubSubService.Subscribe<CompensateTransaction2Command>(
        nameof(CompensateTransaction2Command),
        async (command) =>
        {
            using IServiceScope scope = provider.CreateScope();

            ISecondService secondService = scope.ServiceProvider.GetRequiredService<ISecondService>();

            await secondService.CompensateTransaction2(command.TransactionKey);
        });
}

static void StartThirdServiceListeners(IServiceProvider provider)
{
    IPubSubService pubSubService = provider.GetRequiredService<IPubSubService>();

    pubSubService.Subscribe<StartTransaction3Command>(nameof(StartTransaction3Command), async (@event) =>
    {
        ConsoleHelper.WriteLineYellow("Press enter to continue");
        Console.ReadLine();

        using IServiceScope scope = provider.CreateScope();

        IThirdService thirdService = scope.ServiceProvider.GetRequiredService<IThirdService>();

        await thirdService.StartTransaction3(@event.TransactionKey);
    });

    pubSubService.Subscribe<CompensateTransaction3Command>(
        nameof(CompensateTransaction3Command),
        async (command) =>
        {
            using IServiceScope scope = provider.CreateScope();

            IThirdService thirdService = scope.ServiceProvider.GetRequiredService<IThirdService>();

            await thirdService.CompensateTransaction3(command.TransactionKey);
        });
}

static void StartOrchestrator(IServiceProvider provider)
{
    IOrchestrator orchestrator = provider.GetRequiredService<IOrchestrator>();

    orchestrator.Start();
}

static partial class Program
{
    public const string NextTransactionChannel = "NextTransaction";

    public static bool ShouldTransaction2Fail { get; set; }
    public static bool ShouldTransaction3Fail { get; set; }
    public static int CompleteActionTryCount { get; set; }
}