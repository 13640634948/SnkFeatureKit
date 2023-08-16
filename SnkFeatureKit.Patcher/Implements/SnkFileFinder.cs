using System.Collections.Generic;
using System.IO;
using System.Linq;

using SnkFeatureKit.Patcher.Interfaces;

namespace SnkFeatureKit.Patcher
{
    namespace Implements
    {
        public class SnkFileFinder : ISnkFileFinder
        {
            public string SourceDirPath => this._sourceDirPath;

            /// <summary>
            /// 资源目录路径
            /// </summary>
            protected string _sourceDirPath;

            /// <summary>
            /// 筛选关键字
            /// </summary>
            public string[] filters;

            /// <summary>
            /// 忽略关键字
            /// </summary>
            public string[] ignores;

            public SearchOption SearchOption { get; private set; }

            public SnkFileFinder(string sourceDirPath, SearchOption searchOption = SearchOption.AllDirectories)
            {
                this._sourceDirPath = sourceDirPath;
                this.SearchOption = searchOption;
            }

            /// <summary>
            /// 尝试探测资源
            /// </summary>
            /// <param name="fileInfos">构建出的资源文件信息</param>
            /// <param name="dirFullPath">资源目录的根路径</param>
            /// <returns>操作结果：true：成功， false：失败</returns>
            public virtual bool TrySurvey(out FileInfo[] fileInfos)
            {
                fileInfos = null;
                var dirInfo = new DirectoryInfo(_sourceDirPath);
                if (dirInfo.Exists == false)
                    return false;
                fileInfos = dirInfo.GetFiles("*.*", SearchOption);
                fileInfos = FiltersProcess(fileInfos);
                fileInfos = IgnoreProcess(fileInfos);
                return true;
            }

            /// <summary>
            /// 筛选过滤
            /// </summary>
            /// <param name="fileInfos"></param>
            /// <returns></returns>
            protected FileInfo[] FiltersProcess(FileInfo[] fileInfos)
            {
                if (filters == null || filters.Length == 0)
                    return fileInfos;

                return (from fileInfo in fileInfos
                        from filter in filters
                        where fileInfo.FullName.Contains(filter)
                        select fileInfo).ToArray();
            }

            /// <summary>
            /// 忽略
            /// </summary>
            /// <param name="fileInfos"></param>
            /// <returns></returns>
            protected FileInfo[] IgnoreProcess(FileInfo[] fileInfos)
            {
                if (ignores == null || ignores.Length == 0)
                    return fileInfos;

                var list = new List<FileInfo>(fileInfos);
                list.RemoveAll(fileInfo => ignores.Any(ignore => fileInfo.FullName.Contains(ignore)));
                return list.ToArray();
            }
        }
    }
}