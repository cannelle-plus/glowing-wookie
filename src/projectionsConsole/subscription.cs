using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace projectionsConsole
{

    public class SubscriptionDroppedException : Exception { }

    public class Subscription
    {

        public UserCredentials _user { get; set; }
        public bool _resolveLinkTos { get; set; }
        public IEventStoreConnection _connection { get; set; }

        public Subscription(IEventStoreConnection connection, bool resolveLinkTos, UserCredentials user)
        {
            _connection = connection;
            _resolveLinkTos = resolveLinkTos;
            _user = user;
        }
        
        public void subscribeTo(Projection p)
        {
            Action<EventStoreCatchUpSubscription, ResolvedEvent> next = (s, e) => p.Handle(e);
            Action<EventStoreCatchUpSubscription> catchup = (s) => Console.WriteLine("Catchup started");
            Action<EventStoreCatchUpSubscription, SubscriptionDropReason, Exception> error = (s, dr, e) => p.OnError(e);

            _connection.SubscribeToStreamFrom(p.Name, EventStore.ClientAPI.StreamPosition.Start, _resolveLinkTos, next, catchup, error, _user,500);
        }



        
    }




}
