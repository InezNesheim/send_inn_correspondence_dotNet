using System;
using System.Configuration;
using System.IO;
using SendCorrespondenceService.Model;
using SendCorrespondenceService.Utils;

namespace SendCorrespondenceService
{
    public class FileProcesser
    {
        readonly FileSystemWatcher watcher;
        readonly string directoryToWatch;

        public FileProcesser(string path)
        {
            watcher = new FileSystemWatcher();
            directoryToWatch = path;
        }

        public void Watch()
        {
            watcher.Path = directoryToWatch;
            watcher.NotifyFilter = NotifyFilters.LastAccess |
                                   NotifyFilters.LastWrite |
                                   NotifyFilters.FileName |
                                   NotifyFilters.DirectoryName;
            watcher.Filter = "*.*";            
            watcher.Created += new FileSystemEventHandler(OnCreated);
            watcher.EnableRaisingEvents = true;
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            string toPath;

            try
            {
                CreateAndSendCorrespondence(e.FullPath);

                toPath = ConfigurationManager.AppSettings["ToPath"];

                if (!Directory.Exists(toPath))
                    Directory.CreateDirectory(toPath);
            }
            catch (Exception exception)
            {
                Logger.Log($"Failed to process Altinn batch. File will be moved to proper directory. Message: {exception.Message}");

                toPath = ConfigurationManager.AppSettings["ToPathFailed"];

                if (!Directory.Exists(toPath))
                    Directory.CreateDirectory(toPath);
            }

            try
            {
                if (e.FullPath == null) return;

                File.Copy(e.FullPath, Path.Combine(toPath, Path.GetFileName(e.FullPath)), true);
                File.Delete(e.FullPath);
            }
            catch (Exception exception)
            {                
                Logger.Log($"Failed to move (copy/delete) processed file to target location. Error message: {exception.Message}");
            }            
        }

        private void CreateAndSendCorrespondence(string filename)
        {
            DataBatch batch = LoadBatch(filename);
            foreach (DataUnit dataunit in batch.DataUnits)
            {
                SendCorrespondenceDal.CreateAndSendCorrespondence(dataunit.archiveReference, dataunit.reportee);
            }
        }

        private DataBatch LoadBatch(string fileName)
        {             
            var serializer = XmlUtils.GetXmlSerializerOfType<Payload>();

            var payload = XmlUtils.DeserializeXmlString<Payload>(serializer, File.ReadAllText(fileName));

            serializer = XmlUtils.GetXmlSerializerOfType<DataBatch>();

            return XmlUtils.DeserializeXmlString<DataBatch>(serializer, payload.Batch);
        }       
    }
}
