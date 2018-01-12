using Com.Gmlan.Middlewares.MessageQueue.Extension;
using System;

namespace Com.Gmlan.Middlewares.MessageQueue.Model.Generic
{
    public class MessageBody
    {
        public MessageBody()
        {
        }

        public MessageBody(string serviceType, string methodName, object param)
        {
            ServiceTypeFullName = serviceType;
            ServiceMethodName = methodName;
            ParameterTypeFullName = param.GetType().AssemblyQualifiedName;
            SerializedJson = param.ToJson();
        }

        public string ServiceTypeFullName { get; set; }

        public string ServiceMethodName { get; set; }

        public string ParameterTypeFullName { get; set; }

        public string SerializedJson { get; set; }

        public object DeserializeAsParam()
        {
            return SerializedJson.FromJson(Type.GetType(ParameterTypeFullName));
        }
    }
}