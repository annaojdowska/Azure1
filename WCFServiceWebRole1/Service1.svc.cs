using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
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
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class Service1 : IService1
    {
        private const string containerName = "blob-users";
        private const string usersTableName = "Users";
        private const string userBlobsTableName = "UserBlobs";

        public bool AddUser(string login, string password) {
            var account = CloudStorageAccount.DevelopmentStorageAccount;
            CloudTableClient cl = account.CreateCloudTableClient();
            var table = cl.GetTableReference("Users");
            table.CreateIfNotExists();
            if (!CheckUser(login, password)) {
                var e = new UserEntity(login, password);
                TableOperation op = TableOperation.Insert(e);
                var result = table.Execute(op);
                if (result != null) return true;
                else return false;
            }
            return false;
        }

        public bool CheckUser(string login, string password) {
            var account = CloudStorageAccount.DevelopmentStorageAccount;
            CloudTableClient cl = account.CreateCloudTableClient();
            var table = cl.GetTableReference(usersTableName);
            table.CreateIfNotExists();
            TableOperation op = TableOperation.Retrieve<UserEntity>(login, password);
            var result = table.Execute(op);
            UserEntity e = (UserEntity)result.Result;
            if (e != null) return true;
            else return false;
        }

        public bool AddFile(string login, string password, string name, string content) {
            if (CheckUser(login, password)) {
                var account = CloudStorageAccount.DevelopmentStorageAccount;
                CloudBlobClient client = account.CreateCloudBlobClient();
                CloudBlobContainer container = client.GetContainerReference(containerName);
                container.CreateIfNotExists();

                CloudTableClient clientTable = account.CreateCloudTableClient();
                var table = clientTable.GetTableReference(userBlobsTableName);
                table.CreateIfNotExists();
                var e = new UserBlobEntity(login, name);
                TableOperation op = TableOperation.Insert(e);
                var result = table.Execute(op);

                var blob = container.GetBlockBlobReference(name);
                var bytes = new System.Text.ASCIIEncoding().GetBytes(content);
                var s = new System.IO.MemoryStream(bytes);
                blob.UploadFromStream(s);
                return true;
            }
            return false;
        }
        public string GetFile(string login, string password, string name) {
            if (CheckUser(login, password)) {
                var account = CloudStorageAccount.DevelopmentStorageAccount;
                CloudBlobClient client = account.CreateCloudBlobClient();
                CloudBlobContainer container = client.GetContainerReference(containerName);
                container.CreateIfNotExists();

                var blob = container.GetBlockBlobReference(name);
                var stream = new System.IO.MemoryStream();
                blob.DownloadToStream(stream);
                string content = System.Text.Encoding.UTF8.GetString(stream.ToArray());
                return content;
            }
            return "";
        }

        public string List(string login, string password) {
            if(CheckUser(login, password)) {
                var account = CloudStorageAccount.DevelopmentStorageAccount;
                CloudBlobClient client = account.CreateCloudBlobClient();
                CloudBlobContainer container = client.GetContainerReference(containerName);
                container.CreateIfNotExists();

                CloudTableClient clientTable = account.CreateCloudTableClient();
                var table = clientTable.GetTableReference(userBlobsTableName);
                table.CreateIfNotExists();

                List<string> names = new List<string>();
                TableQuery<UserBlobEntity> query = new TableQuery<UserBlobEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, login));
                foreach (UserBlobEntity e in table.ExecuteQuery(query))
                    names.Add(e.RowKey);



                string list = "";
                foreach (IListBlobItem item in container.ListBlobs(null, false)) {
                    if (item.GetType() == typeof(CloudBlockBlob)) {
                        CloudBlockBlob blob = (CloudBlockBlob)item;
                        if (names.Contains(blob.Name)) {
                            list += String.Format("Name: {0} Content: {1}",
                            blob.Name, GetFile(login, password, blob.Name)) + "\n";
                        }
                    }
                }
                return list;
            }
            return "";
        }
    }
}
