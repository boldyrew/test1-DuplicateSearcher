using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Linq;

namespace DuplicateSearch
{
    public class DuplicateSearcher
    {
        public string RootPath { get; set; }

        public string CopyDirPath
        {
            get { return RootPath + @"\Copy"; }
        }

        //Алгоритм хеширования для сравнения по содержимому
        private MD5 MD5;

        //Список хешей и имен, считающееся оригинальными файлами (найденные первыми)
        public List<string> OriginalFileHashes = null;
        public List<string> OriginalFileNames = null;

        //Словарь названий файлов-дубликатов, счетчик кол-ва дубликатов
        private Dictionary<string, int> DuplicatesByFileName = null;

        //Обработка ошибок
        public delegate void ErrorHandler(object sender, ErrorEventArgs e);
        public event ErrorHandler Error;

        public DuplicateSearcher(string path)
        {
            RootPath = path;
            OriginalFileNames = new List<string>();
            OriginalFileHashes = new List<string>();
            DuplicatesByFileName = new Dictionary<string, int>();
            MD5 = MD5.Create();
        }

        public void FindDuplicates()
        {
            try
            {
                DirectoryInfo root = new DirectoryInfo(RootPath);
                // Создание папки COPY в корне
                if (!Directory.Exists(CopyDirPath))
                    Directory.CreateDirectory(CopyDirPath);
                //Рекурсивный обход дерева каталогов
                CheckDirectoriesTree(root);
            }
            catch (Exception ex)
            {
                if (Error != null)
                    Error.Invoke(this, new ErrorEventArgs(ex));
            }
        }

        //Рекурсивный обход дерева папок
        private void CheckDirectoriesTree(DirectoryInfo root)
        {
            //Не просматривать папку COPY
            if (root.FullName == CopyDirPath) return;

            try
            {
                //Поиск дубликатов в текущей папке
                CheckCurrentDirectory(root);
                var dirs = root.GetDirectories();
                
                foreach (var dir in dirs)
                {
                    CheckDirectoriesTree(dir);
                }
            }
            catch (Exception ex)
            {
                if (Error != null)
                    Error.Invoke(this, new ErrorEventArgs(ex));
            }
        }

        //Поиск дубликатов в текущей папке
        private void CheckCurrentDirectory(DirectoryInfo dir)
        {
            string hashStr = null;
            try
            {
                foreach (var file in dir.GetFiles())
                {
                        //Получение хеша содержимого
                        hashStr = GetFileHashString(file);
                        if (OriginalFileHashes.Contains(hashStr) || OriginalFileNames.Contains(file.Name))
                            MoveFile(file);
                        else
                        {
                            //Если файл найден впервые, он считается оригиналом, все последующие обьявляются дубликатами
                            OriginalFileHashes.Add(hashStr);
                            OriginalFileNames.Add(file.Name);
                        }
                }
            }
            catch (Exception ex)
            {
                if (Error != null)
                    Error.Invoke(this, new ErrorEventArgs(ex));
            }
        }

        //Перенос дубликата в папку COPY
        private void MoveFile(FileInfo file)
        {
            if (!DuplicatesByFileName.ContainsKey(file.Name))
                DuplicatesByFileName.Add(file.Name, 1);
            //Если в папке уже находится дубликат с именем текущего файла, получить новое имя
            File.Move(file.FullName, GetDuplicateNewPath(file));           
        }


        //Новое имя дубликата
        //Ориг. имя + COPY + номер копии + расширение
        private string GetDuplicateNewPath(FileInfo file)
        {
            string nCopy = "";
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.FullName);
            var extension = Path.GetExtension(file.FullName);

            if (DuplicatesByFileName.ContainsKey(file.Name))
                nCopy = (DuplicatesByFileName[file.Name]++).ToString();

            return string.Format(@"{0}\{1}-COPY{2}{3}", CopyDirPath, fileNameWithoutExtension, nCopy, extension);
        }

        //Получение хеша содержимого
        private string GetFileHashString(FileInfo file)
        {
            string hashStr = null;
            FileStream stream = null;
            try
            {
                stream = file.OpenRead();
                hashStr = string.Join("", MD5.ComputeHash(stream));
            }
            catch (Exception ex)
            {
                if (Error != null)
                    Error.Invoke(this, new ErrorEventArgs(ex));
            }
            finally
            {
                stream.Close();
            }
            

            return hashStr;
        }
    }
}
