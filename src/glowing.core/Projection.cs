using EventStore.ClientAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace glowing.core
{
    public interface Projection
    {
        string Name { get; }
        int? LastCheckPoint { get; }
        void Start();
        void Handle(ResolvedEvent e);
        void OnError(Exception e);
    }
}
