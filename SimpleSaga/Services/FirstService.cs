using SimpleSaga.Events;
using SimpleSaga.Shared;
using SimpleSaga.Utils;

namespace SimpleSaga.Services
{
    public interface IFirstService
    {
        Task StartTransaction1();
        Task CompensateTransaction1(Guid transactionKey);
        Task DoCompleteAction(Guid transactionKey);
    }

    internal class FirstService : IFirstService
    {
        private readonly IPubSubService _pubSubService;

        public FirstService(IPubSubService pubSubService)
        {
            _pubSubService = pubSubService;
        }

        public async Task StartTransaction1()
        {
            Guid transactionKey = Guid.NewGuid();

            Console.WriteLine($"[FirstService] Start transaction 1 with transaction key {transactionKey} ...");

            await Task.Delay(2000);

            Console.WriteLine($"[FirstService] Finish processing transaction");
            Console.WriteLine($"[FirstService] Publish event Transaction1CompletedEvent");

            _pubSubService.Publish(
                nameof(Transaction1CompletedEvent),
                new Transaction1CompletedEvent
                {
                    TransactionKey = transactionKey
                });
        }

        public async Task CompensateTransaction1(Guid transactionKey)
        {
            Console.WriteLine($"[FirstService] Compensate transaction 1 with key {transactionKey} ...");

            await Task.Delay(2000);

            Console.WriteLine($"[FirstService] Finish compensating transaction");

            await RestartProgram();
        }

        public async Task DoCompleteAction(Guid transactionKey)
        {
            Console.WriteLine($"[FirstService] Do complete action (retryable) with transaction key {transactionKey} ...");

            int count = 0;
            bool success = false;

            do
            {
                try
                {
                    count++;

                    Console.WriteLine($"[FirstService] Try {count}");

                    await Task.Delay(2000);

                    if (count >= Program.CompleteActionTryCount)
                    {
                        Console.WriteLine($"[FirstService] Finish complete action");

                        success = true;
                    }
                    else
                    {
                        throw new Exception("Failed to do complete action");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[FirstService] [Exception] {ex.Message}");
                }
            } while (!success && count < 5);

            if (success)
            {
                await RestartProgram();
            }
            else
            {
                Console.WriteLine($"[FirstService] Publish event CompletionFailure");
                ConsoleHelper.WriteLineYellow("Press enter to continue");
                Console.ReadLine();

                _pubSubService.Publish(
                    nameof(CompletionFailure),
                    new CompletionFailure
                    {
                        TransactionKey = transactionKey
                    });
            }
        }

        private async Task RestartProgram()
        {
            await Task.Delay(2000);

            _pubSubService.Publish(Program.NextTransactionChannel, true);
        }
    }
}
