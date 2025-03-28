using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RollProject.Models
{
	public class user_data
	{
		[Key]
		public int Id { get; set; }
		public string Username { get; set; }
		public string Email { get; set; }
		public string otp {  get; set; }
		public int Rollid	{ get; set; }

		public  bool IsActive { get; set; }

		//public string image { get; set; }
	}
}