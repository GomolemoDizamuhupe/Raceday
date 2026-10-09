using Microsoft.Data.SqlClient;

namespace Part2_Raceday.Backend
{
    public class Categories
    {


        //Event object
        Events events = new Events();

        private const string connection_string = @"Data source=(localdb)\MSSQLLocalDB;database=Race_day;";

        public List<string> get_event_category(int event_id)
        {
            List<string> message = new List<string>();

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = string.Empty;

                    query = "SELECT Name FROM Categories WHERE EventId = @EventId;";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@EventId", event_id);

                        using (SqlDataReader reader = run_query.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                do
                                {
                                    message.Add($"Event category: " + reader.GetString(0));
                                }
                                while (reader.Read());
                            } 
                            else 
                            {
                                message.Add("Event category not found.");
                            }
                        }

                    }

                }
            }
            catch (Exception e)
            {
                message.Add("Error: " + e.Message);
            }

            return message;
        }


        public string uploading_category(int eventId, string name, string distancekm, int maxparticipants)
        {
            string message = string.Empty;

            // Validate the decimal up front so bad input fails fast with a clear error
            if (!decimal.TryParse(distancekm, out decimal distanceValue))
            {
                return "Invalid distance value.";
            }

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = "INSERT INTO Categories (EventId, Name, DistanceKm, MaxParticipants) " +
                                    "VALUES (@EventId, @Name, @DistanceKm, @MaxParticipants);";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@EventId", eventId);
                        run_query.Parameters.AddWithValue("@Name", name);
                        run_query.Parameters.AddWithValue("@DistanceKm", distanceValue);
                        run_query.Parameters.AddWithValue("@MaxParticipants", maxparticipants);

                        run_query.ExecuteNonQuery();
                        message = "Category Created Successfully.";
                    }
                }
            }
            catch (Exception error)
            {
                message = error.Message;
            }

            return message;
        }



        public string deleting_category(int categoryId, int organiserId)
        {
            string message = string.Empty;
            string role = events.role_valiation(organiserId);

            if (role == "Organiser")
            {
                try
                {
                    using (SqlConnection connect = new SqlConnection(connection_string))
                    {
                        connect.Open();

                        // Delete from Categories (not Events), verifying ownership via the parent event
                        string query = @"DELETE c FROM Categories c
                                        JOIN Events e ON c.EventId = e.EventId
                                        WHERE c.CategoryId = @CategoryId AND e.OrganiserId = @OrganiserId;";

                        using (SqlCommand run_query = new SqlCommand(query, connect))
                        {
                            run_query.Parameters.AddWithValue("@CategoryId", categoryId);
                            run_query.Parameters.AddWithValue("@OrganiserId", organiserId);

                            int rows = run_query.ExecuteNonQuery();
                            if (rows > 0)
                            {
                                message = "Category Deleted Successfully.";
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
                message = "Category can not be deleted because of your role. You are not a organiser.";
            }

            return message;
        }


        public string editing_category(int category_id, int organiser_id, string name, string distancekm, int maxparticipant)
        {
            string message = string.Empty;

            if (!decimal.TryParse(distancekm, out decimal distanceValue))
            {
                return "Invalid distance value.";
            }

            string role = events.role_valiation(organiser_id);
            if (role == "Organiser")
            {
                try
                {
                    using (SqlConnection connect = new SqlConnection(connection_string))
                    {
                        connect.Open();

                        // Update by CategoryId; verify the organiser owns the parent event via a join
                        string query = @"UPDATE c SET c.Name = @Name,
                                                       c.DistanceKm = @DistanceKm,
                                                       c.MaxParticipants = @MaxParticipants
                                        FROM Categories c
                                        JOIN Events e ON c.EventId = e.EventId
                                        WHERE c.CategoryId = @CategoryId AND e.OrganiserId = @OrganiserId;";

                        using (SqlCommand run_query = new SqlCommand(query, connect))
                        {
                            run_query.Parameters.AddWithValue("@Name", name);
                            run_query.Parameters.AddWithValue("@DistanceKm", distanceValue);
                            run_query.Parameters.AddWithValue("@MaxParticipants", maxparticipant);
                            run_query.Parameters.AddWithValue("@CategoryId", category_id);
                            run_query.Parameters.AddWithValue("@OrganiserId", organiser_id);

                            int rows = run_query.ExecuteNonQuery();
                            if (rows > 0)
                            {
                                message = "Category Updated Successfully.";
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
                message = "Category can not be edited because of your role. You are not a organiser.";
            }

            return message;
        }


        public string get_category_route(int categoryId)
        {
            string message = string.Empty;

            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    string query = "SELECT Categories.Name, Categories.DistanceKm, Categories.MaxParticipants, " +
                        "Routes.StartPoint, Routes.EndPoint, Routes.ElevationGainM, Routes.MapUrl FROM Categories " +
                        "LEFT JOIN Routes ON Categories.CategoryId = Routes.CategoryId " +
                        "WHERE Categories.CategoryId = @CategoryId";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {
                        run_query.Parameters.AddWithValue("@CategoryId", categoryId);

                        using (SqlDataReader reader = run_query.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                message = $"{reader["Name"]} | {reader["DistanceKm"]}km | " +
                                           $"Max {reader["MaxParticipants"]} | " +
                                           $"{reader["StartPoint"]} -> {reader["EndPoint"]} | " +
                                           $"Elevation {reader["ElevationGainM"]}m | {reader["MapUrl"]}";
                            }
                            else
                            {
                                message = "No category found with that id.";
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
