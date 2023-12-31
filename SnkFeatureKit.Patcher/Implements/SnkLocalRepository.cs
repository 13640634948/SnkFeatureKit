using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SnkFeatureKit.Patcher.Interfaces;

namespace SnkFeatureKit.Patcher.Implements
{
    public class SnkLocalRepository : ISnkLocalPatchRepository
    {
        private bool _disposed = false;

        protected virtual string ArchiveFilePath => patchController.Settings.localArchiveFileName;

        protected ISnkPatchController patchController { get; private set; }
        protected ISnkJsonParser jsonParser { get; private set; }
        
        public int AppVersion { get; private set; }
        public virtual ushort ResVersion => this.localArchive.resVersion;
        public string LocalPath => patchController.Settings.localPatchRepoPath;

        protected List<SnkSourceInfo> sourceInfoList = new List<SnkSourceInfo>();

        protected SnkLocalArchive localArchive;
        protected virtual bool CalculateMD5 => true;

        public bool IsError { get; }
        public string ExceptionString { get; }

        protected virtual List<SnkSourceInfo> LoadSourceInfo()
        {
            var count = 0;
            var currNum = 0;
            var finder = new SnkFileFinder(this.LocalPath);
            return SnkPatch.GenerateSourceInfoList(0, finder, ref count, ref currNum, calculateMD5:CalculateMD5) ?? new List<SnkSourceInfo>();
        }


        public virtual Task Initialize(ISnkPatchController patchController)
        {
            this.patchController = patchController;
            this.jsonParser = patchController.jsonParser;
            this.AppVersion = patchController.Settings.appVersion;
            this.localArchive = LoadLocalArchive();
            this.sourceInfoList = LoadSourceInfo();
            return Task.CompletedTask;
        }
        
        protected SnkLocalArchive LoadLocalArchive()
        {
            SnkLocalArchive localSourceInfos = null;
            if (System.IO.File.Exists(ArchiveFilePath))
            {
                var json = System.IO.File.ReadAllText(ArchiveFilePath);
                localSourceInfos = jsonParser.FromJson<SnkLocalArchive>(json);
            }
            return localSourceInfos ?? new SnkLocalArchive();
        }

        public virtual List<SnkSourceInfo> GetSourceInfoList(ushort version) => sourceInfoList;

        public virtual void UpdateLocalResVersion(ushort resVersion)
        {
            this.localArchive.resVersion = resVersion;
            System.IO.File.WriteAllText(ArchiveFilePath, jsonParser.ToJson(this.localArchive));
        }
        
        ~SnkLocalRepository()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_disposed == false)
                {
                    _disposed = true;

                    if (sourceInfoList != null)
                    {
                        sourceInfoList.Clear();
                    }
                }
            }
        }
    }
}