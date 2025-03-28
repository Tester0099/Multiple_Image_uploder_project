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
    public class AdminController : Controller
    {
        private readonly string connections = ConfigurationManager.ConnectionStrings["Intract"].ConnectionString;


        public ActionResult Dashboard()
        {

            if (Session["Userid"] == null)
            {
                return RedirectToAction("SendOTP","Account");
            }
            //clearotp();

            int count = 0;

            using(SqlConnection conn=new SqlConnection(connections))
            {
                string query = "SELECT COUNT(*) FROM user_data";
                using(SqlCommand cmd=new SqlCommand(query, conn))
                {
                    conn.Open();
                    count = (int)cmd.ExecuteScalar();
                }

            }
            ViewBag.count = count;

            return View();
        }

        //private void clearotp()
        //{
        //    int userid = Convert.ToInt32(Session["userid"]);
        //    using(SqlConnection con=new SqlConnection(connections))
        //    {
        //        string query = "Update user_data SET otp=null Where Id=@Id";
        //        using (SqlCommand cmd = new SqlCommand(query,con))
        //        {
        //            cmd.Parameters.AddWithValue("@Id", userid);
        //            con.Open();
        //            cmd.ExecuteNonQuery();
        //        }

        //    }

        //}

        public ActionResult Multiple_Image()
        {
            return View();
        }


        [HttpPost]
        public async Task<ActionResult> Multiple_Image(IEnumerable<HttpPostedFileBase> image,user_data data,image_data info)
        {
            int userid = Convert.ToInt32(Session["userid"]);

            if (Session["Userid"] == null)
            {
                return RedirectToAction("otp_verificatin");
            }
         
            if (userid == 0)
            {
                TempData["error"] = "User not found!";
                return RedirectToAction("Multiple_image");
            }


            if (image != null)
                {
                    string folder_path = Server.MapPath("~/Content/Upload_Image");

                    if (!Directory.Exists(folder_path))
                    {
                        Directory.CreateDirectory(folder_path);
                    }

                    List<string> filepaths = new List<string>();

                    foreach (var file in image)
                    {
                        string filename = file.FileName;
                        string filepath = Path.Combine(folder_path, filename);

                        file.SaveAs(filepath);
                        filepaths.Add(filename);


                        using (SqlConnection con = new SqlConnection(connections))
                        {
                            string Query = "Insert into Image_data (image,Userid) values(@image,@Userid)";
                            using (SqlCommand cmd = new SqlCommand(Query, con))
                            {
                                cmd.Parameters.AddWithValue("@image", filename);
                                cmd.Parameters.AddWithValue("@Userid",userid);
                                con.Open();
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }

                    }

                }
            TempData["msg"] = "Image insert";
            return RedirectToAction("Multiple_Image");

        }

        [HttpGet]
        public ActionResult show_image(image_data data)
        {

            if (Session["Userid"] == null)
            {
                return RedirectToAction("SendOTP", "Account");
            }
            List<image_data> images = new List<image_data>();   

            try
            {
                using(SqlConnection con = new SqlConnection(connections))
                {
                    string query = "SELECT * FROM Image_data";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        con.Open();
                        using(SqlDataReader red = cmd.ExecuteReader())
                        {
                            while (red.Read())
                            {
                                images.Add(new image_data()
                                {
                                    id = Convert.ToInt32(red["id"]),
                                    image=red["image"].ToString(),
                                    Userid = Convert.ToInt32(red["Userid"])

                                });
                            }
                           
                        }

                    }
                    
                }
            }
            catch (Exception ex)
            {
                ViewBag.error=ex.Message;
                return View();
            }


            return View(images);
        }

        //Logout Process
        public ActionResult Logout()
        {
            try
            {
                string otp =(string)Session["otp"];

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

        [HttpGet]
        public ActionResult Userlist()
        {
            if (Session["Userid"] == null)
            {
                return RedirectToAction("SendOTP", "Account");
            }

            List<user_data> userlist = new List<user_data>();
            try
            {
                using(SqlConnection conn = new SqlConnection(connections))
                {
                    string query = "SELECT *FROM user_data";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        using(SqlDataReader holder = cmd.ExecuteReader())
                        {
                            while (holder.Read())
                            {
                                userlist.Add(new user_data()
                                {
                                    Id = Convert.ToInt32(holder["id"]),
                                    Username = holder["Username"].ToString(),
                                    Email = holder["email"].ToString(),
                                    otp = holder["otp"].ToString(),
                                    Rollid = Convert.ToInt32(holder["Rollid"]),
                                    IsActive = Convert.ToBoolean(holder["IsActive"])

                                });
                            }
                        };
                    }
                }
            }
            catch(Exception ex)
            {
                ViewBag.ErrorLog = ex.Message;
                return View();
            }

            return View(userlist);
        }

        //Delete Method
        [HttpPost]
        public ActionResult Remove(int Id)
        {
            try
            {
                using( SqlConnection conn = new SqlConnection(connections))
                {
                    string query = "Delete From user_data where Id= @Id";
                    using(SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", Id);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                TempData["msg"] = "Data delete successfully";
                return RedirectToAction("Userlist", "Admin");
            }
            catch (Exception ex)
            {
                ViewBag["error"] = ex.Message;
                return View();
            }
            
        }

        //[HttpPost]
        ////Update User Status
        //public ActionResult UpdateUserStatus(bool IsActive)
        //{
        //    int Userid = Convert.ToInt32(Session["UserId"]);

        //    try
        //    {
        //        using (SqlConnection conn = new SqlConnection(connections))
        //        {
        //            string query = "UPDATE user_data SET IsActive = @IsActive WHERE Id = @Id";
        //            using (SqlCommand cmd = new SqlCommand(query, conn))
        //            {
        //                cmd.Parameters.AddWithValue("@Id", Userid);
        //                cmd.Parameters.AddWithValue("@IsActive", IsActive);
        //                conn.Open();
        //                cmd.ExecuteNonQuery();
        //            }
        //        }
        //        TempData["Success"] = "User status updated successfully.";
        //        return RedirectToAction("UserList", "Admin");
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["Error"] = "Something went wrong: " + ex.Message;
        //        return RedirectToAction("UserList", "Admin");
        //    }
        //}
        [HttpPost]
        public ActionResult UpdateUserStatus(int id, bool isActive)
        {

            using (SqlConnection conn = new SqlConnection(connections))
            {
                string query = "UPDATE user_data SET IsActive = @IsActive WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@IsActive", isActive);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("UserList", "Admin");
        }

    }
}