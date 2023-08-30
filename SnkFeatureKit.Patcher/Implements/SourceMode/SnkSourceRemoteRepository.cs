using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SnkFeatureKit.Patcher.Abstracts;
using SnkFeatureKit.Patcher.Interfaces;

namespace SnkFeatureKit.Patcher
{
    namespace Implements
    {
        public class SnkSourceRemoteRepository : SnkRemoteRepositoryAbstract
        {
            private List<SnkSourceInfo> _sourceInfoList = new List<SnkSourceInfo>();

            public override async Task Initialize(ISnkPatchController patchController)
            {
                await base.Initialize(patchController);
                var url = "";
                try
                {
                    url = Path.Combine(GetCurrURL(), patchController.ChannelName, AppVersion.ToString(), ResVersion.ToString(), patchController.Settings.manifestFileName);
                    var content = await SnkHttpWeb.GetAsync(url);
                    this._sourceInfoList = jsonParser.FromJson<List<SnkSourceInfo>>(content);
                }
                catch (Exception exception)
                {
                    var tag = $"web request remote {patchController.Settings.manifestFileName} failed. url:{url}";
                    this.SetException(exception, tag);
                }
            }

            public override List<SnkSourceInfo> GetSourceInfoList(ushort version)
                => this._sourceInfoList;

        }
    }
}