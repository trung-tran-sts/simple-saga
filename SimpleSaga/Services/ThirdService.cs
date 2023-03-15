using SimpleSaga.Events;
using SimpleSaga.Shared;
using SimpleSaga.Utils;

namespace SimpleSaga.Services
{
    public interface IThirdService
    {
        Task StartTransaction3(Guid transactionKey);
        Task CompensateTransaction3(Guid transactionKey);
    }

    internal class ThirdService : IThirdService
    {
        private readonly IPubSubService _pubSubService;

        public ThirdService(IPubSubService pubSubService)
        {
            _pubSubService = pubSubService;
        }

        public async Task StartTransaction3(Guid transactionKey)
        {
            Console.WriteLine($"[ThirdService] Start transaction 3 with key {transactionKey} ...");

            await Task.Delay(2000);

            if (!Program.ShouldTransaction3Fail)
            {
                Console.WriteLine($"[ThirdService] Do external operation (pivot)");

                await Task.Delay(2000);

                Console.WriteLine($"[ThirdService] Finish processing transaction");
                Console.WriteLine($"[ThirdService] Publish event Transaction3CompletedEvent");

                _pubSubService.Publish(
                    nameof(Transaction3CompletedEvent),
                    new Transaction3CompletedEvent
                    {
                        TransactionKey = transactionKey
                    });
            }
            else
            {
                Console.WriteLine($"[ThirdService] [Exception] Transaction 3 failed");
                Console.WriteLine($"[ThirdService] Publish event Transaction3FailureEvent");
                ConsoleHelper.WriteLineYellow("Press enter to continue");
                Console.ReadLine();

                _pubSubService.Publish(
                    nameof(Transaction3FailureEvent),
                    new Transaction3FailureEvent
                    {
                        TransactionKey = transactionKey
                    });
            }
        }

        public async Task CompensateTransaction3(Guid transactionKey)
        {
            Console.WriteLine($"[ThirdService] Compensate transaction 3 with key {transactionKey} ...");

            await Task.Delay(2000);

            Console.WriteLine($"[ThirdService] Reverse external transaction 3 ...");

            await Task.Delay(2000);

            Console.WriteLine($"[ThirdService] Finish compensating transaction");
        }
    }
}
