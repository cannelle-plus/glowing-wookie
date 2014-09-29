using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using glowing.core;
using System.Net;

namespace glowing.projections
{

    public class SubscriptionDroppedException : Exception { }

    public class Subscription
    {

        public UserCredentials _user { get; set; }
        public bool _resolveLinkTos { get; set; }
        public IEventStoreConnection _connection { get; set; }

        public Subscription(IPEndPoint tcpEndPoint,string username, string password, bool resolveLinkTos)
        {
            _user = new UserCredentials(username,password);

            _connection = EventStoreConnection.Create(tcpEndPoint);
            _connection.Connect();

            _resolveLinkTos = resolveLinkTos;
        }
        
        public void subscribeTo(Projection p)
        {
            Action<EventStoreCatchUpSubscription, ResolvedEvent> next = (s, e) => p.Handle(e);
            Action<EventStoreCatchUpSubscription> catchup = (s) => Console.WriteLine("Catchup started");
            Action<EventStoreCatchUpSubscription, SubscriptionDropReason, Exception> error = (s, dr, e) => p.OnError(e);

            _connection.SubscribeToStreamFrom(p.Name, null , _resolveLinkTos, next, catchup, error, _user,500);
        }



        
    }




}
