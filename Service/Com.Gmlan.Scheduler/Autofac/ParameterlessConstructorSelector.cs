using Autofac.Core.Activators.Reflection;
using System;
using System.Linq;

namespace Com.Gmlan.Scheduler.Autofac
{
    public class ParameterlessConstructorSelector : IConstructorSelector
    {
        public ConstructorParameterBinding SelectConstructorBinding(
            ConstructorParameterBinding[] constructorBindings)
        {
            var defaultConstructor =
                constructorBindings.SingleOrDefault(c => c.TargetConstructor.GetParameters().Length != 0);
            if (defaultConstructor == null)
                //handle the case when there is no default constructor
                throw new InvalidOperationException();
            return defaultConstructor;
        }
    }
}
