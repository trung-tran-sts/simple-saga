using SimpleSaga.Commands;
using SimpleSaga.Events;
using SimpleSaga.Shared;

namespace SimpleSaga.Services
{
    public interface IOrchestrator
    {
        void Start();
    }

    public class Orchestrator : IOrchestrator
    {
        private readonly IPubSubService _pubSubService;

        public Orchestrator(IPubSubService pubSubService)
        {
            _pubSubService = pubSubService;
        }

        public void Start()
        {
            _pubSubService.Subscribe<Transaction1CompletedEvent>(
                nameof(Transaction1CompletedEvent),
                (@event) => SendStartTransaction2(@event.TransactionKey));

            _pubSubService.Subscribe<Transaction2CompletedEvent>(
                nameof(Transaction2CompletedEvent),
                (@event) => SendStartTransaction3(@event.TransactionKey));

            _pubSubService.Subscribe<Transaction3CompletedEvent>(
                nameof(Transaction3CompletedEvent),
                (@event) => SendDoCompleteAction(@event.TransactionKey));

            _pubSubService.Subscribe<CompletionFailure>(
                nameof(CompletionFailure),
                (@event) =>
                {
                    SendCompensateTransaction3(@event.TransactionKey);
                    SendCompensateTransaction2(@event.TransactionKey);
                    SendCompensateTransaction1(@event.TransactionKey);
                });

            _pubSubService.Subscribe<Transaction3FailureEvent>(
                nameof(Transaction3FailureEvent),
                (@event) =>
                {
                    SendCompensateTransaction2(@event.TransactionKey);
                    SendCompensateTransaction1(@event.TransactionKey);
                });

            _pubSubService.Subscribe<Transaction2FailureEvent>(
                nameof(Transaction2FailureEvent),
                (@event) =>
                {
                    SendCompensateTransaction1(@event.TransactionKey);
                });
        }

        public void SendStartTransaction2(Guid transactionKey)
        {
            _pubSubService.Publish(nameof(StartTransaction2Command), new StartTransaction2Command
            {
                TransactionKey = transactionKey
            });
        }

        public void SendStartTransaction3(Guid transactionKey)
        {
            _pubSubService.Publish(nameof(StartTransaction3Command), new StartTransaction3Command
            {
                TransactionKey = transactionKey
            });
        }

        public void SendDoCompleteAction(Guid transactionKey)
        {
            _pubSubService.Publish(nameof(DoCompleteActionCommand), new DoCompleteActionCommand
            {
                TransactionKey = transactionKey
            });
        }

        public void SendCompensateTransaction3(Guid transactionKey)
        {
            _pubSubService.Publish(nameof(CompensateTransaction3Command), new CompensateTransaction3Command
            {
                TransactionKey = transactionKey
            });
        }

        public void SendCompensateTransaction2(Guid transactionKey)
        {
            _pubSubService.Publish(nameof(CompensateTransaction2Command), new CompensateTransaction2Command
            {
                TransactionKey = transactionKey
            });
        }

        public void SendCompensateTransaction1(Guid transactionKey)
        {
            _pubSubService.Publish(nameof(CompensateTransaction1Command), new CompensateTransaction1Command
            {
                TransactionKey = transactionKey
            });
        }
    }
}
