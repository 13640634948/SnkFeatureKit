using System.IO;
using System.Security.Cryptography;
using System.Text;
using System;
using SnkFeatureKit.Patcher.Extension;
using SnkFeatureKit.Patcher.Interfaces;

namespace SnkFeatureKit.Patcher
{
    namespace Implements
    {
        public class SnkMD5Generator : ISnkCodeGenerator
        {
            private const long bigFileSize = 1024 * 1024 * 256;//256M
            private const int blockBuffSize = 1024 * 1024 * 2;//2M

            public string CalculateFileMD5(string filePath)
            {
                var md5 = "";

                var fileInfo = new FileInfo(filePath.FixLongPath());
                if (fileInfo.Exists == false)
                    return md5;

                using (var inputStream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (var provider = new MD5CryptoServiceProvider())
                    {
                        if (fileInfo.Length >= bigFileSize)
                            md5 = GetMD5ByHashAlgorithm(inputStream, provider);
                        else
                        {
                            byte[] buffer = provider.ComputeHash(inputStream);
                            md5 = BitConverter.ToString(buffer);
                        }
                        provider.Clear();
                    }
                    inputStream.Close();
                }
                return md5.Replace("-", string.Empty);
            }

            /// <summary>
            /// 文件生成MD5（适用大文件）
            /// </summary>
            /// <param name="filePath"></param>
            /// <returns></returns>
            private string GetMD5ByHashAlgorithm(Stream inputStream, MD5CryptoServiceProvider provider)
            {
                var buffer = new byte[blockBuffSize];
                var len = 0; //每次读取长度            
                var output = new byte[blockBuffSize];
                while ((len = inputStream.Read(buffer, 0, blockBuffSize)) > 0)
                    provider.TransformBlock(buffer, 0, len, output, 0);

                //完成最后计算，必须调用(由于上一部循环已经完成所有运算，所以调用此方法时后面的两个参数都为0)            		  
                provider.TransformFinalBlock(buffer, 0, 0);
                return BitConverter.ToString(provider.Hash);
            }

            public string CalculateContentMD5(string content)
            {
                using (MD5 md5 = MD5.Create())
                {
                    byte[] inputBytes = Encoding.UTF8.GetBytes(content);
                    byte[] hashBytes = md5.ComputeHash(inputBytes);

                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < hashBytes.Length; i++)
                        builder.Append(hashBytes[i].ToString("x2"));
                    return builder.ToString();
                }
            }

        }
    }
}