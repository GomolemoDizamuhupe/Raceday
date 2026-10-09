using Microsoft.Data.SqlClient;

namespace Part2_Raceday.Backend
{
    public class Routes
    {
        private const string connection_string = @"Data source=(localdb)\MSSQLLocalDB;database=Race_day;";

        //Event object
        Events events = new Events();

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


        public string uploading_route(int categoryId, string StartPoint, string EndPoint, int ElevationGainM, string MapUrl)
        {
            string message = string.Empty;

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = "INSERT INTO Routes(categoryId, StartPoint, EndPoint, ElevationGainM, MapUrl)" +
                                    "VALUES (@categoryId, @StartPoint, @EndPoint, @ElevationGainM, @MapUrL);";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@CategoryId", categoryId);
                        run_query.Parameters.AddWithValue("@StartPoint", StartPoint);
                        run_query.Parameters.AddWithValue("@EndPoint", EndPoint);
                        run_query.Parameters.AddWithValue("@ElevationGainM", ElevationGainM);
                        run_query.Parameters.AddWithValue("@MapUrl", MapUrl);

                        run_query.ExecuteNonQuery();
                        message = "Route Created Successfully.";
                    }
                }
            }
            catch (Exception error)
            {
                message = error.Message;
            }

            return message;
        }



        public string editing_route(int OrganiserId, int RouteId, int categoryId, string StartPoint, string EndPoint, int ElevationGainM, string MapUrl)
        {
            string message = string.Empty;

            string role = events.role_valiation(OrganiserId);
            if (role == "Organiser")
            {
                try
                {
                    using (SqlConnection connect = new SqlConnection(connection_string))
                    {
                        connect.Open();

                        // Update by CategoryId; verify the organiser owns the parent event via a join
                        string query = @"UPDATE r SET r.StartPoint = @StartPoint,
                                                       r.EndPoint = @EndPoint,
                                                       r.ElevationGainM = @ElevationGainM,
                                                       r.MapUrl = @MapUrl
                                        FROM Routes r
                                        JOIN Categories c ON r.CategoryId = c.CategoryId
                                        JOIN Events e ON c.EventId = e.EventId
                                        WHERE r.RouteId = @RouteId AND e.OrganiserId = @OrganiserId;";

                        using (SqlCommand run_query = new SqlCommand(query, connect))
                        {
                            run_query.Parameters.AddWithValue("@OrganiserId", OrganiserId);
                            run_query.Parameters.AddWithValue("@CategoryId", categoryId);
                            run_query.Parameters.AddWithValue("@RouteId", RouteId);
                            run_query.Parameters.AddWithValue("@StartPoint", StartPoint);
                            run_query.Parameters.AddWithValue("@EndPoint", EndPoint);
                            run_query.Parameters.AddWithValue("@ElevationGainM", ElevationGainM);
                            run_query.Parameters.AddWithValue("@MapUrl", MapUrl);

                            int rows = run_query.ExecuteNonQuery();
                            if (rows > 0)
                            {
                                message = "Route Updated Successfully.";
                            }
                            else
                            {
                                message = "No matching category found for this organiser.";
                            }
                        }
                    }
                }
                catch (Exception error)
                {
                    message = error.Message;
                }
            }
            else
            {
                message = "Route can not be edited because of your role. You are not a organiser.";
            }

            return message;
        }

    }
}
