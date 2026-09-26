using Microsoft.Data.SqlClient;

namespace Part2_Raceday.Backend
{
    public class Users
    {
        public List<string> return_specific_user(int id)
        {
            List<string> user = new List<string>();
            string connection_string = @"Data source=(localdb)\Race_Day_DB;database=RACEDAY_DB;";

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();


                    string query = "SELECT * FROM Users WHERE UserId = @UserId;";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@UserId", id);

                        using (SqlDataReader reader = run_query.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (!reader.IsDBNull(0))
                                    user.Add(reader.GetString(1));
                            }
                        }
                    }

                    connect.Close();
                }
            }
            catch (Exception error)
            {
                user.Add(error.Message);
            }

            return user;
        }





    }
}
