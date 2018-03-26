using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DuplicateSearch;

namespace DSClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Path to directory: ");

            string path = Console.ReadLine();

            if (path != null)
            {
                DuplicateSearcher ds = new DuplicateSearcher(path);
                XMLDiscriber describer = new XMLDiscriber();
                describer.Error += Handler;
                ds.Error += Handler;

                ds.FindDuplicates();
                describer.DescribeDirectoriesTree(path);
                describer.SaveXMLDocument(path);   
            }
            

            Console.WriteLine("Done!");
            Console.ReadKey();

        }

        static void Handler(object sender, ErrorEventArgs args)
        {
            Console.WriteLine("Error: " + args.GetException().Message);
        }
    }
}
