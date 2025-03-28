using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RollProject.Models
{
	public class image_data
	{
		[Key]
		public int id { get; set; }	
		public string image {  get; set; }
		public int Userid { get; set; }
	}
}