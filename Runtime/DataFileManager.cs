using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using UnityEngine;
using System.Runtime.Serialization;
using Mixin.Utils;

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
        /// The File Name without extension.
        /// </summary>
        protected string _fileName = "default";
        protected FileType _fileType = FileType.Binary;
        protected string _fileVersion;
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
        public DataFileManager(string fileName, FileType fileType, string fileVersion, string salt = null)
        {
            _fileName = fileName;
            _fileType = fileType;
            _fileVersion = fileVersion;
            _salt = salt;

            if (salt != string.Empty)
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

            // Create file if it does not exist, or open existing file.
            FileStream fileStream = new FileStream(GetFileNameWithPathAndExtension(), FileMode.Create);

            object dataToWrite = _data;

            if (_useEncryption)
                dataToWrite = Encrypter.Encrypt(JsonUtility.ToJson(_data), _salt);

            // Save data depending on FileType.
            switch (_fileType)
            {
                case FileType.Binary:
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    binaryFormatter.Serialize(fileStream, dataToWrite);
                    break;
                case FileType.XML:
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(DataFileT));
                    xmlSerializer.Serialize(fileStream, dataToWrite);
                    break;
                default:
                    throw new Exception($"FileType '{_fileType}' is not defined. Save process is canceled");
            }

            fileStream.Close();

            success = true;
            OnAfterSave?.Invoke(success);
        }

        /// <summary>
        /// Load data from file.
        /// Calls onDataLoaded.
        /// Invokes OnDataLoaded.
        /// </summary>
        public void Load()
        {
            Load(null);
        }

        /// <summary>
        /// Load data from file.
        /// Calls onDataLoaded.
        /// Invokes OnDataLoaded.
        /// </summary>
        public void Load(SerializationBinder serializationBinder)
        {
            $"Loading {GetFileNameWithPathAndExtension()}".LogProgress();
            OnBeforeLoad?.Invoke();

            bool success = false;

            
            if (!ThisFileExists()) // File does not exist.
            {
                $"The file {GetFileNameWithPathAndExtension()} does not exist.".LogWarning();
            }
            else // File does exist.
            {
                // Open existing file.
                FileStream fileStream = new FileStream(GetFileNameWithPathAndExtension(), FileMode.Open);
                DataFileT loadedData = default;
                string loadedEncryptedData = "";

                try
                {
                    // Load data depending on FileType.
                    switch (_fileType)
                    {
                        case FileType.Binary:
                            BinaryFormatter binaryFormatter = new BinaryFormatter();
                            if (serializationBinder != null)
                                binaryFormatter.Binder = serializationBinder;
                            if (_useEncryption)
                                loadedEncryptedData = (string)binaryFormatter.Deserialize(fileStream);
                            else
                                loadedData = (DataFileT)binaryFormatter.Deserialize(fileStream);
                            break;
                        case FileType.XML:
                            XmlSerializer xmlSerializer = new XmlSerializer(typeof(DataFileT));
                            if (_useEncryption)
                                loadedEncryptedData = (string)xmlSerializer.Deserialize(fileStream);
                            else
                                loadedData = (DataFileT)xmlSerializer.Deserialize(fileStream);
                            break;
                        default:
                            throw new Exception($"FileType '{_fileType}' is not defined. Save process is canceled");
                    }

                    success = true;
                }

                catch (SerializationException)
                {
                    $"Could not Serialize FileData.".LogError();
                    success = false;
                }
                catch (Exception)
                {
                    success = false;
                }

                if (_useEncryption && success)
                    loadedData = JsonUtility.FromJson<DataFileT>(Encrypter.Decrypt(loadedEncryptedData, _salt));

                fileStream.Close();

                if (success)
                    _data = loadedData;
            }

            $"File {GetFileNameWithExtension()} successfully loaded".Log(Color.yellow);
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
                File.Delete(GetFileNameWithPathAndExtension());
                success = true;
            }
            // File does not exist.
            else
                $"The file {GetFileNameWithPathAndExtension()} does not exist.".LogWarning();

            _data = default;
            OnAfterDelete?.Invoke(success);
        }

        protected string GetFileNameWithExtension()
        {
            return $"{_fileName}.{FileUtils.GetFileExtensionFromType(_fileType)}";
        }

        protected string GetFileNameWithPathAndExtension()
        {
            return $"{FileUtils.GetDataSavePath()}/{GetFileNameWithExtension()}";
        }

        protected bool ThisFileExists()
        {
            return FileUtils.FileExists(GetFileNameWithPathAndExtension());
        }
    }
}