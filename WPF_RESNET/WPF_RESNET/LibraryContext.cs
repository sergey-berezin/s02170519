using Microsoft.EntityFrameworkCore;
using MyLibrary;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WPF_RESNET
{
    public class Type
    {
        public int Id { get; set; }
        public string TypeName { get; set; }
        public ICollection<File> Files { get; set; }        
    }
    public class FileView
    {
        public string TypeName { get; set; }
        public string Image { get; set; }
        public int NumberOfRequests { get; set; }
        public FileView (string typeName, string image, int count)
        {
            Image = image;
            TypeName = typeName;
            NumberOfRequests = count;
        }
    }    
    public class File
    {
        public int Id { get; set; }
        public int Hash { get; set; }
        public int NumberOfRequests { get; set; }
        public string Path { get; set; }
        public FileDetails FileDetails { get; set; }
        public Type Type { get; set; }
    }
    public class FileDetails
    {
        public int Id { get; set; }
        public byte[] Data { get; set; }
    }
    public class LibraryContext : DbContext
    {
        public DbSet<File> Files { get; set; }
        public DbSet<Type> Types { get; set; }
        public DbSet<FileDetails> FileDetails { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=../../../library.db");
        }
        public void Clear()
        {
            foreach(var i in Files)
            {
                Files.Remove(i);
            }
            foreach (var i in Types)
            {
                Types.Remove(i);
            }
            foreach (var i in FileDetails)
            {
                FileDetails.Remove(i);
            }
            SaveChanges();
        }
        public FileView GetFile(string fileName)
        {
            FileView file  = null;
            var binary = System.IO.File.ReadAllBytes(fileName);
            var hash = new BigInteger(binary).GetHashCode();
            foreach (var f in Files.Include(a => a.Type).Where(a => a.Hash == hash))
            {
                Entry(f).Reference("FileDetails").Load();
                //f.Type = new Type();
                if (binary.Length != f.FileDetails.Data.Length)
                    continue;
                bool flag = false;
                for (int i = 0; i < binary.Length; i++)
                    if (binary[i] != f.FileDetails.Data[i])
                    {
                        flag = true;
                        break;
                    }
                if (flag)
                    continue;

                file = new FileView(f.Type.TypeName, f.Path, f.NumberOfRequests + 1);
                f.NumberOfRequests++;
                SaveChanges();
                break;
            }
            return file;
        }
        public FileView AddResNetResult(ResNetResult result)
        {
            var logo = System.IO.File.ReadAllBytes(result.FileName);
            var query = Types.Include(a => a.Files).Where(a => a.TypeName == result.Label);

            var f = new File() { Hash = new BigInteger(logo).GetHashCode() };
            f.FileDetails = new FileDetails() { Data = logo };
            f.NumberOfRequests = 1;
            f.Path = result.FileName;

            if (query.Count() == 0) //если новый класс
            {
                f.Type = new Type() { TypeName = result.Label };
                f.Type.Files = new List<File>();
                f.Type.Files.Add(f);

                Files.Add(f);
                Types.Add(f.Type);
                FileDetails.Add(f.FileDetails);
            }
            else
            {
                f.Type = query.First();
                f.Type.Files.Add(f);

                Files.Add(f);
                FileDetails.Add(f.FileDetails);
            }
            SaveChanges();
            return new FileView(result.Label, result.FileName, f.NumberOfRequests);
        }
    }
}
