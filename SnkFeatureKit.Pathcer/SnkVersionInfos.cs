using System;
using System.Collections.Generic;
using System.Text;

namespace SnkFeatureKit.Patcher
{
    [Serializable]
    public class SnkVersionInfos
    {
        /// <summary>
        /// 应用版本
        /// </summary>
        public int appVersion;

        /// <summary>
        /// 历史版本
        /// </summary>
        public List<SnkVersionMeta> histories;

        /*
        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(appVersion.ToString());
            if (histories != null && histories.Count > 0)
            {
                for (int i = 0; i < histories.Count; i++)
                {
                    stringBuilder.AppendLine(histories[i].ToString());
                }
            }
            return stringBuilder.ToString().Trim();
        }

        public static SnkVersionInfos ValueOf(string content)
        {
            var infos = new SnkVersionInfos();
            var array = content.Trim().Split('\n');
            if (array.Length <= 0)
            {
                throw new System.Exception($"SnkVersionInfos Parse Error. len lessthen 1. len:{array.Length}. content:{content}");
            }

            if (int.TryParse(array[0], out infos.appVersion) == false)
            {
                throw new System.Exception($"Parse SnkVersionInfos.AppVersion Error. param:{array[0]}. content:{content}");
            }

            infos.histories = new List<SnkVersionMeta>();
            if (array.Length > 1)
            {
                for (int i = 1; i < array.Length; i++)
                {
                    infos.histories.Add(SnkVersionMeta.ValueOf(array[i]));
                }
            }

            return infos;
        }*/
    }
}    
