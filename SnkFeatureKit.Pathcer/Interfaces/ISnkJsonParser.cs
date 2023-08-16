using System;

namespace SnkFeatureKit.Patcher
{
    public interface ISnkJsonParser
    {
        T FromJson<T>(string json) where T : class;
        object FromJson(string json, Type objType);
        string ToJson(object target);
    }
}