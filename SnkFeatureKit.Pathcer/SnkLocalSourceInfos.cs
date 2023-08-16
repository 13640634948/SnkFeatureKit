using System;
using System.Collections.Generic;
using System.Text;

namespace SnkFeatureKit.Patcher
{
        [Serializable]
        public class SnkLocalSourceInfos
        {
            public ushort resVersion;

            /// <summary>
            /// 历史版本
            /// </summary>
            public List<SnkSourceInfo> localSourceInfoList;

            /*
            public override string ToString()
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(resVersion.ToString());
                if (localSourceInfoList != null && localSourceInfoList.Count > 0)
                {
                    for (int i = 0; i < localSourceInfoList.Count; i++)
                    {
                        stringBuilder.AppendLine(localSourceInfoList[i].ToString());
                    }
                }
                return stringBuilder.ToString().Trim();
            }

            public static SnkLocalSourceInfos ValueOf(string content)
            {
                var infos = new SnkLocalSourceInfos();
                var array = content.Trim().Split('\n');
                if (array.Length <= 0)
                {
                    throw new System.Exception($"SnkLocalSourceInfos Parse Error. len lessthen 1. len:{array.Length}. content:{content}");
                }

                if (ushort.TryParse(array[0], out infos.resVersion) == false)
                {
                    throw new System.Exception($"Parse SnkLocalSourceInfos.AppVersion Error. param:{array[0]}. content:{content}");
                }

                infos.localSourceInfoList = new List<SnkSourceInfo>();
                if (array.Length > 1)
                {
                    for (int i = 1; i < array.Length; i++)
                    {
                        infos.localSourceInfoList.Add(SnkSourceInfo.ValueOf(array[i]));
                    }
                }

                return infos;
            }
            */
    }
}