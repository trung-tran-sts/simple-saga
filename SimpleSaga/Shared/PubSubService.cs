using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace SimpleSaga.Shared
{
    public interface IPubSubService
    {
        void Subscribe<T>(string channel, Action<T> action);
        void Publish<T>(string channel, T message);
    }

    public class PubSubService : IPubSubService
    {
        private readonly ConcurrentDictionary<string, object> _subjectMap;

        public PubSubService()
        {
            _subjectMap = new ConcurrentDictionary<string, object>();
        }

        public void Publish<T>(string channel, T message)
        {
            ISubject<T> subject = GetSubject<T>(channel);

            subject.OnNext(message);
        }

        public void Subscribe<T>(string channel, Action<T> action)
        {
            ISubject<T> subject = GetSubject<T>(channel);

            subject.Subscribe(action);
        }

        private ISubject<T> GetSubject<T>(string channel)
        {
            return _subjectMap.GetOrAdd(channel, (key) => new Subject<T>()) as ISubject<T>;
        }
    }
}
