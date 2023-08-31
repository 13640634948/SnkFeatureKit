using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SnkFeatureKit.Logging;
using SnkFeatureKit.Patcher.Interfaces;

namespace SnkFeatureKit.Patcher
{
    public class SnkPatchBuilder
    {
        private static readonly ISnkLogger logger = SnkLogHost.GetLogger<SnkPatchBuilder>();

        private readonly string _projPath;
        private readonly string _channelName;
        private readonly int _appVersion;
        private readonly SnkPatchSettings _settings;
        private readonly ISnkJsonParser _jsonParser;
        private readonly ISnkCompressor _compressor;

        public SnkPatchBuilder(string projPath, string channelName, int appVersion, SnkPatchSettings settings, ISnkJsonParser jsonParser, ISnkCompressor compressor = null)
        {
            this._projPath = projPath;
            this._channelName = channelName;
            this._appVersion = appVersion;
            this._settings = settings ?? new SnkPatchSettings();
            this._jsonParser = jsonParser;
            this._compressor = compressor;
        }

        private List<SnkVersionMeta> LoadResVersionInfos(string resVersionPath)
        {
            var fileInfo = new FileInfo(Path.Combine(resVersionPath, _settings.resVersionInfoFileName));
            if (fileInfo.Exists == false)
            {
                return new List<SnkVersionMeta>();
            }

            var jsonString = File.ReadAllText(fileInfo.FullName);
            return _jsonParser.FromJson<List<SnkVersionMeta>>(jsonString);
        }

        private List<int> LoadAppVersionInfos(string appVersionPath)
        {
            var fileInfo = new FileInfo(Path.Combine(appVersionPath, _settings.appVersionInfoFileName));
            if (fileInfo.Exists == false)
                return new List<int>();

            var jsonString = File.ReadAllText(fileInfo.FullName);
            return _jsonParser.FromJson<List<int>>(jsonString);
        }

        public async Task<SnkVersionMeta> Build(List<ISnkFileFinder> finderList, System.Func<string, bool> overrideReadOnlyFile = null, bool clearSourceDir = false)
        {
            if (finderList == null || finderList.Count == 0)
            {
                throw new System.Exception("finderList is null or len = 0");
            }

            if(logger != null && logger.IsEnabled(SnkLogLevel.Info))
                logger.LogInfo(Path.GetFullPath(this._projPath));

            var channelPath = Path.Combine(this._projPath, this._channelName);
            
            var appVersionPath = Path.Combine(channelPath, _appVersion.ToString());
            if (Directory.Exists(appVersionPath) == false)
                Directory.CreateDirectory(appVersionPath);

            var lastSourceInfoList = new List<SnkSourceInfo>();
            ushort resVersion = 1;
            ushort lastResVersion = 0;

            var appVersionList = LoadAppVersionInfos(channelPath);
            var resVersionList = LoadResVersionInfos(appVersionPath);

            var isHotUpdatePackage = resVersionList.Count > 0;
            if (isHotUpdatePackage)
            {
                lastResVersion = resVersionList.Last().version;
                resVersion = (ushort)(lastResVersion + 1);

                //加载最新的资源列表
                
                var lastResManifestPath = "";
                if(_compressor == null)
                    lastResManifestPath = Path.Combine(appVersionPath, lastResVersion.ToString(), _settings.manifestFileName);
                else
                    lastResManifestPath = Path.Combine(appVersionPath, _settings.manifestFileName);
                    
                var fileInfo = new FileInfo(lastResManifestPath);
                if (fileInfo.Exists)
                {
                    var jsonString = await Task.Run(() => File.ReadAllText(fileInfo.FullName));
                    var list = _jsonParser.FromJson<List<SnkSourceInfo>>(jsonString);
                    lastSourceInfoList.AddRange(list);
                }
            }

            var projResPath = Path.Combine(appVersionPath, resVersion.ToString());

            //生成当前目标目录的资源信息列表
            var keyPathMapping = new Dictionary<string, string>();
            var currSourceInfoList = new List<SnkSourceInfo>();

            var count = 0;
            var currNum = 0;
            foreach (var finder in finderList)
            {
                var list = await Task.Run(()=> SnkPatch.GenerateSourceInfoList(resVersion, finder, ref count ,ref currNum, keyPathMapping));
                if (list != null && list.Count > 0)
                    currSourceInfoList.AddRange(list);
            }

            if (isHotUpdatePackage == false && overrideReadOnlyFile != null)
            {
                for (int i = 0; i < currSourceInfoList.Count; i++)
                {
                    if (overrideReadOnlyFile(currSourceInfoList[i].key) == false)
                        continue;
                    var newSourceInfo = currSourceInfoList[i];
                    newSourceInfo.version = 0;
                    currSourceInfoList[i] = newSourceInfo;
                }
            }

            //生成差异列表
            var tuple = SnkPatch.CompareToDiff(lastSourceInfoList, currSourceInfoList);
            var addList = tuple.Item1;
            var delList = tuple.Item2;

            // 删除资源
            lastSourceInfoList.RemoveAll(a => delList.Exists(b => a.key == b));

            //新增资源，更新资源
            lastSourceInfoList.RemoveAll(a => addList.Exists(b => a.key == b.key));
            lastSourceInfoList.AddRange(addList);
                
            //保存最新的资源清单
            var manifestFileInfo = new FileInfo(Path.Combine(projResPath, this._settings.manifestFileName));
            if(manifestFileInfo.Exists)
                manifestFileInfo.Delete();
            if(manifestFileInfo.Directory?.Exists == false)
                manifestFileInfo.Directory.Create();
            File.WriteAllText(manifestFileInfo.FullName, _jsonParser.ToJson(lastSourceInfoList));

            if (_compressor != null)
            {
                var tmpManifestFilePath = Path.Combine(appVersionPath, manifestFileInfo.Name);
                if(File.Exists(tmpManifestFilePath))
                    File.Delete(tmpManifestFilePath);
                manifestFileInfo.CopyTo(tmpManifestFilePath);
            }

            if (isHotUpdatePackage)
            {
                var willCopyFileList = addList;
                willCopyFileList.RemoveAll(a => a.version == 0);
                //复制资源文件
                var patchAssetsDirPath = Path.Combine(projResPath, this._settings.assetsDirName);
                SnkPatch.CopySourceTo(patchAssetsDirPath, willCopyFileList, keyPathMapping);
            }
            else
            {
                addList.Clear();
            }
            
            //新版本元信息
            var versionMeta = new SnkVersionMeta
            {
                version = resVersion,
                from = lastResVersion,
            };
            
            if (_compressor != null)
            {
                var zipFileInfo = new System.IO.FileInfo(Path.Combine(appVersionPath, $"patcher_{resVersion}.zip"));
                await _compressor.Compress(projResPath, zipFileInfo.FullName);
                if (clearSourceDir)
                {
                    Directory.Delete(projResPath, true);
                }

                versionMeta.size = zipFileInfo.Length;
                versionMeta.count = zipFileInfo.Directory.GetFiles("*", SearchOption.TopDirectoryOnly).Length;
                versionMeta.code = SnkPatch.S_CodeGenerator.CalculateFileMD5(zipFileInfo.FullName);
            }
            else
            {
                versionMeta.size = addList.Sum(a => a.size);
                versionMeta.count = addList.Count;
                versionMeta.code = SnkPatch.S_CodeGenerator.CalculateFileMD5(manifestFileInfo.FullName);
            }

            resVersionList.Add(versionMeta);

            //保存版本信息
            var resVersionInfosPath = Path.Combine(appVersionPath, this._settings.resVersionInfoFileName);
            await Task.Run(() => File.WriteAllText(resVersionInfosPath, _jsonParser.ToJson(resVersionList)));

            if (appVersionList.Exists(a=> a == _appVersion) == false)
                appVersionList.Add(_appVersion);

            var appVersionInfosPath = Path.Combine(channelPath, this._settings.appVersionInfoFileName);
            await Task.Run(() => File.WriteAllText(appVersionInfosPath, _jsonParser.ToJson(appVersionList)));
            return versionMeta;
        }
    }
}
