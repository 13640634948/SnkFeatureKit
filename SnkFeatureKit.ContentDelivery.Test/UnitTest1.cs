using Newtonsoft.Json;
using NUnit.Framework;
using SnkFeatureKit.ContentDelivery.Implements;

namespace SnkFeatureKit.ContentDelivery.Test
{
    

    public class SnkJsonParser : ISnkJsonParser
    {
        public T FromJson<T>(string json) => JsonConvert.DeserializeObject<T>(json);
        public object FromJson(string json, Type objType) => JsonConvert.DeserializeObject(json, objType);
        public string ToJson(object target) => JsonConvert.SerializeObject(target);
    }
    
    public class Tests
    {
        private SnkBFSStorage _bfStore;
        
        [SetUp]
        public void Setup()
        {
            _bfStore = new SnkBFSStorage("DefaultBucket", "http://10.20.200.101:8090", string.Empty,
                string.Empty, true, new SnkJsonParser());
        
        }

        [Test]
        public void PutObjects()
        {
            var list = new List<SnkPutObject>();

            var dirPath = "/Users/dongjunjie/Documents/BFStorageTest";
            var dirInfo = new DirectoryInfo(dirPath);
            var allFiles = dirInfo.GetFiles("*.*", SearchOption.AllDirectories);
            for (var i = 0; i < allFiles.Length; i++)
            {
                var item = allFiles[i];
                var putObject = new SnkPutObject();
                putObject.path = item.FullName;
                putObject.key = "PutObjectsTest/" + Path.GetFileName(item.FullName);
                list.Add(putObject);
            }

            _bfStore.PutObjects(list);
        }

        [Test]
        public void TakeObjects()
        {
            var list = _bfStore.LoadObjects("PutObjectsTest/bluestacks.dmg");

            var keyList = new List<string>();
            for (var i = 0; i < list.Length; i++)
            {
                var item = list[i];
                keyList.Add(item.key);
            }

            _bfStore.TakeObjects(keyList,"/Users/dongjunjie/Documents/Test");
            int a = 0;
        }

        [Test]
        public void DeleteObjects()
        {
            var list = new List<string>();
            list.Add("PutObjectsTest/.DS_Store");
            
            _bfStore.DeleteObjects(list);
        }
    }
}