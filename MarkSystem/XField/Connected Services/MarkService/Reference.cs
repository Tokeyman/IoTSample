﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace XField.MarkService {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="MarkService.IMarkService")]
    public interface IMarkService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMarkService/Pull", ReplyAction="http://tempuri.org/IMarkService/PullResponse")]
        System.Collections.Generic.Dictionary<string, object> Pull(string ClientGuid);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMarkService/Pull", ReplyAction="http://tempuri.org/IMarkService/PullResponse")]
        System.Threading.Tasks.Task<System.Collections.Generic.Dictionary<string, object>> PullAsync(string ClientGuid);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMarkService/Test", ReplyAction="http://tempuri.org/IMarkService/TestResponse")]
        string Test(string message);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMarkService/Test", ReplyAction="http://tempuri.org/IMarkService/TestResponse")]
        System.Threading.Tasks.Task<string> TestAsync(string message);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IMarkServiceChannel : XField.MarkService.IMarkService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class MarkServiceClient : System.ServiceModel.ClientBase<XField.MarkService.IMarkService>, XField.MarkService.IMarkService {
        
        public MarkServiceClient() {
        }
        
        public MarkServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public MarkServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public MarkServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public MarkServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public System.Collections.Generic.Dictionary<string, object> Pull(string ClientGuid) {
            return base.Channel.Pull(ClientGuid);
        }
        
        public System.Threading.Tasks.Task<System.Collections.Generic.Dictionary<string, object>> PullAsync(string ClientGuid) {
            return base.Channel.PullAsync(ClientGuid);
        }
        
        public string Test(string message) {
            return base.Channel.Test(message);
        }
        
        public System.Threading.Tasks.Task<string> TestAsync(string message) {
            return base.Channel.TestAsync(message);
        }
    }
}
