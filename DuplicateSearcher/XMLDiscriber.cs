using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.IO;

namespace DuplicateSearch
{
    public class XMLDiscriber
    {
        public XDocument XMLDocument = null;

        public delegate void ErrorHandler(object sender, ErrorEventArgs args);
        public event ErrorHandler Error;

        public XMLDiscriber()
        {
            XMLDocument = new XDocument();
        }

        //Получение XML-структуры дерева каталогов
        public void DescribeDirectoriesTree(string path)
        {
            XElement root = new XElement("directories");

            var dirs = DescribeDirectory(path);

            if (dirs != null)
                root.Add(dirs);

            XMLDocument.Add(root);
        }

        // XML-схема текущей директории
        private XElement DescribeDirectory(string path)
        {
            XElement fileNode = null;
            XAttribute fileNameAttr = null;
            XElement currNode = null;
            try
            {
                DirectoryInfo currDir = new DirectoryInfo(path);
                currNode = new XElement("directory");
                XAttribute nameAttr = new XAttribute("name", currDir.Name);
                currNode.Add(nameAttr);

                foreach (var dir in currDir.GetDirectories())
                {
                    currNode.Add(DescribeDirectory(dir.FullName));
                }

                foreach (var file in currDir.GetFiles())
                {
                    fileNode = new XElement("file");
                    fileNameAttr = new XAttribute("name", file.Name);
                    fileNode.Add(fileNameAttr);
                    currNode.Add(fileNode);
                }
            }
            catch (Exception ex)
            {
                if (Error != null)
                    Error.Invoke(this, new ErrorEventArgs(ex));
            }

            return currNode;
        }


        //Сохранение
        public void SaveXMLDocument(string path)
        {
            FileStream stream = null;
            try
            {
                stream = File.Open(path + @"\schema.xml", FileMode.Create);
                XMLDocument.Save(stream);
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
        }
    }
}
