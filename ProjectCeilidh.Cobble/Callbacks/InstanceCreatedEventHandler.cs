using System;
using ProjectCeilidh.Cobble.Generator;

namespace ProjectCeilidh.Cobble.Callbacks
{
    public delegate void InstanceCreatedEventHandler(object sender, InstanceCreatedEventArgs e);

    public class InstanceCreatedEventArgs : EventArgs
    {
        public IInstanceGenerator InstanceGenerator { get; }
        public object Instance { get; }

        internal InstanceCreatedEventArgs(IInstanceGenerator instanceGenerator, object instance)
        {
            InstanceGenerator = instanceGenerator;
            Instance = instance;
        }
    }
}
