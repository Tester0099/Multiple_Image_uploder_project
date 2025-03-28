using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using RollProject.Models;

namespace RollProject.Controllers
{
    public class AccountController : Controller
    {
        // Connection Strings
        private readonly string connections = ConfigurationManager.ConnectionStrings["Intract"].ConnectionString;


        public ActionResult Register()
        {
            return View();
        }

        //Data insert code
        [HttpPost]

        public ActionResult Adduser(user_data data)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connections))
                {
                    string query = "insert into user_data (Username,Email,Rollid,IsActive) values (@Username,@Email,2,0)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", data.Username);
                        cmd.Parameters.AddWithValue("@Email", data.Email);
                        conn.Open();
                        cmd.ExecuteNonQuery();

                    }
                }
                TempData["Msg"] = "Data insert successfully";
                return RedirectToAction("SendOTP","Account");
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return View();
            }
        }

        public ActionResult SendOTP()
        {
            return View();
        }

        //send OTP Process
        [HttpPost]

        public async Task<ActionResult> Otpverify(string Email)
        {
            //Fetch the database User is Active or not 
            using (SqlConnection conn = new SqlConnection(connections))
            {
                string query = "SELECT IsActive FROM user_data WHERE Email = @Email";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", Email);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int IsActive = Convert.ToInt32(reader["IsActive"]);
                            Session["IsActive"] = IsActive;

                            if (IsActive == 0)
                            {
                                TempData["msg"] = "Your account is not active. Please contact support.";
                                return RedirectToAction("SendOTP","Account");
                            }
                        }
                    }
                }
            }


            //Check the email exist in database or not
            string enteredEmail = Email;
            using (SqlConnection conn = new SqlConnection(connections))
            {
                string query = "SELECT COUNT(*) FROM user_data WHERE Email = @Email";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", enteredEmail);
                    conn.Open();
                    int count = (int)await cmd.ExecuteScalarAsync();

                    if (count == 0)
                    {
                        TempData["msg"] = "This email does not exist in database";
                        return RedirectToAction("SendOTP","Account");
                    }
                }
            }

            //OTP SEND PROCESS

              // Generate OTP
                Random random = new Random();
                string otp = random.Next(100000, 999999).ToString();

                // Update OTP in database
                using (SqlConnection conn = new SqlConnection(connections))
                {
                    string query = "UPDATE user_data SET otp = @otp WHERE email = @Email";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@otp", otp);
                        cmd.Parameters.AddWithValue("@Email", enteredEmail);
                        conn.Open();
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                string senderEmail = "Rakesh1214@barrownz.com";
                string password = "jcpr bope hlcv mizb";
                string smtpServer = "smtp.gmail.com";

                MailMessage mail = new MailMessage
                {
                    From = new MailAddress(senderEmail),
                    Subject = "Dear User, Your OTP",
                    Body = $"Dear User,\nYour OTP code for login is:{otp}\nThis OTP is valid for 5 minutes.\nRegards\nYour Company Barrownz Group",
                    IsBodyHtml = false
                };

                mail.To.Add(enteredEmail);

                SmtpClient smtp = new SmtpClient
                {
                    Host = smtpServer,
                    Port = 587,
                    Credentials = new NetworkCredential(senderEmail, password),
                    EnableSsl = true
                };


                await smtp.SendMailAsync(mail);

                return RedirectToAction("VerifyOTP", "Account");

            
           
        }


        public  ActionResult VerifyOTP()
        {
            return View();
        }

        [HttpPost]
        public ActionResult verification(user_data data)
        {

            try
            {
                using (SqlConnection conn = new SqlConnection(connections))
                {
                    string Query = "SELECT Id,Username,Email,Rollid,IsActive FROM user_data WHERE otp=@otp";
                    using (SqlCommand cmd = new SqlCommand(Query, conn))
                    {
                        cmd.Parameters.AddWithValue("@otp", data.otp);
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int rollid = Convert.ToInt32(reader["rollid"]);
                                string Username = reader["Username"].ToString();
                                string Email=reader["email"].ToString();
                                int UserId = Convert.ToInt32(reader["Id"]);
                                bool IsActive =Convert.ToBoolean(reader["IsActive"]);
                                Session["UserLogin"] = true;
                                Session["rollid"] = rollid;
                                Session["Username"]=Username;
                                Session["Email"]=Email;
                                Session["Userid"]=UserId;
                                Session["IsActive"]=IsActive;

                                TempData["rollid"] = rollid;
                                TempData["Username"] = Username;
                                TempData["Email"] = Email;
                                TempData["Userid"] = UserId;
                                TempData["IsActive"] = IsActive;

                                if (IsActive == true)
                                {
                                    if (rollid == 1)
                                    {
                                        return RedirectToAction("Dashboard", "Admin");
                                    }
                                    else if (rollid == 2)
                                    {
                                        return RedirectToAction("User_dashboard", "User");
                                    }
                                    else
                                    {
                                        TempData["msg"] = "You are not an authorized person.";
                                        return RedirectToAction("SendOTP", "Account");
                                    }
                                }
                                else
                                {
                                    TempData["Msg"] = "Your account is inactive. Please contact support.";
                                    return RedirectToAction("VerifyOTP", "Account");
                                }
                            }
                            else
                            {
                                TempData["msg"] = "Invalid OTP";
                                return RedirectToAction("VerifyOTP", "Account");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return RedirectToAction("VerifyOTP", "Account");
            }
        }
    }
}