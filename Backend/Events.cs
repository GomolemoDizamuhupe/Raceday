using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Part2_Raceday.Backend
{
    public class Events
    {

        public string role_valiation(int id)
        {
            //connection string to connect
            string connection_string = @"Data source=(localdb)\MSSQLLocalDB;database=Race_day;";
            string role = string.Empty;

            try
            {
                //sql connection is used for connecting to the database
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    //do a query to insert the user info
                    string query = string.Empty;

                    query = @"SELECT Role FROM Users WHERE UserId = @UserId;";

                    //crearing sn instance to run the query
                    //using SqlCommand
                    SqlCommand run_query = new SqlCommand(query, connect);
                    run_query.Parameters.AddWithValue("@UserId", id);

                    //ExecuteScalar returns a single value(the count)
                    object results = run_query.ExecuteScalar();

                    if (results != null)
                    {
                        role = results.ToString();
                    }

                    //then closing the connection
                    connect.Close();
                }
            }
            catch (Exception error)
            {
                role = error.Message;
            }

            //returing the role
            return role;
        }


    }
}
