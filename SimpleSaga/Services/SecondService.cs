using SimpleSaga.Events;
using SimpleSaga.Shared;
using SimpleSaga.Utils;

namespace SimpleSaga.Services
{
    public interface ISecondService
    {
        Task StartTransaction2(Guid transactionKey);
        Task CompensateTransaction2(Guid transactionKey);
    }

    internal class SecondService : ISecondService
    {
        private readonly IPubSubService _pubSubService;

        public SecondService(IPubSubService pubSubService)
        {
            _pubSubService = pubSubService;
        }

        public async Task CompensateTransaction2(Guid transactionKey)
        {
            Console.WriteLine($"[SecondService] Compensate transaction 2 with key {transactionKey} ...");

            await Task.Delay(2000);

            Console.WriteLine($"[SecondService] Finish compensating transaction");
        }

        public async Task StartTransaction2(Guid transactionKey)
        {
            Console.WriteLine($"[SecondService] Start transaction 2 with key {transactionKey} ...");

            await Task.Delay(2000);

            if (!Program.ShouldTransaction2Fail)
            {
                Console.WriteLine($"[SecondService] Finish processing transaction");
                Console.WriteLine($"[SecondService] Publish event Transaction2CompletedEvent");

                _pubSubService.Publish(
                    nameof(Transaction2CompletedEvent),
                    new Transaction2CompletedEvent
                    {
                        TransactionKey = transactionKey
                    });
            }
            else
            {
                Console.WriteLine($"[SecondService] [Exception] Transaction 2 failed");
                Console.WriteLine($"[SecondService] Publish event Transaction2FailureEvent");
                ConsoleHelper.WriteLineYellow("Press enter to continue");
                Console.ReadLine();

                _pubSubService.Publish(
                    nameof(Transaction2FailureEvent),
                    new Transaction2FailureEvent
                    {
                        TransactionKey = transactionKey
                    });
            }
        }
    }
}
