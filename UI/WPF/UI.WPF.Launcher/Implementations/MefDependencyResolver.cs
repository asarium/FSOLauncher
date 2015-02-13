#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Linq;
using ReactiveUI;
using Splat;

#endregion

namespace UI.WPF.Launcher.Implementations
{
    public class MefDependencyResolver : IMutableDependencyResolver
    {
        private readonly CompositionContainer _container;

        private readonly IMutableDependencyResolver _previousResolver;

        public MefDependencyResolver(CompositionContainer container, IMutableDependencyResolver previousResolver)
        {
            _container = container;
            _previousResolver = previousResolver;
        }

        #region IMutableDependencyResolver Members

        public void Register(Func<object> factory, Type serviceType, string contract = null)
        {
            _previousResolver.Register(factory, serviceType, contract);
        }

        public IDisposable ServiceRegistrationCallback(Type serviceType, string contract, Action<IDisposable> callback)
        {
            return _previousResolver.ServiceRegistrationCallback(serviceType, contract, callback);
        }

        public void Dispose()
        {
            _previousResolver.Dispose();
        }

        public object GetService(Type serviceType, string contract = null)
        {
            var service = _previousResolver.GetService(serviceType, contract);

            if (service != null)
            {
                return service;
            }

            contract = string.IsNullOrEmpty(contract) ? AttributedModelServices.GetContractName(serviceType) : contract;
            var exports = _container.GetExportedValues<object>(contract);

            return exports.FirstOrDefault();
        }

        public IEnumerable<object> GetServices(Type serviceType, string contract = null)
        {
            var previous = _previousResolver.GetServices(serviceType, contract);

            if (previous == null || previous.Any())
            {
                return previous;
            }

            return _container.GetExportedValues<object>(AttributedModelServices.GetContractName(serviceType));
        }

        #endregion
    }
}
