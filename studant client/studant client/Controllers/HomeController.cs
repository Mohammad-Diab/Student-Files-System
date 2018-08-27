using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace studant_client.Controllers
{
    public class HomeController : Controller
    {
        string api_url = @"http://localhost:51550/api/values/";
        public ActionResult Index()
        {
            return RedirectToAction("Studants");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            ViewBag.k = "44";
            var x = View();
            return x;
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult AddFile(int? id)
        {
            if (id.HasValue)
            {
                ViewBag.id = id;
            }
            return View();
        }

        public async Task<ActionResult> Studants(int? page)
        {
            page = page.HasValue ? page.Value : 1;
            HttpClient client = new HttpClient();
            var r = await client.GetAsync(api_url + "students/page" + page);
            if (r.IsSuccessStatusCode)
            {
                var resultStr = await r.Content.ReadAsStringAsync();
                var k = JsonConvert.DeserializeObject<List<Person>>(resultStr);
                ViewBag.students = k;
            }
            else
                ViewBag.students = r.Content;

            ViewBag.page = page;
            ViewBag.count = 0;
            var s = await client.GetAsync(api_url + "status");
            if (s.IsSuccessStatusCode)
                ViewBag.count = JsonConvert.DeserializeObject<Status>(await s.Content.ReadAsStringAsync()).StudentsCount;

            return View();
        }

        public async Task<ActionResult> Student(long? student_id)
        {
            ViewBag.jsfile = "student.js";
            if (student_id.HasValue)
            {
                HttpClient client = new HttpClient();
                var request = await client.GetAsync(api_url + "student/" + student_id);
                if (request.IsSuccessStatusCode)
                {
                    var resultstr = await request.Content.ReadAsStringAsync();
                    var student = JsonConvert.DeserializeObject<Student>(resultstr);
                    student.birthday_str = student.birthday.Date.ToString("yyyy-MM-dd");
                    ViewBag.std = student;
                }
                else
                    ViewBag.std = request.Content;
            }
            else
                ViewBag.std = new Student() { files_list = new List<FileDB>() }; // empty form = add student
            return View();
        }


        public async Task<FileResult> download(long? id)
        {
            if (id.HasValue)
            {
                HttpClient client = new HttpClient();
                var request = await client.GetAsync(api_url + "download/" + id);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                if (request.IsSuccessStatusCode)
                {
                    var bytes = await request.Content.ReadAsByteArrayAsync();

                    return File(bytes, System.Net.Mime.MediaTypeNames.Application.Octet, "file name here.dat");

                }
            }
            return null;
        }

        [HttpPost]
        public async Task<ActionResult> upload(HttpPostedFileBase file, long? id, string title, string description)
        {
            //foreach(var i in file)

            if (file != null && id.HasValue)
            {
                byte[] bytes = new byte[file.InputStream.Length];
                file.InputStream.Read(bytes, 0, (int)file.InputStream.Length);
                if (string.IsNullOrEmpty(title))
                    title = System.IO.Path.GetFileName(file.FileName);

                FileDB gg = new FileDB() { description = "file1", file_data = bytes, file_title = "title" };
                HttpClient client = new HttpClient();

                var k = new MultipartContent();
                k.Add(new StringContent(description ?? ""));
                k.Add(new StringContent(title));
                k.Add(new ByteArrayContent(bytes));
                k.Add(new StringContent(id.ToString()));
                //var kk = new FileContentResult(bytes, "byte[]");
                ByteArrayContent kk = new ByteArrayContent(bytes);
                var request = await client.PostAsync(api_url + "add%20file", k);

                if (request.IsSuccessStatusCode)
                {
                    int i = 0;
                }
            }
            return Redirect("student?student_id=" + id);
        }

        [HttpPost]
        public async Task<ActionResult> save(Student st)
        {
            HttpClient client = new HttpClient();
            var content = new StringContent(JsonConvert.SerializeObject(st), System.Text.Encoding.UTF8, "application/json");
            HttpResponseMessage request;
            if (st.id > 0)
                request = await client.PostAsync(api_url + "edit%20student/" + st.id, content);
            else
                request = await client.PostAsync(api_url + "add%20student", content);
            return new HttpStatusCodeResult(request.StatusCode);
        }

        [HttpPost]
        public async Task<ActionResult> delete_file(long? id)
        {
            HttpClient client = new HttpClient();
            var request = await client.DeleteAsync(api_url + "delete%20file/" + id);
            return new HttpStatusCodeResult(request.StatusCode);
        }

        [HttpPost]
        public async Task<ActionResult> delete_student(long? id)
        {
            HttpClient client = new HttpClient();
            var request = await client.DeleteAsync(api_url + "delete%20student/" + id);
            return new HttpStatusCodeResult(request.StatusCode);
        }


    }
}