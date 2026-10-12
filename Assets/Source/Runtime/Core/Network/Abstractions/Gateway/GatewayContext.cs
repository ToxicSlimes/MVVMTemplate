using System;
using System.Collections.Generic;

namespace Core.Network.Abstractions.Gateway
{
    public sealed class GatewayContext
    {
        public string Route { get; }
        public object Request { get; }
        public Type RequestType { get; }
        public Type ResponseType { get; }

        private Dictionary<Type, object> _features;

        public GatewayContext(string route, object request, Type requestType, Type responseType)
        {
            if (string.IsNullOrWhiteSpace(route))
                throw new ArgumentException("Gateway route cannot be empty", nameof(route));

            Route = route;
            Request = request ?? throw new ArgumentNullException(nameof(request));
            RequestType = requestType ?? throw new ArgumentNullException(nameof(requestType));
            ResponseType = responseType ?? throw new ArgumentNullException(nameof(responseType));

            if (RequestType.IsInstanceOfType(Request) == false)
                throw new ArgumentException("Gateway request does not match its declared type", nameof(request));
        }

        public void SetFeature<TFeature>(TFeature feature)
        {
            if (feature == null)
                throw new ArgumentNullException(nameof(feature));

            (_features ??= new Dictionary<Type, object>())[typeof(TFeature)] = feature;
        }

        public bool TryGetFeature<TFeature>(out TFeature feature)
        {
            if (_features != null &&
                _features.TryGetValue(typeof(TFeature), out var value) &&
                value is TFeature typedFeature)
            {
                feature = typedFeature;
                return true;
            }

            feature = default;
            return false;
        }
    }
}
