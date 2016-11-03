using System;
using System.Configuration;
using System.IO;
using SendCorrespondenceService.Model;
using SendCorrespondenceService.Utils;

namespace SendCorrespondenceService
{
    public class FileProcesser
    {
        FileSystemWatcher watcher;
        string directoryToWatch;

        public FileProcesser(string path)
        {
            this.watcher = new FileSystemWatcher();
            this.directoryToWatch = path;
        }

        public void Watch()
        {
            watcher.Path = directoryToWatch;
            watcher.NotifyFilter = NotifyFilters.LastAccess |
                                   NotifyFilters.LastWrite |
                                   NotifyFilters.FileName |
                                   NotifyFilters.DirectoryName;
            watcher.Filter = "*.*";
            //watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.EnableRaisingEvents = true;
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            string toPath;

            try
            {
                CreateAndSendCorrespondence(e.FullPath);

                toPath = ConfigurationManager.AppSettings["ToPath"];

                if (!Directory.Exists(toPath))
                    Directory.CreateDirectory(toPath);                
            }
            catch (Exception)
            {
                toPath = ConfigurationManager.AppSettings["ToPathFailed"];

                if (!Directory.Exists(toPath))
                    Directory.CreateDirectory(toPath);
            }

            File.Copy(e.FullPath, Path.Combine(toPath, Path.GetFileName(e.FullPath)), true);
            File.Delete(e.FullPath);



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
