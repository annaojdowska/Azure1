using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace WCFServiceWebRole1
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IService1
    {
        [OperationContract]
        bool AddUser(string login, string password);
        [OperationContract]
        bool CheckUser(string login, string password);
        [OperationContract]
        bool AddFile(string login, string password, string name, string content);
        [OperationContract]
        string GetFile(string login, string password, string name);
        [OperationContract]
        string List(string login, string password);

    }


    // Use a data contract as illustrated in the sample below to add composite types to service operations.
    [DataContract]
    public class UserEntity : TableEntity {
        public UserEntity(string login, string password) {
            this.PartitionKey = login; 
            this.RowKey = password; 
                              
        }
        public UserEntity() { }
    }
    [DataContract]
    public class UserBlobEntity : TableEntity {
        public UserBlobEntity(string login, string blobName) {
            this.PartitionKey = login;
            this.RowKey = blobName;

        }
        public UserBlobEntity() { }
    }
}
