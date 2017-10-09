using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using project.Models;

namespace project.Classes
{
	class TablePancaker
	{
		private string						  	 _connectionString;
		private List<Tuple<int, string, string>> _tableData;
		public  enum StoredProcedureType 
		{	// CRUD
			insert,
			fetch,
			update,
			delete
		}

		/// <summary>
		/// TablePanckaer() Constructor creates a db connection basted on a default 
		/// AD Credential in a web.config.
		/// </summary>
		public TablePancaker() { _connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString; }

		public TablePancaker(string connectionString) { _connectionString = connectionString; }

		public TablePancaker(string user, string passwd, string server, string database) { _connectionString = user + passwd + server + database; }

		public List<Tuple<int, string, string>> StoredProcedure(StoredProcedureType storedProecureType, string storedProcedureName)
		{
			using (var connection = new SqlConnection(_connectionString))
			using (var command    = new SqlCommand(storedProcedureName, connection)
					  {
					  	  CommandType = CommandType.StoredProcedure 
					  }
				  )
			{
				try
				{
					connection.Open();
					var reader = command.ExecuteReader();
					while (reader.Read())
						for (int ii = 0; ii < reader.FieldCount; ii++)
							_tableData.Add(Tuple.Create<int, string, string>(reader.GetInt32(0),reader.GetName(ii).ToString(),reader.GetValue(ii).ToString()));
				}
				catch   (Exception ex) { Console.WriteLine(ex.ToString()); }
				finally { connection.Close(); }
			}
			return _tableData;
		}

	}
}
