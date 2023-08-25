using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using SnkFeatureKit.Patcher.Interfaces;

namespace SnkFeatureKit.Patcher
{
    namespace Implements
    {
        public class SnkDownloadTask : ISnkDownloadTask
        {
            public SNK_DOWNLOAD_STATE State { get; private set; } = SNK_DOWNLOAD_STATE.none;
            public string Name { get; set; }
            public string Url { get; set; }
            public string SavePath { get; set; }
            public long TotalSize { get; private set; }
            public long DownloadedSize { get; private set; }
            public Exception DownloadException { get; private set; }
            
            private bool _disposed = false;

            public void Cancel()
            {
                if (State == SNK_DOWNLOAD_STATE.downloading)
                    State = SNK_DOWNLOAD_STATE.canceling;
            }     
            
            public void Pause()
            {
                if (State == SNK_DOWNLOAD_STATE.downloading)
                    State = SNK_DOWNLOAD_STATE.pause;
            }

            public void Resume()
            {
                if (State == SNK_DOWNLOAD_STATE.pause)
                    State = SNK_DOWNLOAD_STATE.downloading;
            }

            public void DownloadFile(int buffSize = 65536, bool resume = false)
            {
                State = SNK_DOWNLOAD_STATE.downloading;
                FileStream fileStream = null;
                try
                {
                    var fileInfo = new FileInfo(this.SavePath);
                    if (fileInfo.Exists)
                    {
                        if (resume)
                        {
                            fileStream = fileInfo.Open(FileMode.Open, FileAccess.ReadWrite);
                            fileStream.Seek(0, SeekOrigin.End);
                        }
                        else
                            fileInfo.Delete();
                    }
                    
                    var request = WebRequest.CreateHttp(Url);
                    request.Method = "GET";

                    if (fileStream != null)
                        request.AddRange(fileStream.Length);

                    using (var response = request.GetResponse())
                    {
                        this.TotalSize = response.ContentLength;
                        var buffer = new byte[buffSize];
                        using (var responseStream = response.GetResponseStream())
                        {
                            if(fileInfo.Directory?.Exists == false)
                                fileInfo.Directory.Create();

                            using (fileStream = fileStream ?? fileInfo.Open(FileMode.Create, FileAccess.ReadWrite))
                            {
                                var len = 0;
                                while (this.State != SNK_DOWNLOAD_STATE.canceling)
                                {
                                    if(this.State == SNK_DOWNLOAD_STATE.pause)
                                        continue;
                                    
                                    if (responseStream == null)
                                        throw new ArgumentNullException($"responseStream is null. url:{Url}");

                                    if ((len = responseStream.Read(buffer, 0, buffSize)) > 0)
                                    {
                                        fileStream.Write(buffer, 0, len);
                                        this.DownloadedSize += len;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                fileStream.Flush();
                                fileStream.Close();
                            }
                        }
                    }
                }
                catch (System.Exception e)
                {
                    DownloadException = e;
                }
                finally
                {
                    this.State = SNK_DOWNLOAD_STATE.completed;
                }
            }

            public Task DownloadFileAsync(int buffSize = 1024 * 64, bool resume = false)
                => Task.Run(() => DownloadFile(buffSize, resume));

            ~SnkDownloadTask()
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

                        Url = string.Empty;
                        SavePath = string.Empty;
                        TotalSize = 0L;
                        DownloadedSize = 0L;
                        State = SNK_DOWNLOAD_STATE.none;
                    }
                }
            }
        }
    }
}