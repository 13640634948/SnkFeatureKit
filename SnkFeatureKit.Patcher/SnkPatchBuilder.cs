using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SnkFeatureKit.Patcher.Implements;
using SnkFeatureKit.Patcher.Interfaces;

namespace SnkFeatureKit.Patcher
{
    public class SnkPatchBuilder
    {
        private static readonly ILogger logger = SnkLogHost.GetLogger<SnkRemotePatchRepository>();

        private readonly string _projPath;
        private readonly string _channelName;
        private readonly int _appVersion;
        private readonly SnkPatchSettings _settings;
        private readonly ISnkJsonParser _jsonParser;

        public SnkPatchBuilder(string projPath, string channelName, int appVersion, SnkPatchSettings settings, ISnkJsonParser jsonParser)
        {
            this._projPath = projPath;
            this._channelName = channelName;
            this._appVersion = appVersion;
            this._settings = settings;
            this._jsonParser = jsonParser;
        }

        private SnkVersionInfos LoadVersionInfos(string appVersionPath)
        {
            var fileInfo = new FileInfo(Path.Combine(appVersionPath, _settings.versionInfoFileName));
            if (fileInfo.Exists == false)
            {
                var versionInfos = new SnkVersionInfos
                {
                    histories = new List<SnkVersionMeta>()
                };
                return versionInfos;
            }

            var jsonString = File.ReadAllText(fileInfo.FullName);
            return _jsonParser.FromJson<SnkVersionInfos>(jsonString);
        }

        public async Task<SnkVersionMeta> Build(List<ISnkFileFinder> finderList, System.Func<string, bool> overrideReadOnlyFile = null)
        {
            if (finderList == null || finderList.Count == 0)
            {
                throw new System.Exception("finderList is null or len = 0");
            }

            if(logger != null && logger.IsEnabled(LogLevel.Information))
                logger.LogInformation(Path.GetFullPath(this._projPath));

            var appVersionPath = Path.Combine(this._projPath, this._channelName, _appVersion.ToString());
            if (Directory.Exists(appVersionPath) == false)
                Directory.CreateDirectory(appVersionPath);

            var lastSourceInfoList = new List<SnkSourceInfo>();
            ushort resVersion = 1;

            var versionInfos = LoadVersionInfos(appVersionPath);

            var isHotUpdatePackage = versionInfos.histories.Count > 0;
            if (isHotUpdatePackage)
            {
                var lastResVersion = versionInfos.histories.Last().version;
                resVersion = (ushort)(lastResVersion + 1);

                //加载最新的资源列表
                var lastResManifestPath = Path.Combine(appVersionPath, lastResVersion.ToString(), _settings.manifestFileName);
                var fileInfo = new FileInfo(lastResManifestPath);
                if (fileInfo.Exists)
                {
                    var jsonString = await Task.Run(() => File.ReadAllText(fileInfo.FullName));
                    var list = _jsonParser.FromJson<List<SnkSourceInfo>>(jsonString);
                    lastSourceInfoList.AddRange(list);
                }
            }

            var projResPath = Path.Combine(appVersionPath, resVersion.ToString());
            if (Directory.Exists(projResPath) == false)
                Directory.CreateDirectory(projResPath);

            //生成当前目标目录的资源信息列表
            var keyPathMapping = new Dictionary<string, string>();
            var currSourceInfoList = new List<SnkSourceInfo>();

            foreach (var finder in finderList)
            {
                var list = await SnkPatch.GenerateSourceInfoList(resVersion, finder, keyPathMapping);
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
            var manifestPath = Path.Combine(projResPath, this._settings.manifestFileName);

            await Task.Run(() => File.WriteAllText(manifestPath, _jsonParser.ToJson(lastSourceInfoList)));

            var willCopyFileList = addList;
            willCopyFileList.RemoveAll(a => a.version == 0);
            //复制资源文件
            var patchAssetsDirPath = Path.Combine(projResPath, this._settings.assetsDirName);
            SnkPatch.CopySourceTo(patchAssetsDirPath, willCopyFileList, keyPathMapping);

            //新版本元信息
            var versionMeta = new SnkVersionMeta
            {
                version = resVersion,
                size = addList.Sum(a => a.size),
                count = addList.Count,
                code = SnkPatch.S_CodeGenerator.CalculateFileMD5(manifestPath)
            };
            versionInfos.histories.Add(versionMeta);
            versionInfos.appVersion = _appVersion;

            //保存版本信息
            var versionInfosPath = Path.Combine(appVersionPath, this._settings.versionInfoFileName);
            await Task.Run(() => File.WriteAllText(versionInfosPath, _jsonParser.ToJson(versionInfos)));

            return versionMeta;
        }
    }
}
