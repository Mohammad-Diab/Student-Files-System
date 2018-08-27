using System;
using System.Collections.Generic;

namespace studant_client.Controllers
{
    public class Status
    {
        public long StudentsCount { get; set; }
        public long FilesCount { get; set; }
    }

    public class Person
    {
        public static long StudantsCount = 50; 
        public long id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public DateTime birthday { get; set; }
        public string birthday_str { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
    }

    public class Student : Person
    {
        public string number { get; set; }
        public List<FileDB> files_list { get; set; }
    }

    public class FileDB
    {
        public long id { get; set; }
        public string student_id { get; set; }
        public string file_path { get; set; }
        public string file_title { get; set; }
        public DateTime uploaded_time { get; set; }
        public string description { get; set; }
        public byte[] file_data { get; set; }
    }

}