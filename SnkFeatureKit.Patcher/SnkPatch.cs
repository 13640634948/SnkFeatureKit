using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SnkFeatureKit.Logging;
using SnkFeatureKit.Patcher.Extensions;
using SnkFeatureKit.Patcher.Interfaces;
using SnkFeatureKit.Patcher.Implements;

namespace SnkFeatureKit.Patcher
{
    public class SnkPatch
    {
        public static ISnkCodeGenerator S_CodeGenerator = new SnkMD5Generator();

        public static List<SnkSourceInfo> GenerateSourceInfoList(ushort resVersion, ISnkFileFinder fileFinder, ref int count, ref int currNum, Dictionary<string,string> keyPathMapping = null, bool calculateMD5 = true)
        {
            if (fileFinder.TrySurvey(out var fileInfos) == false)
            {
                if(SnkLogHost.Default != null && SnkLogHost.Default.IsEnabled(SnkLogLevel.Warn))
                    SnkLogHost.Default.LogWarn("搜索目录文件失败。路径：{0}", fileFinder.SourceDirPath);
                return null;
            }
            var sourceInfoList = new List<SnkSourceInfo>();
            var mapping = new Dictionary<string, string>();
            var dirInfo = new DirectoryInfo(fileFinder.SourceDirPath);
            count = fileInfos.Length;
            currNum = 0;
            foreach (var fileInfo in fileInfos)
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
                        info.key = info.key.Substring(1);
                    }
                }

                info.version = resVersion;
                info.size = fileInfo.Length;
                if(calculateMD5)
                    info.code = S_CodeGenerator.CalculateFileMD5(fileInfo.FullName);
                sourceInfoList.Add(info);
                mapping[info.key] = fileInfo.FullName;
                currNum++;
            }

            if (keyPathMapping != null)
            {
                foreach (var entity in mapping)
                    keyPathMapping.Add(entity.Key, entity.Value);
            }

            return sourceInfoList;
        }

        public static void CopySourceTo(string toDirectoryFullPath, List<SnkSourceInfo> sourceInfoList, Dictionary<string,string> keyPathMapping)
        {
            foreach (var sourceInfo in sourceInfoList)
            {
                if (keyPathMapping.TryGetValue(sourceInfo.key, out var fullPath))
                {
                    var fromFileInfo = new FileInfo(fullPath);
                    var toFileInfo = new FileInfo(Path.Combine(toDirectoryFullPath, sourceInfo.key));
                    if (toFileInfo.Directory?.Exists == false)
                        toFileInfo.Directory.Create();
                    if(toFileInfo.Exists)
                        toFileInfo.Delete();
                    fromFileInfo.CopyTo(toFileInfo.FullName);
                }
                else
                {
                    throw new Exception("路径映射表中，没有好到key对应的路径. key:" + sourceInfo.key);
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
            return new Tuple<List<SnkSourceInfo>, List<string>>(addList, delList);
        }
    }
}
