using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace ScriptsDomain.Middleware
{
    public class ScriptTemplatingFileProvider : IFileProvider
    {
        private readonly PathString _rootPath;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly Dictionary<Regex, Func<string, string>> _mapper;

        public ScriptTemplatingFileProvider(string rootPath, Dictionary<string, Func<string, string>> mapper, IHostingEnvironment hostingEnvironment)
        {
            _rootPath = new PathString(rootPath);
            _hostingEnvironment = hostingEnvironment;
            _mapper = mapper.ToDictionary(kvp => new Regex(@"\<%=\s*" + kvp.Key + @"\s%\>"), kvp => kvp.Value);
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            var path = _rootPath.Add(subpath);
            var fileInfo = _hostingEnvironment.WebRootFileProvider.GetFileInfo(path);
            if (!fileInfo.Exists || fileInfo.IsDirectory)
            {
                return fileInfo;
            }

            return new TemplatingAwareFileInfo(fileInfo, _mapper);
        }

        public class TemplatingAwareFileInfo : IFileInfo
        {
            private byte[] _buffer;
            private readonly IFileInfo _fileInfo;
            private readonly Dictionary<Regex, Func<string, string>> _mapper;

            public TemplatingAwareFileInfo(IFileInfo fileInfo, Dictionary<Regex, Func<string, string>> mapper)
            {
                _fileInfo = fileInfo;
                _mapper = mapper;
            }

            private byte[] GetBuffer()
            {
                if (_buffer == null)
                {
                    var res = new MemoryStream();
                    var outs = new StreamWriter(res);

                    using (var ins = new StreamReader(_fileInfo.CreateReadStream(), Encoding.UTF8))
                    {
                        var s = ins.ReadLine();
                        while (s != null)
                        {
                            foreach (var kvp in _mapper)
                            {
                                var pattern = kvp.Key;
                                if (pattern.IsMatch(s))
                                {
                                    var replacer = kvp.Value;
                                    s = pattern.Replace(s, match => replacer(match.Value));
                                }
                            }

                            outs.WriteLine(s);
                            s = ins.ReadLine();
                        }
                    }

                    outs.Flush();
                    _buffer = new byte[res.Length];
                    res.Position = 0;
                    res.Read(_buffer, 0, _buffer.Length);
                }

                return _buffer;
            }

            public Stream CreateReadStream()
            {
                return new MemoryStream(GetBuffer());
            }

            public bool Exists => _fileInfo.Exists;
            public long Length => GetBuffer().Length;
            public string PhysicalPath => _fileInfo.PhysicalPath;
            public string Name => _fileInfo.Name;
            public DateTimeOffset LastModified => _fileInfo.LastModified;
            public bool IsDirectory => _fileInfo.IsDirectory;
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            throw new NotImplementedException();
        }

        public IChangeToken Watch(string filter)
        {
            throw new NotImplementedException();
        }
    }
}