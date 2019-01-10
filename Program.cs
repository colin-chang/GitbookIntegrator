using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Integrator
{
    class Program
    {
        #region const variables

        private const string TargetBooksPath = "/books";
        private const string GitbookPath = "/_book";
        private const string TargetSiteMapPath = "/pages/pages-root-folder/sitemap.xml";
        private const string GitbookSiteMapPath = "/_book/sitemap.xml";

        private const string ChangeFreq = "monthly";
        private const string Priority = "0.6";

        #endregion


        static void Main(string[] args)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && !RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Console.WriteLine($"Sorry,we don't support your OS yet!");
                return;
            }

            PublishBooks();
        }

        private static void PublishBooks()
        {
            var books = GetBooks();
            if (books == null || books.Length <= 0)
            {
                Console.WriteLine("Warn: You didn't select any valid books.");
                return;
            }

            var siteMap = GetSiteMap();


            var watch = new Stopwatch();
            watch.Start();
            Console.WriteLine("Start publishing ...");
            double totalElapsed = 0;

            var summary = new List<string>();
            int succeed = 0, failed = 0;
            foreach (var book in books)
            {
                var bookName = book.GetFileName();
                if (PublishOneBook(book, siteMap))
                {
                    succeed++;
                    summary.Add($"{bookName}\tsucceed\t{watch.Elapsed.TotalSeconds}");
                }
                else
                {
                    failed++;
                    summary.Add($"{bookName}\tfailed\t{watch.Elapsed.TotalSeconds}");
                }

                totalElapsed += watch.Elapsed.TotalSeconds;
                watch.Restart();
            }

            watch.Restart();
            Console.WriteLine("Book\tStatus\tElapsed");
            Console.WriteLine(string.Join("\r\n", summary));
            Console.WriteLine(
                $"Finish publishing.Total {books.Length} books,{succeed} succeed,{failed} failed. Total {totalElapsed} seconds");

            Push2Git();
        }

        private static string[] GetBooks()
        {
            var projectDir = AppDomain.CurrentDomain.BaseDirectory.ParentPathUtil(3);
            var booksPath = projectDir.ParentPathUtil(1);
            projectDir = projectDir.GetFileName();

            Console.WriteLine($"Please enter the books directory. [Default: {booksPath}]");
            var input = Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(input))
                booksPath = input;


            Console.WriteLine($"Please select which book you wanna publish? [Default: all]");
            var books = Directory.GetDirectories(booksPath)
                .Where(b => !b.GetFileName().Contains("github.io") && !b.GetFileName().Equals(projectDir)).ToArray();
            for (var i = 0; i < books.Length; i++)
                Console.WriteLine($"[{i}]\t{books[i].GetFileName()}");
            Console.WriteLine($"[{books.Length}]\tall");
            var bno = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(bno) || !int.TryParse(bno, out var no) || no >= books.Length)
                return books;

            return new string[] {books[no]};
        }

        private static string GetSiteMap()
        {
            var webPath = AppDomain.CurrentDomain.BaseDirectory.ParentPathUtil(4);
            webPath = Directory.GetDirectories(webPath).FirstOrDefault(d => d.EndsWith("github.io"));

            do
            {
                Console.WriteLine(
                    $"Please enter the website directory. {(webPath == null ? string.Empty : $"[Default: {webPath}]")}");
                var input = Console.ReadLine()?.Trim();
                if (!string.IsNullOrWhiteSpace(input))
                    webPath = input;
            } while (string.IsNullOrWhiteSpace(webPath) ||
                     !File.Exists(GetSiteMap()));

            string GetSiteMap() => webPath + TargetSiteMapPath;

            return GetSiteMap();
        }

        private static bool PublishOneBook(string book, string siteMap)
        {
            try
            {
                //build book
                var success = ShellUtil.ExecShell("publish_build_osx.sh", book, true);
                if (!success)
                    return false;

                //read new items
                var bookName = book.GetFileName();
                var map = book + GitbookSiteMapPath;
                if (!File.Exists(map))
                {
                    Console.WriteLine($"Warn. {bookName} has no sitemap.xml");
                    return true;
                }

                var root = XDocument.Load(map).Root;
                var xmlns = root?.Attribute("xmlns");
                string GetXElementFullName(string shortName) => $"{{{xmlns?.Value}}}{shortName}";
                var urls = root?.Elements(GetXElementFullName("url"));
                if (urls == null || !urls.Any())
                    return true;


                //clear history
                var lines = File.ReadAllLines(siteMap).ToList();
                var begin = lines.IndexOf(lines.FirstOrDefault(l => l.Contains($"{bookName} begin")));
                var end = lines.IndexOf(lines.FirstOrDefault(l => l.Contains($"{bookName} end")));
                if (begin > 0 && end > begin)
                    lines.RemoveRange(begin, end - begin + 1);


                //insert new items
                begin = begin > 0 ? begin : lines.IndexOf(lines.FirstOrDefault(l => l.Contains($"</urlset>")));
                lines.Insert(begin,
                    $"<!-- {bookName} begin: {urls.Count() - 1} links with priority=0.6, changefreq=monthly -->");
                for (var i = 1; i < urls.Count(); i++)
                {
                    //edit new item
                    var url = urls.ElementAt(i);
                    var loc = url.Element(GetXElementFullName("loc"));
                    loc?.SetValue(Regex.Replace(loc?.Value, "(https?://.+?)/", $"$1{TargetBooksPath}/{bookName}/"));
                    url.Element(GetXElementFullName("changefreq"))?.SetValue(ChangeFreq);
                    url.Element(GetXElementFullName("priority"))?.SetValue(Priority);

                    lines.Insert(begin + i,
                        url.ToString().Replace(" " + xmlns?.ToString(), string.Empty));
                }

                lines.Insert(begin + urls.Count(), $"<!-- {bookName} end -->");
                File.WriteAllLines(siteMap, lines);

                //update new book files
                var bookPath = siteMap.ParentPathUtil(3) + TargetBooksPath + "/" + bookName;
                success = ShellUtil.ExecShell("publish_cpfiles_osx.sh",
                    $"{map} {bookPath} {book + GitbookPath} {bookPath}", true);

                return success;
            }
            catch
            {
                return false;
            }
        }

        private static void Push2Git()
        {
            Console.WriteLine("Would you like to update changes to git? y(yes)|n(no) [Default:yes]");
            var answer = Console.ReadLine()?.Trim()?.ToLower();
            if (string.Equals(answer, "n") || string.Equals(answer, "no"))
                return;

            Console.WriteLine("Please enter your changes log.");
            var msg= Console.ReadLine()?.Trim();
            ShellUtil.ExecShell("publish_git_osx.sh", msg, true);
        }
    }
}