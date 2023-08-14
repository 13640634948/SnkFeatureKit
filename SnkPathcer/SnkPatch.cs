using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SnkToolKit.Features.Logging;
using SnkToolKit.Features.Patcher.Extension;
using SnkToolKit.Features.Patcher.Interfaces;
using SnkToolKit.Features.Patcher.Implements;

namespace SnkToolKit.Features.Patcher
{
        public partial class SnkPatch
        {
            public static ISnkCodeGenerator codeGenerator = new SnkMD5Generator();

            public static List<SnkSourceInfo> SnkSourceInfoListValueOf(string content)
            {
                var sourceInfoList = new List<SnkSourceInfo>();
                var tmpContent = content.Trim();
                if (string.IsNullOrEmpty(tmpContent))
                    return sourceInfoList;

                var array = tmpContent.Trim().Split('\n');
                if (array.Length <= 0)
                {
                    throw new System.Exception($"SnkPatch.SnkSourceInfoListValueOf Parse Error. len is error. len:{array.Length}. content:{content}");
                }
                else
                {
                    for (int i = 0; i < array.Length; i++)
                    {
                        sourceInfoList.Add(SnkSourceInfo.ValueOf(array[i].Trim()));
                    }
                }
                return sourceInfoList;
            }

            public static string SnkSourceInfoListToString(List<SnkSourceInfo> sourceInfoList)
            {
                if(sourceInfoList == null || sourceInfoList.Count == 0)
                    return string.Empty;
                var stringBuilder = new StringBuilder();
                for (int i = 0;i< sourceInfoList.Count;i++)
                    stringBuilder.AppendLine(sourceInfoList[i].ToString());
                return stringBuilder.ToString().Trim();

            }

            public static SnkPatchBuilder CreatePatchBuilder(string projPath, string channelName, int appVersion, SnkPatchSettings settings = null)
            {
                var builder = new SnkPatchBuilder(projPath, channelName, appVersion, settings ?? new SnkPatchSettings());
                return builder;
            }

            public static ISnkPatchController CreatePatchExecutor<TLocalRepo, TRemoteRepo>(SnkPatchControlSettings settings, int threadNumber)
                where TLocalRepo : class, ISnkLocalPatchRepository, new()
                where TRemoteRepo : class, ISnkRemotePatchRepository, new()
            {
                return new SnkPatchController<TLocalRepo, TRemoteRepo>(settings, threadNumber);
            }

            public static ISnkPatchController CreatePatchExecutor(SnkPatchControlSettings settings, int threadNumber)
                => CreatePatchExecutor<SnkLocalPatchRepository, SnkRemotePatchRepository>(settings, threadNumber);

            public static async Task<List<SnkSourceInfo>> GenerateSourceInfoList(ushort resVersion, ISnkFileFinder fileFinder, Dictionary<string,string> keyPathMapping = null)
            {
                if (fileFinder.TrySurvey(out var fileInfos) == false)
                {
                    SnkLogHost.Default?.WarnFormat("搜索目录文件失败。路径：{0}", fileFinder.SourceDirPath);
                    return null;
                }

                var sourceInfoBag = new ConcurrentBag<SnkSourceInfo>();
                var fileInfoBag = new ConcurrentBag<FileInfo>(fileInfos);
                var mappingBag = new ConcurrentBag<Tuple<string, string>>();

                var dirInfo = new DirectoryInfo(fileFinder.SourceDirPath);
                var taskList = new List<Task>();
                var fileInfoCount = fileInfoBag.Count;
                const int threadNumber = 12;

                await Task.Run(() => 
                {
                    while (sourceInfoBag.Count < fileInfoCount)
                    {
                        for (int i = 0; i < taskList.Count; i++) 
                        {
                            if (taskList[i].IsCompleted)
                                taskList.RemoveAt(i--);
                        }

                        if (taskList.Count < threadNumber && fileInfoBag.Count > 0)
                        {
                            if (fileInfoBag.TryTake(out var fileInfo) == false)
                                continue;

                            var task = Task.Run(() => 
                            {
                                var info = new SnkSourceInfo();

                                if (fileFinder.SearchOption == SearchOption.AllDirectories)
                                {
                                    info.key = fileInfo.FullName.Replace(dirInfo.FullName, dirInfo.Name).FixSlash();
                                }
                                else
                                {
                                    info.key = fileInfo.FullName.Replace(dirInfo.FullName, string.Empty).FixSlash();
                                    if (info.key.StartsWith("/"))
                                    {
                                        //info.key = info.key[1..];
                                        info.key = info.key.Substring(1);
                                    }
                                }

                                info.version = resVersion;
                                info.size = fileInfo.Length;
                                info.code = SnkPatch.codeGenerator.CalculateFileMD5(fileInfo.FullName);

                                sourceInfoBag.Add(info);
                                mappingBag.Add(new Tuple<string, string>(info.key, fileInfo.FullName));
                            });
                            taskList.Add(task);
                        }
                    }
                });

                if(keyPathMapping != null)
                    foreach (var entity in mappingBag)
                        keyPathMapping.Add(entity.Item1, entity.Item2);

                return sourceInfoBag.ToList();
            }

            public static void CopySourceTo(string toDirectoryFullPath, List<SnkSourceInfo> sourceInfoList, Dictionary<string,string> keyPathMapping)
            {
                foreach (var sourceInfo in sourceInfoList)
                {
                    if (keyPathMapping.TryGetValue(sourceInfo.key, out string fullPath))
                    {
                        var fromFileInfo = new System.IO.FileInfo(fullPath);
                        var toFileInfo = new System.IO.FileInfo(System.IO.Path.Combine(toDirectoryFullPath, sourceInfo.key));
                        if (toFileInfo.Directory.Exists == false)
                            toFileInfo.Directory.Create();
                        if(toFileInfo.Exists)
                            toFileInfo.Delete();
                        fromFileInfo.CopyTo(toFileInfo.FullName);
                    }
                    else
                    {
                        throw new System.Exception("路径映射表中，没有好到key对应的路径. key:" + sourceInfo.key);
                    }
                }
            }

            public static Tuple<List<SnkSourceInfo>, List<string>> CompareToDiff(List<SnkSourceInfo> from, List<SnkSourceInfo> to)
            {
                var addList = new List<SnkSourceInfo>();
                foreach (var a in to)
                {
                    if (from.Exists(xx => xx.key == a.key && xx.code == a.code))
                        continue;
                    addList.Add(a);
                }

                var delList = new List<string>();
                foreach (var a in from)
                {
                    if (to.Exists(xx => xx.key == a.key))
                        continue;
                    delList.Add(a.key);
                }

                SnkLogHost.Default?.Debug($"--------------------[CompareToDiff]----Start----------------");
                SnkLogHost.Default?.Debug($"[FROM]{from.Count}\n{SnkPatch.SnkSourceInfoListToString(from)}");
                SnkLogHost.Default?.Debug($"[TO]{to.Count}\n{SnkPatch.SnkSourceInfoListToString(to)}");
                SnkLogHost.Default?.Debug($"[ADD]{addList.Count}\n{SnkPatch.SnkSourceInfoListToString(addList)}");
                var outputString = $"[DEL]{delList.Count}\n";
                foreach (var a in delList)
                    outputString += a.ToString() + "\n";
                SnkLogHost.Default?.Debug(outputString.Trim());

                SnkLogHost.Default?.Debug($"--------------------[CompareToDiff]----End----------------");
                return new Tuple<List<SnkSourceInfo>, List<string>>(addList, delList);
            }
        }
    }
