using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using glowing.core;
using System.Net;
using System.Threading;

namespace glowing.projections
{
    public class SubscriptionDroppedException : Exception { }

    public class Subscription
    {

        private UserCredentials _user;
        private bool _resolveLinkTos;
        private IEventStoreConnection _connection;
        private IPEndPoint _tcpEndPoint;
        private bool _isConnected;
        private Dictionary<string, Projection> _projections = new Dictionary<string, Projection>();


        public Subscription(IPEndPoint tcpEndPoint,string username, string password, bool resolveLinkTos)
        {
            _user = new UserCredentials(username,password);
            _resolveLinkTos = resolveLinkTos;
            _tcpEndPoint = tcpEndPoint;

            ConnectToEventStore();
        }


        public void ConnectToEventStore()
        {
            Console.WriteLine("connecting to database...");

            _connection = EventStoreConnection.Create(_tcpEndPoint);
            _connection.Reconnecting += (sender, e) => {
                Console.WriteLine("reconnecting....");
            };
            _connection.Connected += (sender, e) => {
                if (!_isConnected)
                { 
                    Console.WriteLine("connected!");
                    _isConnected = true;
                    startProjections(_projections);
                }
            };
            _connection.Connect();
        }

        private void onError(EventStoreCatchUpSubscription s, SubscriptionDropReason dr, Exception e, Projection p)
        {

            p.OnError(e);

            switch (dr)
            {
                
                case SubscriptionDropReason.ConnectionClosed:
                    _isConnected = false;
                    Console.WriteLine(e);
                    break;
                case SubscriptionDropReason.NotAuthenticated:
                    _isConnected = false;
                    Console.WriteLine(e);
                    break;
                case SubscriptionDropReason.AccessDenied:
                case SubscriptionDropReason.CatchUpError:
                case SubscriptionDropReason.EventHandlerException:
                case SubscriptionDropReason.ProcessingQueueOverflow:
                case SubscriptionDropReason.ServerError:
                case SubscriptionDropReason.SubscribingError:
                case SubscriptionDropReason.Unknown:
                case SubscriptionDropReason.UserInitiated:
                default:
                    Console.WriteLine(e);
                    break;
            }
        }

        private void startProjections(Dictionary<string,Projection> projections)
        {
            foreach (var name in projections.Keys) startProjection(projections[name]);
        }

        private void startProjection(Projection p)
        {
            p.Start();
            
            Action<EventStoreCatchUpSubscription, ResolvedEvent> next = (s, e) => p.Handle(e);
            Action<EventStoreCatchUpSubscription> catchup = (s) => Console.WriteLine("Catchup started");
            Action<EventStoreCatchUpSubscription, SubscriptionDropReason, Exception> error = (s, dr, e) => onError(s, dr, e, p);

            _connection.SubscribeToStreamFrom(p.Name, p.LastCheckPoint, _resolveLinkTos, next, catchup, error, _user, 500);
            
        }
        
        public void subscribeTo(Projection p)
        {
            if (!_projections.ContainsKey(p.Name))
                _projections.Add(p.Name, p);

            if (_isConnected)
                startProjection(p);
        }



        
    }

}
