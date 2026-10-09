using Microsoft.Data.SqlClient;

namespace Part2_Raceday.Backend
{
    public class Routes
    {
        private const string connection_string = @"Data source=(localdb)\MSSQLLocalDB;database=Race_day;";
        public string get_route_for_category(int categoryId) 
        {

            string message = string.Empty;

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = "SELECT StartPoint, EndPoint, ElevationGainM, MapUrl FROM Routes WHERE CategoryId = @CategoryId";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@CategoryId", categoryId);

                        using (SqlDataReader reader = run_query.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                message = $"Start Point: {reader["StartPoint"]}\n End Point: {reader["EndPoint"]}\n" +
                                           $"Elevation: {reader["ElevationGainM"]}m\n MapUrl: {reader["MapUrl"]}";
                            }
                            else
                            {
                                message = "No category found with that ID.";
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                message = e.Message;
            }

            return message;
        }

    }
}
