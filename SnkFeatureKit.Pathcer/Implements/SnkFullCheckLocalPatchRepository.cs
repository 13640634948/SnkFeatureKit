using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SnkFeatureKit.Logging;
using SnkFeatureKit.Patcher.Interfaces;

namespace SnkFeatureKit.Patcher
{
    namespace Implements
    {
        public class SnkFullCheckLocalPatchRepository : SnkLocalPatchRepository
        {
            private List<SnkSourceInfo> _sourceInfoList;
            protected override List<SnkSourceInfo> sourceInfoList => _sourceInfoList;

            public override async Task<bool> Initialize(ISnkPatchController patchController)
            {
                await base.Initialize(patchController);
                _sourceInfoList = await GetSourceInfoList();
                return true;
            }

            protected override void SaveSourceInfo() 
            {

            }

            public override async Task<List<SnkSourceInfo>> GetSourceInfoList(ushort version = 0)
            {
                if (sourceInfoList != null)
                    return sourceInfoList;

                var bag = new ConcurrentBag<SnkSourceInfo>();

                SnkLogHost.Default?.Debug($"[SnkFullCheckLocalPatchRepository]LocalPath:{this.LocalPath}");
                try
                {
                    var rootDirInfo = new DirectoryInfo(this.LocalPath);
                    if (rootDirInfo.Exists == false)
                        rootDirInfo.Create();

                    List<Task> taskList = new List<Task>();
                    foreach (var dirInfo in rootDirInfo.GetDirectories("*", SearchOption.TopDirectoryOnly))
                    {
                        var task = Task.Run(async () =>
                        {
                            var finder = new SnkFileFinder(dirInfo.FullName);
                            var tmpList = await SnkPatch.GenerateSourceInfoList(version, finder);
                            if (tmpList != null && tmpList.Count > 0)
                                foreach (var a in tmpList)
                                    bag.Add(a);
                        });
                        taskList.Add(task);
                    }

                    var rootTask = Task.Run(async () =>
                    {
                        var finder = new SnkFileFinder(rootDirInfo.FullName, SearchOption.TopDirectoryOnly);
                        var tmpList = await SnkPatch.GenerateSourceInfoList(version, finder);
                        if (tmpList != null && tmpList.Count > 0)
                            foreach (var a in tmpList)
                                bag.Add(a);
                    });

                    taskList.Add(rootTask);
                    await Task.WhenAll(taskList);
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                return bag.ToList();
            }
        }
    }
}