using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace student_api.Controllers
{
    public class Status
    {
        public long StudentsCount { get; set; }
        public long FilesCount { get; set; }
    }

    public class Person
    {
        public long id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public DateTime birthday { get; set; }
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

    public class ValuesController : ApiController
    {
        const string ConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;" +
            "Initial Catalog=students;" + "Integrated Security=True;" +
            "Connect Timeout=30;" + "Encrypt=False;" + "TrustServerCertificate=False;" +
            "ApplicationIntent=ReadWrite;" + "MultiSubnetFailover=False";
        //const string ConnectionString = @"data source=.\SQLEXPRESS;Integrated Security=SSPI;AttachDBFilename=|DataDirectory|aspnetdb.mdf;User Instance=true";
        int page_size = 2;
        string initial_file_path = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/Files") + @"\";
        string files_table_name = "files";
        string students_table_name = "studends";


        // Get Status Function

        [NonAction]
        public long GetStudentCount()
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                try
                {
                    string queryString = "SELECT COUNT(Id) FROM students";
                    SqlCommand command = new SqlCommand(queryString);
                    command.Connection = con;
                    command.CommandType = CommandType.Text;
                    con.Open();
                    var value = command.ExecuteScalar();
                    return (int)value;
                }
                catch (Exception ex) { return 0; }
                finally { con.Close(); }
            }
        }

        [NonAction]
        public long GetFilesCount()
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                try
                {
                    string queryString = "SELECT COUNT(Id) FROM files";
                    SqlCommand command = new SqlCommand(queryString);
                    command.Connection = con;
                    command.CommandType = CommandType.Text;
                    con.Open();
                    var value = command.ExecuteScalar();
                    return (int)value;
                }
                catch (Exception) { return 0; }
                finally { con.Close(); }
            }
        }

        [HttpGet]
        [Route("api/values/status")]
        public Status GetStatus()
        {
            long studant_count = GetStudentCount();
            long files_count = GetFilesCount();
            return new Status() { FilesCount = files_count, StudentsCount = studant_count };
        }


        //Read All studants

        [NonAction]
        public int ReadPage(int page, out List<Person> result)
        {
            int error_code = 200;
            result = new List<Person>();
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                try
                {
                    string queryString = "SELECT * FROM students ORDER BY Id Offset " + (page - 1) * page_size + " ROWS FETCH NEXT " + page_size + " ROWS ONLY;";
                    SqlCommand command = new SqlCommand(queryString);
                    command.Connection = con;
                    command.CommandType = CommandType.Text;
                    con.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        result.Add(new Person
                        {
                            id = Convert.ToInt64(reader["Id"]),
                            first_name = reader["first_name"].ToString(),
                            last_name = reader["last_name"].ToString(),
                            address = reader["address"].ToString(),
                            email = reader["email"].ToString(),
                            phone = reader["phone"].ToString(),
                            birthday = reader.GetDateTime(reader.GetOrdinal("bairthday"))
                        });
                    }

                }
                catch (Exception) { error_code = 401; }
                finally { con.Close(); }
            }
            return error_code;
        }

        [HttpGet]
        [Route("api/values/students/page{page:int?}")]
        public IHttpActionResult GetAllStudent(int? page)
        {
            if (!page.HasValue || page < 1)
                page = 1;
            List<Person> result;
            int error_code = ReadPage(page.Value, out result);
            if (error_code == 200)
                return Ok(result);
            //throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "Item not Found"));
            return NotFound();
        }

        [HttpGet]
        [Route("api/values/students")]
        public List<Person> GetAllStudent() //page 1
        {
            List<Person> result;
            int error_code = ReadPage(1, out result);
            if (error_code == 200)
                return result;
            throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "Item not Found"));
        }


        //Read student info

        [NonAction]
        public int GetStudentInfo(long student_id, out Student result)
        {
            int error_code = 200;
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                result = new Student();
                try
                {
                    string queryString = string.Format("SELECT * FROM students WHERE Id='{0}'", student_id);
                    SqlCommand command = new SqlCommand(queryString, con);
                    con.Open();
                    var reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        reader.Read();
                        result = new Student()
                        {
                            files_list = new List<FileDB>(),
                            id = student_id,
                            number = reader["Number"].ToString(),
                            first_name = reader["first_name"].ToString(),
                            last_name = reader["last_name"].ToString(),
                            address = reader["address"].ToString(),
                            email = reader["email"].ToString(),
                            phone = reader["phone"].ToString(),
                            birthday = reader.GetDateTime(reader.GetOrdinal("bairthday"))
                        };
                        return GetStudentFiles(student_id, result);
                    }
                    error_code = 400;
                }
                catch (Exception ex) { error_code = 401; }
                finally { con.Close(); }
            }
            return error_code;
        }

        [NonAction]
        public int GetStudentFiles(long student_id, Student student)
        {
            int exit_code = 200;
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                try
                {
                    con.Open();
                    string queryString = string.Format("SELECT * FROM files WHERE student_id='{0}'", student_id);
                    SqlCommand command = new SqlCommand(queryString, con);
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        student.files_list.Add(new FileDB
                        {
                            id = Convert.ToInt64(reader["Id"]),
                            file_title = reader["title"].ToString(),
                            student_id = reader["student_id"].ToString(),
                            description = reader["description"].ToString(),
                            file_path = reader["file_path"].ToString(),
                            uploaded_time = reader.GetDateTime(reader.GetOrdinal("upladed_date")),
                        });
                    }
                }
                catch (Exception ex) { exit_code = 0; }
                finally { }
            }
            return exit_code;
        }

        [HttpGet]
        [Route("api/values/student/{studant_id:long?}")]
        public Student GetStudentInfo(long? studant_id)
        {
            if (studant_id.HasValue)
            {
                Student result;
                GetStudentInfo(studant_id.Value, out result);
                return result;
            }
            return null;
        }


        //Add new studant

        [NonAction]
        int InsertNewStudent(Student item)
        {
            int error_code = 200;
            if (item.address != null &&
                item.birthday != null &&
                item.email != null &&
                item.first_name != null &&
                item.last_name != null &&
                item.number != null &&
                item.phone != null)
            {
                long num;
                if (long.TryParse(item.number, out num))
                {
                    using (SqlConnection con = new SqlConnection(ConnectionString))
                    {
                        try
                        {
                            string queryString = string.Format("INSERT INTO students " +
                                "(first_name, last_name, Number, bairthday, email, phone, address) Values " +
                                "('{0}','{1}','{2}','{3}','{4}','{5}','{6}')", item.first_name,
                                item.last_name, item.number, item.birthday, item.email, item.phone, item.address);
                            SqlCommand command = new SqlCommand(queryString, con);
                            con.Open();
                            var reader = command.ExecuteNonQuery();
                            if (item.files_list != null)
                            {
                                foreach (var i in item.files_list)
                                    InsertNewFile(i);
                            }
                        }

                        catch (Exception ex) { error_code = 401; }
                        finally { con.Close(); }
                    }
                }
                error_code = 501;
            }
            return error_code;
        }

        [HttpPost]
        [Route("api/values/add student")]
        public int AddStudent([FromBody]Student st)
        {
            InsertNewStudent(st);
            return 0;
        }


        //Add new file

        [NonAction]
        private int InsertNewFile(FileDB item)
        {
            int error_code = 200;
            if (item.file_title != null &&
                item.file_title.IndexOfAny(Path.GetInvalidFileNameChars()) < 0 &&
                item.file_data != null &&
                item.student_id != null)
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    try
                    {
                        if (UploadFile(item) == 0)
                        {
                            string queryString = string.Format("INSERT INTO files " +
                                "(student_id, file_path, title, upladed_date, description) Values " +
                                "('{0}','{1}','{2}','{3}','{4}')", item.student_id,
                                item.file_path, item.file_title, item.uploaded_time, item.description);
                            SqlCommand command = new SqlCommand(queryString, con);
                            con.Open();
                            var reader = command.ExecuteNonQuery();
                        }

                    }
                    catch (Exception ex) { error_code = 401; }
                    finally { con.Close(); }
                }
            }
            return error_code;
        }

        [NonAction]
        private int UploadFile(FileDB item)
        {
            string path;
            if ((path = GetNewFilePath()) != null)
            {
                path = Path.Combine(path, item.student_id + "_" + Path.GetRandomFileName());
                FileInfo file = new FileInfo(Path.Combine(initial_file_path + path));
                FileStream stream = null;
                try
                {
                    stream = file.OpenWrite();
                    stream.Write(item.file_data, 0, item.file_data.Length);
                    item.file_path = path;
                    item.uploaded_time = DateTime.Now;
                    return 0;
                }
                catch (Exception)
                {
                    return 10;
                }
                finally
                {
                    if (stream != null)
                        stream.Close();
                }

            }

            return 0;
        }

        [NonAction]
        string GetNewFilePath()
        {
            var path = string.Format(@"{0}\{1}\{2}\", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            if (!Directory.Exists(Path.Combine(initial_file_path + path)))
            {
                try
                {
                    Directory.CreateDirectory(Path.Combine(initial_file_path + path));
                }
                catch (Exception) { return null; }
            }
            return path;
        }

        [HttpPost]
        [Route("api/values/add file")]
        public async Task<IHttpActionResult> AddFile()
        {

            if (Request.Content.IsMimeMultipartContent())
            {
                var content = Request.Content;
                var n = await content.ReadAsMultipartAsync();
                var k = await n.Contents[0].ReadAsStringAsync();
                var l = await n.Contents[1].ReadAsStringAsync();
                var h = await n.Contents[2].ReadAsByteArrayAsync();
                var s = await n.Contents[3].ReadAsStringAsync();

                FileDB sst = new FileDB() { file_data = h, file_title = l, description = k, student_id = s };
                if (InsertNewFile(sst) != 200)
                    return BadRequest("file not saved");

                //foreach (var item in content.ReadAsMultipartAsync())
                //{
                //    if (i < 2)
                //        xx[i] = await item.ReadAsAsync(typeof(string));
                //    else
                //        xx[i] = await item.ReadAsByteArrayAsync();
                //}
                return Created("loaction", "Created success");
            }
            return BadRequest("htrhtrhtrhtr trhtrhtr hrthtr");

            //FileDB sst = new FileDB() { file_data = st, file_title = "hello", description = "fewgewgewgew" };

            //InsertNewFile(sst);
            
        }


        //Edit existing student
        [NonAction]
        private int EditStudent(long id, Student item)
        {
            int error_code = 200;
            if (item.address != null &&
                item.birthday != null &&
                item.email != null &&
                item.first_name != null &&
                item.last_name != null &&
                item.number != null &&
                item.phone != null)
            {
                long num;
                if (long.TryParse(item.number, out num))
                {
                    using (SqlConnection con = new SqlConnection(ConnectionString))
                    {
                        try
                        {
                            string queryString = string.Format("UPDATE students SET " +
                                "first_name = '{0}', last_name = '{1}', Number = '{2}', bairthday = '{3}', " +
                                "email = '{4}', phone = '{5}', address = '{6}' WHERE Id = {7}"
                                , item.first_name, item.last_name, item.number, item.birthday,
                                item.email, item.phone, item.address, id);
                            SqlCommand command = new SqlCommand(queryString, con);
                            con.Open();
                            var reader = command.ExecuteNonQuery();
                            if (reader != 1)
                                error_code = 404;
                        }

                        catch (Exception ex) { error_code = 401; }
                        finally { con.Close(); }
                    }
                }
                error_code = 501;
            }
            return error_code;
        }

        [HttpPost]
        [Route("api/values/edit student/{id:long?}")]
        public void Edit(long? id, [FromBody]Student st)
        {
            if (id != null)
                EditStudent(id.Value, st);
        }


        //Delete existing student
        [NonAction]
        private int DeleteStudent(long id)
        {
            int error_code = 200;
            if (id > 0)
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    try
                    {
                        string queryString = string.Format("DELETE FROM students WHERE Id = {0}", id);
                        SqlCommand command = new SqlCommand(queryString, con);
                        con.Open();
                        var reader = command.ExecuteNonQuery();
                        if (reader != 1)
                            error_code = 404;
                    }

                    catch (Exception ex) { error_code = 401; }
                    finally { con.Close(); }
                }

                error_code = 501;
            }
            return error_code;
        }

        [HttpDelete]
        [Route("api/values/delete student/{id:long?}")]
        public void DeleteStudent(long? id)
        {
            if (id.HasValue)
                DeleteStudent(id.Value);
        }


        //Delete existing student

        [NonAction]
        private int DeleteFile(long id)
        {
            int error_code = 200;
            if (id > 0)
            {
                bool done = true;
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    try
                    {
                        string queryString = string.Format("SELECT file_path FROM files WHERE Id = {0}", id);
                        SqlCommand command = new SqlCommand(queryString, con);
                        con.Open();
                        var reader = command.ExecuteScalar();
                        if (reader != null)
                        {
                            try
                            {
                                System.IO.File.Delete(Path.Combine(initial_file_path, reader.ToString()));
                            }
                            catch (Exception)
                            {
                                done = false;
                            }
                        }
                    }
                    catch (Exception ex) { error_code = 401; }
                    finally { con.Close(); }
                }
                if (done)
                {
                    using (SqlConnection con = new SqlConnection(ConnectionString))
                    {
                        try
                        {
                            string queryString = string.Format("DELETE FROM files WHERE Id = {0}", id);
                            SqlCommand command = new SqlCommand(queryString, con);
                            con.Open();
                            var reader = command.ExecuteNonQuery();
                            if (reader != 1)
                                error_code = 404;
                        }
                        catch (Exception ex) { error_code = 401; }
                        finally { con.Close(); }
                    }
                }

                error_code = 501;
            }
            return error_code;
        }

        [HttpDelete]
        [Route("api/values/delete file/{file_id:long?}")]
        public void DeleteFile(long? file_id)
        {
            if (file_id.HasValue)
                DeleteFile(file_id.Value);
        }


        //Download File

        [NonAction]
        byte[] DownloadFile(string path)
        {
            path = initial_file_path + path;
            if (File.Exists(path))
            {
                try
                {
                    var vv = File.ReadAllBytes(path);
                    return vv;
                }
                catch (Exception) { }
            }
            return null;

        }

        [HttpGet, Route("api/values/download/{file_id:int?}")]
        public HttpResponseMessage Download(int? file_id)
        {
            if (file_id.HasValue)
            {
                string file_path;
                if (GetFilePath(file_id.Value, out file_path))
                {
                    var bytes = DownloadFile(file_path);


                    HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
                    result.Content = new ByteArrayContent(bytes);
                    result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                    //var y = Ok(bytes);
                    //y.Request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    //y.Request.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                    return result;
                }
            }

            return null;
        }

        [NonAction]
        private bool GetFilePath(int id, out string file_path)
        {
            file_path = "";
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                try
                {
                    string queryString = string.Format("SELECT file_path FROM files WHERE Id = {0}", id);
                    SqlCommand command = new SqlCommand(queryString, con);
                    con.Open();
                    var reader = command.ExecuteScalar();
                    if (reader != null)
                    {
                        file_path = reader.ToString();
                        return true;
                    }
                }
                catch (Exception ex) { }
                finally { con.Close(); }
                return false;
            }
        }
    }
}
