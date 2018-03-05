using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace PDF_Service.Classes
{
    public interface IFileProvider
    {
        bool Exists(string name);
        FileStream Open(string name);
        long GetLength(string name);
        string DefaultFileLocation { get; }

    }
}