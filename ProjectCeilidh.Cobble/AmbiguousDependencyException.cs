using System;
namespace ProjectCeilidh.Cobble
{
    public class AmbiguousDependencyException : Exception
    {
        public AmbiguousDependencyException(Type dependencyType) : base($"Could not decide on an instance to match {dependencyType.FullName}.")
        {
        }
    }
}
