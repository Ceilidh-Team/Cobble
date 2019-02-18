using System;
using ProjectCeilidh.Cobble.Generator;

namespace ProjectCeilidh.Cobble.Callbacks
{
    /// <summary>
    /// Delegate called when an instance generator is added to a CobbleContext.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void InstanceGeneratorAddedEventHandler(object sender, InstanceGeneratorAddedEventArgs e);

    public class InstanceGeneratorAddedEventArgs : EventArgs
    {
        public IInstanceGenerator InstanceGenerator { get; }

        internal InstanceGeneratorAddedEventArgs(IInstanceGenerator instanceGenerator)
        {
            InstanceGenerator = instanceGenerator;
        }
    }
}
