using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using System.Runtime.Serialization;
using Mixin.Utils;
using Newtonsoft.Json;

namespace Mixin.Save
{
    /// <summary>
    /// Manages data from a specific file.
    /// </summary>
    [Serializable]
    public class DataFileManager<DataFileT>
    {
        /// <summary>
        /// The assosiated data. 
        /// In order to make changes, you need to write it directly to this object.
        /// </summary>
        private DataFileT _data;

        /// <summary>
        /// The File Name with extension.
        /// </summary>
        protected string _fileName = "default.json";
        protected FileType _fileType = FileType.JSON;
        protected bool _useEncryption;
        protected string _salt;

        /// <summary>
        /// <inheritdoc cref="_data"/>
        /// </summary>
        public DataFileT Data { get => _data; set => _data = value; }

        public event Action OnBeforeSave;
        public event Action OnBeforeLoad;
        public event Action OnBeforeDelete;

        /// <summary>
        /// Called after trying to save file. Returns true if successful.
        /// </summary>
        public event Action<bool> OnAfterSave;
        /// <summary>
        /// Called after trying to load file. Returns true if successful.
        /// </summary>
        public event Action<bool> OnAfterLoad;
        /// <summary>
        /// Called after trying to delete file. Returns true if successful.
        /// </summary>
        public event Action<bool> OnAfterDelete;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileType"></param>
        /// <param name="salt">Default is set to null. When entering a salt, 
        /// it will automatically encrypt your data.</param>
        public DataFileManager(string fileName, FileType fileType, string salt = null)
        {
            _fileName = fileName;
            _fileType = fileType;
            _salt = salt;

            if (salt != null)
                _useEncryption = true;
        }

        /// <summary>
        /// Save data to file.
        /// Calls onDataSaved.
        /// Invokes OnDataSaved.
        /// </summary>
        public void Save()
        {
            OnBeforeSave?.Invoke();

            bool success = false;

            // Overwrite the existing file or create a new one.
            FileStream fileStream = new FileStream(GetFullFilePath(), FileMode.Create);

            object dataToWrite = _data;

            if (_useEncryption)
                dataToWrite = Encrypter.Encrypt(JsonUtility.ToJson(_data), _salt);

            // Save data depending on FileType.
            switch (_fileType)
            {
                case FileType.XML:
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(DataFileT));
                    xmlSerializer.Serialize(fileStream, dataToWrite);
                    fileStream.Close();
                    break;
                case FileType.JSON:
                    fileStream.Close();
                    string json = JsonConvert.SerializeObject(dataToWrite);
                    File.WriteAllText(GetFullFilePath(), json);
                    break;
                default:
                    throw new Exception($"FileType '{_fileType}' is not defined. Save process is canceled");
            }

            success = true;
            $"Saving {GetColorizedFileName()} was successful.".Log();
            OnAfterSave?.Invoke(success);
        }

        /// <summary>
        /// Load data from file.
        /// Calls onDataLoaded.
        /// Invokes OnDataLoaded.
        /// </summary>
        public void Load()
        {
            $"Loading {GetFullFilePath().Colorize(Color.cyan)}".LogProgress();
            OnBeforeLoad?.Invoke();

            bool success = false;

            // Check if file exists. Return if it does not exist.
            if (!ThisFileExists())
            {
                $"The file {GetFullFilePath()} does not exist.".LogWarning();
                return;
            }

            // Open existing file.
            DataFileT loadedData = default;
            string loadedEncryptedData = "";

            try
            {
                // Load data depending on FileType.
                switch (_fileType)
                {
                    case FileType.XML:
                        // Open file
                        FileStream fileStream = new FileStream(GetFullFilePath(), FileMode.Open);

                        // Write file
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(DataFileT));
                        if (_useEncryption)
                            loadedEncryptedData = (string)xmlSerializer.Deserialize(fileStream);
                        else
                            loadedData = (DataFileT)xmlSerializer.Deserialize(fileStream);

                        // Close file
                        fileStream.Close();
                        break;
                    case FileType.JSON:
                        string jsonText = File.ReadAllText(GetFullFilePath());
                        if (_useEncryption)
                            loadedEncryptedData = JsonConvert.DeserializeObject<string>(jsonText);
                        else
                            loadedData = JsonConvert.DeserializeObject<DataFileT>(jsonText);
                        break;
                    default:
                        throw new Exception($"FileType '{_fileType}' is not defined. Save process is canceled");
                }

                success = true;
            }

            catch (SerializationException e) // Catch Errors
            {
                $"Could not Serialize FileData. Error: {e}".LogError();
                success = false;
            }
            catch (Exception e) // Catch Errors
            {
                $"Error Loading Data: {e}".LogError();
                success = false;
            }

            // When using encryption then decrypt it here.
            if (_useEncryption && success)
                loadedData = JsonUtility.FromJson<DataFileT>(Encrypter.Decrypt(loadedEncryptedData, _salt));

            if (success)
                _data = loadedData;

            $"File {GetColorizedFileName()} successfully loaded".Log(Color.yellow);
            OnAfterLoad?.Invoke(success);
        }

        /// <summary>
        /// Delete file and clears data.
        /// Calls onDataDeleted.
        /// Invokes OnDataDeleted.
        /// </summary>
        public void Delete()
        {
            OnBeforeDelete?.Invoke();

            bool success = false;

            // File does exist.
            if (ThisFileExists())
            {
                File.Delete(GetFullFilePath());
                success = true;
            }
            // File does not exist.
            else
                $"The file {GetFullFilePath()} does not exist.".LogWarning();

            _data = default;
            $"File {_fileName} got deleted.".Log(Color.red);
            OnAfterDelete?.Invoke(success);
        }

        /// <summary>
        /// Returns the full file path including the filename and extension.
        /// </summary>
        /// <returns></returns>
        protected string GetFullFilePath()
        {
            return Path.Combine(FileUtils.GetDataSavePath(), _fileName);
        }

        protected bool ThisFileExists()
        {
            return File.Exists(GetFullFilePath());
        }

        private string GetColorizedFileName()
        {
            return _fileName.Colorize(Color.cyan);
        }
    }
}