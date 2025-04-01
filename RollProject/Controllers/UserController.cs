using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using RollProject.Models;

namespace RollProject.Controllers
{
    public class UserController : Controller
    {
        private readonly string connections = ConfigurationManager.ConnectionStrings["Intract"].ConnectionString;

        public ActionResult User_dashboard()
        {
            //check the userid receive on User_dashboard or not 
            if (Session["Userid"] == null)
            {
                return RedirectToAction("SendOTP","Account");
            }
            //Redirect user_dashboard delete otp from database 
            clearotp();

            return View();
        }

        //clear otp from database
        private void clearotp()
        {
            int userid = Convert.ToInt32(Session["userid"]);
            using (SqlConnection con = new SqlConnection(connections))
            {
                string query = "Update user_data SET otp=null Where Id=@Id";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Id", userid);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }

            }

        }
        public ActionResult Image_Upload()
        {
            //check the userid receive on Image_Upload page or not 
            if (Session["Userid"] == null)
            {
                return RedirectToAction("SendOTP", "Account");
            }
           return View();
        }


        [HttpPost]
        public async Task<ActionResult> Upload(IEnumerable<HttpPostedFileBase> image, user_data data, image_data info)
        {
            //user convert in integer
            int userid = Convert.ToInt32(Session["userid"]);

            string Username = Session["username"].ToString();
            if (userid == 0)
            {
                TempData["error"] = "User not found!";
                return RedirectToAction("SendOTP", "Account");
            }


            if (image != null)
            {
                //folder path where image is store
                string folder_path = Server.MapPath("~/Content/Upload_Image");

                //check the folder path exist other wise create path
                if (!Directory.Exists(folder_path))
                {
                    Directory.CreateDirectory(folder_path);
                }

                //create a list
                List<string> filepaths = new List<string>();

                //using foreach loop insert image 
                foreach (var file in image)
                {
                    string filename = file.FileName;
                    string filepath = Path.Combine(folder_path, filename);

                    file.SaveAs(filepath);
                    filepaths.Add(filename);


                    using (SqlConnection con = new SqlConnection(connections))
                    {
                        string Query = "Insert into Image_data (image,Userid,Username) values(@image,@Userid,@Username)";
                        using (SqlCommand cmd = new SqlCommand(Query, con))
                        {
                            cmd.Parameters.AddWithValue("@image", filename);
                            cmd.Parameters.AddWithValue("@Userid", userid);
                            cmd.Parameters.AddWithValue("@Username",Username);
                            con.Open();
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }

                }

            }

            TempData["msg"] = "Image insert";
            return RedirectToAction("Image_Upload");

        }

        public ActionResult Uploaded_ourImage()
        {

            if (Session["Userid"] == null)
            {
                return RedirectToAction("SendOTP","Account");
            }
            int identificationid = Convert.ToInt32(Session["Userid"]);
            List<image_data> images = new List<image_data>();

            try
            {
                using(SqlConnection con = new SqlConnection(connections))
                {
                    string query = "SELECT image ,Id FROM Image_data WHERE Userid=@Userid";
                    using(SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Userid", identificationid);
                        con.Open();
                        using(SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                images.Add(new image_data()
                                {
                                    id = Convert.ToInt32(reader["Id"]),
                                    image = reader["image"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Exception = ex.Message;
                return View();
            }
            return View(images);
        }


        public ActionResult Logout()
        {
            try
            {
                string otp = (string)Session["otp"];

                if (!string.IsNullOrEmpty(otp))
                {
                    using (SqlConnection conn = new SqlConnection(connections))
                    {
                        string Query = "UPDATE user_data SET otp = NULL WHERE otp =@otp ";
                        using (SqlCommand cmd = new SqlCommand(Query, conn))
                        {
                            cmd.Parameters.AddWithValue("@otp", otp);
                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }

                    }
                }

                Session.Clear();
                Session.Abandon();

                TempData["msg"] = "Logout Successfully";
                return RedirectToAction("SendOTP","Account");

            }
            catch (Exception ex)
            {
                ViewBag.ErrorLog = ex.Message;
                return View();
            }

        }

        //Delete method 
        [HttpPost]
        public ActionResult Delete_img(int id)
        {
            try
            {

                //file path
                string folder_path = Server.MapPath("~/Content/Upload_Image/");
                //file name string type
                string filename = string.Empty;

                //get file name in database
                using (SqlConnection conn = new SqlConnection(connections))
                {
                    string query = "select image from image_data where id=@id";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        conn.Open();
                        filename = (string)cmd.ExecuteScalar();
                    }
                }

                //file name and filepath combine and delete
                string combine = Path.Combine(folder_path, filename);
                if (System.IO.File.Exists(combine))
                {
                    System.IO.File.Delete(combine);
                }

                //delete from data image
                using (SqlConnection conn = new SqlConnection(connections))
                {
                    string query = "Delete from image_data where id=@id";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("Uploaded_ourImage","User");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View();
            }

        }

        public ActionResult Edit(int id)
        {
            image_data data = new image_data();

            using (SqlConnection con = new SqlConnection(connections))
            {
                string query = "Select *from image_data  where id=@id";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    con.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            data.id = Convert.ToInt32(rdr["ID"]);
                            data.image = rdr["image"].ToString();
                        }
                    }
                }
            }
            return View(data);
        }

        [HttpPost]
        public ActionResult Update_image(image_data data, HttpPostedFileBase image)
        {

            string folder_path = Server.MapPath("~/Content/Upload_Image/");
            string filename = image.FileName;
            string filepath = Path.Combine(folder_path, filename);

            if (System.IO.File.Exists(filepath))
            {
                System.IO.File.Delete(filepath);
            }

            data.image = filename;
            image.SaveAs(filepath);

            using (SqlConnection con = new SqlConnection(connections))
            {
                string query = "Update image_data set image=@image where id=@id";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@image", filename);
                    cmd.Parameters.AddWithValue("@id", data.id);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            TempData["msg"] = "Update successfully";
            return RedirectToAction("Uploaded_ourImage");
        }
    }
}