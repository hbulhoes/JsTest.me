using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace ScriptsDomain.Middleware
{
    public class ScriptTemplatingFileProvider : IFileProvider
    {
        private IHostingEnvironment _hostingEnvironment;
        private Dictionary<Regex, Func<string, string>> _mapper;

        public ScriptTemplatingFileProvider(Dictionary<string, Func<string, string>> mapper, IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            _mapper = mapper.ToDictionary(kvp => new Regex(@"\<%=\s*" + kvp.Key + @"\s%\>"), kvp => kvp.Value);
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            var fileInfo = _hostingEnvironment.WebRootFileProvider.GetFileInfo(subpath);
            if (!fileInfo.Exists || fileInfo.IsDirectory)
            {
                return fileInfo;
            }

            return new TemplatingAwareFileInfo(fileInfo);
        }

        public class TemplatingAwareFileInfo : IFileInfo
        {
            private readonly IFileInfo _fileInfo;

            public TemplatingAwareFileInfo(IFileInfo fileInfo)
            {
                _fileInfo = fileInfo;
            }

            public Stream CreateReadStream()
            {
                throw new NotImplementedException();
            }

            public bool Exists => _fileInfo.Exists;
            public long Length => _fileInfo.Length;
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