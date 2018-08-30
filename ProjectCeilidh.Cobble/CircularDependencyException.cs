using System;
namespace ProjectCeilidh.Cobble
{
    public class CircularDependencyException : Exception
    {
        public CircularDependencyException() {}

        public CircularDependencyException(string message) : base(message)
        {

        }
    }
}
