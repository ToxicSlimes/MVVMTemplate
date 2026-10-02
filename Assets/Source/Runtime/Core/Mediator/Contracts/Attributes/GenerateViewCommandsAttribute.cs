using System;

namespace Core.Mediator.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class GenerateViewCommandsAttribute : Attribute
    {
    }
}
