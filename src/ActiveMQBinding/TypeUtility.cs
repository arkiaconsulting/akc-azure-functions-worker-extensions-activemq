using Microsoft.Azure.WebJobs;
using System;
using System.Reflection;

namespace Akc.Azure.WebJobs.Extensions.ActiveMQ
{
    internal static class TypeUtility
    {
        internal static TAttribute GetResolvedAttribute<TAttribute>(ParameterInfo parameter) where TAttribute : Attribute
        {
            var attribute = parameter.GetCustomAttribute<TAttribute>();

            if (attribute is IConnectionProvider attributeConnectionProvider
                && string.IsNullOrEmpty(attributeConnectionProvider.Connection))
            {
                // if the attribute doesn't specify an explicit connnection, walk up
                // the hierarchy looking for an override specified via attribute
                var connectionProviderAttribute = attribute.GetType().GetCustomAttribute<ConnectionProviderAttribute>();
                if (connectionProviderAttribute?.ProviderType != null
                    && GetHierarchicalAttributeOrNull(parameter, connectionProviderAttribute.ProviderType) is IConnectionProvider connectionOverrideProvider && !string.IsNullOrEmpty(connectionOverrideProvider.Connection))
                {
                    attributeConnectionProvider.Connection = connectionOverrideProvider.Connection;
                }
            }

            return attribute;
        }

        internal static Attribute GetHierarchicalAttributeOrNull(ParameterInfo parameter, Type attributeType)
        {
            if (parameter == null)
            {
                return null;
            }

            var attribute = parameter.GetCustomAttribute(attributeType);
            if (attribute != null)
            {
                return attribute;
            }

            var method = parameter.Member as MethodInfo;
            if (method == null)
            {
                return null;
            }

            return GetHierarchicalAttributeOrNull(method, attributeType);
        }

        internal static Attribute GetHierarchicalAttributeOrNull(MethodInfo method, Type type)
        {
            var attribute = method.GetCustomAttribute(type);
            if (attribute != null)
            {
                return attribute;
            }

            attribute = method.DeclaringType.GetCustomAttribute(type);
            if (attribute != null)
            {
                return attribute;
            }

            return null;
        }
    }
}
