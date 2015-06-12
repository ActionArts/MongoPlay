using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using MongoDB.Driver;
using M101DotNet.WebApp.Models;
using MongoDB.Bson;
using System.Linq.Expressions;

namespace M101DotNet.WebApp.Controllers
{
	public class Grade
	{
		public ObjectId Id {get; set; }
		public int student_id { get; set; }
		public double score { get; set; }
		public string type { get; set; }
	}

    public class HomeController : Controller
    {
		public ActionResult Index()
		{
			//Homework 2.3


			return View();
		}

		//[HttpGet]
		//[AsyncTimeout(8000)]
		//[HandleError(ExceptionType = typeof(TimeoutException), View = "TimedOut")]
		//public async Task<ActionResult> Index()
		//{
		//	//Homework 2.2
		//	List<Grade> gradesDeleted = await DeleteGradesAsync();

		//	return View(gradesDeleted);
		//}

	    static async Task<List<Grade>> DeleteGradesAsync()
	    {
			var connectionString = "mongodb://localhost:27017";
			var client = new MongoClient(connectionString);

			//remove the grade of type "homework" with the lowest score for each student from the dataset

			var db = client.GetDatabase("students");

			var col = db.GetCollection<Grade>("grades");

			//Hint: If you select homework grade-documents, sort by student and then by score, 
			//you can iterate through and find the lowest score for each student by noticing 
			//a change in student id. As you notice that change of student_id, remove the document.

			var builder = Builders<Grade>.Filter;
			var filter = builder.Eq(g => g.type,"homework");

			var list = await col.Find(filter).Sort(Builders<Grade>.Sort.Ascending(g => g.student_id).Ascending(g => g.score)).ToListAsync();

		    int previousStudentId = int.MinValue;
			List<Grade> gradesToDelete = new List<Grade>();
			foreach (var grade in list)
			{
				if (grade.student_id != previousStudentId)
				{
					gradesToDelete.Add(grade);
					previousStudentId = grade.student_id;
				}
			}

			List<Grade> gradesDeleted = new List<Grade>();
			for(var i = 0; i < gradesToDelete.Count; i++)
			{
				var iterator = i;
				gradesDeleted.Add(await col.FindOneAndDeleteAsync<Grade>(g => g.Id.Equals(gradesToDelete[iterator].Id), new FindOneAndDeleteOptions<Grade, Grade>{ MaxTime = TimeSpan.FromSeconds(60)}) ); 
			}
			//var result = await col.DeleteManyAsync(g => gradesToDelete.Select(s => s.Id).Contains(g.Id));

		    return gradesDeleted;
	    }
    }
}