/**********************************************************************************\
| |                         GNU GENERAL PUBLIC LICENSE                           | |
| |                          Version 3, 29 June 2007                             | |
| |                                                                              | |
| |      Copyright (C) 2007 Free Software Foundation, Inc. <http://fsf.org/>     | |
| | Everyone is permitted to copy and distribute verbatim copies                 | |
| | of this license document, but changing it is not allowed.                    | |
| |                                                                              | |
| | Author(s): Stephen Simon, Kevin Djordjevic                                   | |
| | Class: TablePancaker                                                         | |
| | Version: 0.0.0.5                                                             | |
| |                                                                              | |
| | Description: C# Class to Dynamically compress an unknown table dimension to  | |
| | a JSON array object representing the table for display on front end MV*      | |
| | Frameworks.                                                                  | |
| |                                                                              | |
| | About: I noticed in my industry there was a lot of struggle for a reliable   | |
| | way to fetch data out of a SQL Database. Every project had its own method,   | |
| | everyone power houses things into models, the end result is a jQuery or      | |
| | Angular output that just seemed to lack a consistancy.                       | |
| | On numerous occasions I discovered neglect to even close connections. It can | |
| | get pretty bad. So I decided with my birthday I'd right a nice little Open   | |
| | source project to help out with that struggle that may exist elsewhere.      | |
| | The concept for this TablePancaker is to handle a table as it is displayed   | |
| | on the web front end, which is a string. Numbers can be converted if         | |
| | calculations are desired, but the main focus of this controller is to allow  | |
| | the web server code to be load balanced behind some kind of load balancer.   | |
| | In the instance where we can take advantage of some web server caching too.  | |
| | This also helps offload some strain on some maybe less than averagely load   | |
| | balanced database infrastructures.                                           | |
| |                                                                              | |
\******************************************************************************** */
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace project.Classes
{
	class TablePancaker
	{
		private string						  	 _connectionString;
		private List<Tuple<int, string, string>> _tableData;
		private List<DollarPancake>              _parameters;
		public  enum StoredProcedureType 
		{	// CRUD
			Insert,
			Fetch,
			Update,
			Delete
		}

		/// <summary>
		/// Struct to store a JSON "key":"value" pair for .Net speed it's not KeyValuePair.
		/// </summary>
		public struct DollarPancake
		{
			public string Key;
			public string Value;
		}

		/// <summary>
		/// TablePanckaer() Constructor creates a db connection basted on a default 
		/// AD Credential in a web.config.
		/// </summary>
		public TablePancaker() { _connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString; }

		public TablePancaker(string connectionString) { _connectionString = connectionString; }

		public TablePancaker(string user, string passwd, string server, string database) { _connectionString = user + passwd + server + database; }

		public List<Tuple<int, string, string>> StoredProcedure(StoredProcedureType storedProecureType, string storedProcedureName, List<KeyValuePair<string, string>> parameters)
		{
			_parameters = new List<DollarPancake>();
			foreach (KeyValuePair<string, string> dollarPancake in parameters)
			{
				var temp = new SqlParameter(dollarPancake.Key, dollarPancake.Value);
				_parameters.Add( { temp.Key, temp.Value } );
			}
			switch(storedProecureType)
			{
				case StoredProcedureType.Insert:
					return PourPancake(storedProcedureName, _parameters);
				case StoredProcedureType.Fetch:
					return ServePancake(storedProcedureName, _parameters);
				case StoredProcedureType.Update:
					return FlipPancake(storedProcedureName, _parameters);
				case StoredProcedureType.Delete:
					return ThrowOutPancake(storedProcedureName, _parameters);
				default:
					return ServePancake(storedProcedureName, _parameters);
			}
		}

		private List<Tuple<int, string, string>> ServePancake(string storedProcedureName, DollarPancake parameters)
		{
			// Open the SQL Connection
			using (var connection = new SqlConnection(_connectionString))
			using (var command	  = new SqlCommand(storedProcedureName, connection)
					  {   // By using Stored Procedures we help protect one layer
					  	  CommandType = CommandType.StoredProcedure 
					  }
				  )
			{
				// Opening the connection in a try catch block still to catch it and close it if it fails to open.
				try
				{
					connection.Open();
					var reader = command.ExecuteReader();
					while (reader.Read())
						// Little trick I learned in college to search out itterators, I always use ii to indicate a for loop itterator, and jj for the next, etc.
						for (int ii = 0; ii < reader.FieldCount; ii++)
							// Now we're going through and building out a data point for each piece in the table. Each entry has an <x,y, value> in the matrix.
							_tableData.Add(Tuple.Create<int, string, string>(reader.GetInt32(0),reader.GetName(ii).ToString(),reader.GetValue(ii).ToString()));
							// [ 1 "ID"         "1"  ]
							// [ 1 "FirstName" "Joe" ]
							// [ 1 "LastName" "Smith"]
							// [ 2 "ID"         "2"  ]
							// [ 2 "FirstName" "Bob" ]
							// [ 2 "LastName" "Simon"]
				}
				// Error handler can go here if you have one, for the sake of my testing, I'm in mono on Arch Linux, so I just want console output:
				catch   (Exception ex) { Console.WriteLine(ex.ToString()); }
				// Anything go wrong? Lets close out the connection because we don't trust .NET to:
				finally { connection.Close(); }
			}
			// we just build the object and return it for clean code:
			return _tableData;
		}

		/// <summary>
		/// ToString() converts the private table data into JSON for MV* consumption.
		/// </summary>
		public override string ToString()
		{
			// New the result object, which will be JSON, lets use a logical name:
			StringBuilder JSON = new StringBuilder();
			// The int in the Tuple is a row ID to be able to select the ids from the table, you 
			// query like so: (Item1 being the int in the Tuple)
			List<int> ids      = _tableData.Select(t => t.Item1).ToList().Distinct().ToList();
			// Without the .Distinct().ToList() we are looking at having duplicate ids for each column
			// in the table. Time to start the JSON object:
			JSON.Append("{");

			// Now we loop through 
			foreach (var id in ids)
			{
				// Select a unique row based on an id we now have out of the private _tableData:
				List<Tuple<string,string>> uniqueRow = _tableData.Where(
																	q => q.Item1 == id
																 ).Select(
																 	m => Tuple.Create(
																 						m.Item2,
																 						m.Item3
																 					 )
																 	).ToList();
				JSON.Append("[");
				foreach (var dataPoint in uniqueRow)
					// Looping through this vvvvvv is the key and this vvvvvv is the value of the dataPoint.
					JSON.Append("\"" + dataPoint.Item1 + "\":\"" + dataPoint.Item2 + "\",");
				// After this loop, we have an extra comma at the end of the string, so we just lop it off:
				JSON.Remove(JSON.Length - 1, 1);
				// Close the array (being a single uniqueRow now converted to JSON ["key":"value","key":"value"])
				JSON.Append("],");
			}

			// After this loop, we have an extra comma again from the array. We have the final entry, lop off the comma:
			JSON.Remove(JSON.Length - 1, 1);
			// Close the JSON object, and add a newline to the end for various reasons for the output:
			JSON.Append("}\n");
			// Return the StringBuilder JSON.ToString();
			return JSON.ToString();
		}

	}
}
