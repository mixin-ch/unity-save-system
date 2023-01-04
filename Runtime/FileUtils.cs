using System.IO;
using UnityEngine;
using Mixin.Utils;

namespace Mixin.Save
{
    public static class FileUtils
    {
        /// <summary>
        /// The Persistent Data Path.
        /// </summary>
        /// <returns>the base path where files will be saved.</returns>
        public static string GetDataSavePath()
            => Application.persistentDataPath;

        /// <summary>
        /// Get the size of a file.
        /// </summary>
        /// <param name="path">The full path with filename and extension</param>
        /// <returns></returns>
        public static long GetFileSize(string path)
            => new FileInfo(path).Length;

        /// <summary>
        /// Get the size of a file formated in human readable.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFileSizeInBytes(string path)
            => $"{GetFileSize(path).FormatThousand()} Bytes";

        public static string GetFileExtensionFromType(FileType fileType)
        {
            switch (fileType)
            {
                //case FileType.Binary:
                //    return "bin";
                case FileType.XML:
                    return "xml";
                case FileType.JSON:
                    return "json";
                default:
                    return null;
            }
        }
    }
}