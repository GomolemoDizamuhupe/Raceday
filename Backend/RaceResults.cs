using Microsoft.Data.SqlClient;
using Part2_Raceday.Models;
using System.Data;

namespace Part2_Raceday.Backend
{
    public class RaceResults
    {
        //Event object
        Events events = new Events();

        private const string connection_string = @"Data source=(localdb)\MSSQLLocalDB;database=Race_day;";
        /******************************/

        // Returns null if the user is not a participant
        public List<ParticipantResultDto>? get_results_for_participant(int participant_id)
        {
            if (events.role_valiation(participant_id) != "participant") return null;

            var list = new List<ParticipantResultDto>();

            using (SqlConnection connect = new SqlConnection(connection_string))
            {
                connect.Open();

                string query = @"SELECT re.ResultId, re.EntryId, e.Name, e.EventDate, c.Name, c.DistanceKm,
                                re.FinishTime, re.Position,
                                (SELECT COUNT(*) FROM Results r2
                                 JOIN Entries e2 ON r2.EntryId = e2.EntryId
                                 WHERE e2.CategoryId = en.CategoryId) AS TotalFinishers
                         FROM Results re
                         JOIN Entries en ON re.EntryId = en.EntryId
                         JOIN Categories c ON en.CategoryId = c.CategoryId
                         JOIN Events e ON c.EventId = e.EventId
                         WHERE en.ParticipantId = @ParticipantId
                         ORDER BY e.EventDate DESC;";

                using (SqlCommand cmd = new SqlCommand(query, connect))
                {
                    cmd.Parameters.AddWithValue("@ParticipantId", participant_id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new ParticipantResultDto
                            {
                                ResultId = reader.GetInt32(0),
                                EntryId = reader.GetInt32(1),
                                EventName = reader.GetString(2),
                                EventDate = reader.GetDateTime(3),
                                CategoryName = reader.GetString(4),
                                DistanceKm = reader.GetDecimal(5),
                                FinishTime = reader.GetTimeSpan(6),
                                Position = reader.IsDBNull(7) ? null : reader.GetInt32(7),
                                TotalFinishers = reader.GetInt32(8)
                            });
                        }
                    }
                }
            }

            return list;
        }


        // > 0 = new ResultId, -1 = entry not found or not owned by this organiser, -2 = result already exists
        public int create_result(int entry_id, ResultRequest req)
        {
            using (SqlConnection connect = new SqlConnection(connection_string))
            {
                connect.Open();

                string ownsQuery = @"SELECT COUNT(*) FROM Entries en
                             JOIN Categories c ON en.CategoryId = c.CategoryId
                             JOIN Events e ON c.EventId = e.EventId
                             WHERE en.EntryId = @EntryId AND e.OrganiserId = @OrganiserId;";

                using (SqlCommand owns = new SqlCommand(ownsQuery, connect))
                {
                    owns.Parameters.AddWithValue("@EntryId", entry_id);
                    owns.Parameters.AddWithValue("@OrganiserId", req.OrganiserId);
                    if ((int)owns.ExecuteScalar() == 0) return -1;
                }

                using (SqlCommand exists = new SqlCommand("SELECT COUNT(*) FROM Results WHERE EntryId = @EntryId;", connect))
                {
                    exists.Parameters.AddWithValue("@EntryId", entry_id);
                    if ((int)exists.ExecuteScalar() > 0) return -2;
                }

                string insert = @"INSERT INTO Results (EntryId, FinishTime, Position)
                          OUTPUT INSERTED.ResultId
                          VALUES (@EntryId, @FinishTime, @Position);";

                using (SqlCommand cmd = new SqlCommand(insert, connect))
                {
                    cmd.Parameters.AddWithValue("@EntryId", entry_id);
                    cmd.Parameters.AddWithValue("@FinishTime", req.FinishTime);
                    cmd.Parameters.AddWithValue("@Position", (object?)req.Position ?? DBNull.Value);
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        public bool update_result(int result_id, ResultRequest req)
        {
            using (SqlConnection connect = new SqlConnection(connection_string))
            {
                connect.Open();

                string query = @"UPDATE re SET re.FinishTime = @FinishTime, re.Position = @Position
                         FROM Results re
                         JOIN Entries en ON re.EntryId = en.EntryId
                         JOIN Categories c ON en.CategoryId = c.CategoryId
                         JOIN Events e ON c.EventId = e.EventId
                         WHERE re.ResultId = @ResultId AND e.OrganiserId = @OrganiserId;";

                using (SqlCommand cmd = new SqlCommand(query, connect))
                {
                    cmd.Parameters.AddWithValue("@FinishTime", req.FinishTime);
                    cmd.Parameters.AddWithValue("@Position", (object?)req.Position ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ResultId", result_id);
                    cmd.Parameters.AddWithValue("@OrganiserId", req.OrganiserId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // Returns null if the event isn't owned by this organiser
        public List<EventResultDto>? get_results_for_event(int event_id, int organiser_id)
        {
            var list = new List<EventResultDto>();

            using (SqlConnection connect = new SqlConnection(connection_string))
            {
                connect.Open();

                using (SqlCommand owns = new SqlCommand("SELECT COUNT(*) FROM Events WHERE EventId = @EventId AND OrganiserId = @OrganiserId;", connect))
                {
                    owns.Parameters.AddWithValue("@EventId", event_id);
                    owns.Parameters.AddWithValue("@OrganiserId", organiser_id);
                    if ((int)owns.ExecuteScalar() == 0) return null;
                }

                string query = @"SELECT re.ResultId, re.EntryId, u.Name, u.Surname, c.Name, re.FinishTime, re.Position
                         FROM Results re
                         JOIN Entries en ON re.EntryId = en.EntryId
                         JOIN Users u ON en.ParticipantId = u.UserId
                         JOIN Categories c ON en.CategoryId = c.CategoryId
                         WHERE c.EventId = @EventId
                         ORDER BY c.Name, re.Position;";

                using (SqlCommand cmd = new SqlCommand(query, connect))
                {
                    cmd.Parameters.AddWithValue("@EventId", event_id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new EventResultDto
                            {
                                ResultId = reader.GetInt32(0),
                                EntryId = reader.GetInt32(1),
                                ParticipantName = reader.GetString(2) + " " + reader.GetString(3),
                                CategoryName = reader.GetString(4),
                                FinishTime = reader.GetTimeSpan(5),
                                Position = reader.IsDBNull(6) ? null : reader.GetInt32(6)
                            });
                        }
                    }
                }
            }

            return list;
        }
        /*********************************/
        public string uploading_results(int EntryId, string FinishTime, int Position, int OrganiserId)
        {
            string message = string.Empty;

            //Validating the finish time (HH:mm:ss)
            if (!TimeSpan.TryParse(FinishTime, out TimeSpan parsedTime))
            {
                return "Invalid finish time format. Use HH:mm:ss.";
            }

            string role = events.role_valiation(OrganiserId);

            if (role == "organiser")
            {
                try
                {
                    using (SqlConnection connect = new SqlConnection(connection_string))
                    {
                        connect.Open();

                        string query = "INSERT INTO Results (EntryId, FinishTime, Position) " +
                                       "VALUES (@EntryId, @FinishTime, @Position);";

                        using (SqlCommand run_query = new SqlCommand(query, connect))
                        {
                            run_query.Parameters.AddWithValue("@EntryId", EntryId);
                            run_query.Parameters.AddWithValue("@FinishTime", parsedTime);
                            run_query.Parameters.AddWithValue("@Position", Position);

                            run_query.ExecuteNonQuery();
                            message = "Organiser Has captured the Results of a participant Successfully.";
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
                message = "User Has not Entered The Event Unsuccessfully because of they don't exist.";
            }
            return message;
        }


        public string updating_results(int ResultId, string FinishTime, int Position, int OrganiserId)
        {
            string message = string.Empty;

            //Validating the finish time (HH:mm:ss)
            if (!TimeSpan.TryParse(FinishTime, out TimeSpan parsedTime))
            {
                return "Invalid finish time format. Use HH:mm:ss.";
            }

            string role = events.role_valiation(OrganiserId);

            if (role == "organiser")
            {
                try
                {
                    using (SqlConnection connect = new SqlConnection(connection_string))
                    {
                        connect.Open();

                        string query = "UPDATE Results SET FinishTime = @FinishTime, Position = @Position WHERE ResultId = @ResultId;";

                        using (SqlCommand run_query = new SqlCommand(query, connect))
                        {
                            run_query.Parameters.AddWithValue("@FinishTime", parsedTime);
                            run_query.Parameters.AddWithValue("@Position", Position);
                            run_query.Parameters.AddWithValue("@ResultId", ResultId);

                            run_query.ExecuteNonQuery();
                            message = "Organiser Has updated the Results of a participant Successfully.";
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
                message = "Results Has not been updated Unsuccessfully because it doesn't exist.";
            }
            return message;
        }


        public List<string> get_participant_event_results(int ParticipantId)
        {
            List<string> message = new List<string>();

            string role = events.role_valiation(ParticipantId);

            if (role == "participant")
            {
                try
                {
                    using (SqlConnection connect = new SqlConnection(connection_string))
                    {
                        connect.Open();

                        string query = "SELECT re.ResultId, re.EntryId, re.FinishTime, re.Position, " +
                                       "ca.Name AS CategoryName, ca.DistanceKm, en.Name AS EventName " +
                                       "FROM Results re " +
                                       "JOIN Entries es ON es.EntryId = re.EntryId " +
                                       "JOIN Categories ca ON ca.CategoryId = es.CategoryId " +
                                       "JOIN Events en ON en.EventId = ca.EventId " +
                                       "WHERE es.ParticipantId = @ParticipantId;";

                        using (SqlCommand run_query = new SqlCommand(query, connect))
                        {
                            run_query.Parameters.AddWithValue("@ParticipantId", ParticipantId);

                            using (SqlDataReader reader = run_query.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    do
                                    {
                                        message.Add("ResultId: " + reader["ResultId"] + ", " +
                                                    "EntryId: " + reader["EntryId"] + ", " +
                                                    "Finish Time: " + ((TimeSpan)reader["FinishTime"]).ToString(@"hh\:mm\:ss") + ", " +
                                                    "Position: " + reader["Position"] + ", " +
                                                    "Category Name: " + reader["CategoryName"] + ", " +
                                                    "Distance: " + reader["DistanceKm"] + ", " +
                                                    "Event Name: " + reader["EventName"]);
                                    } while (reader.Read());
                                }
                                else
                                {
                                    message.Add("There are no results for this user found.");
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    message.Add(e.Message);
                }
            }
            else
            {
                message.Add("User not found");
            }
            return message;
        }


        public List<string> get_event_results(int EventId, int OrganiserId)
        {
            List<string> message = new List<string>();

            string role = events.role_valiation(OrganiserId);

            if (role == "organiser")
            {
                try
                {
                    using (SqlConnection connect = new SqlConnection(connection_string))
                    {
                        connect.Open();

                        string query = "SELECT re.ResultId, re.EntryId, re.FinishTime, re.Position FROM Results re " +
                                                "JOIN Entries es " +
                                                "ON es.EntryId = re.EntryId " +
                                                "JOIN Categories ca " +
                                                "ON ca.CategoryId = es.CategoryId " +
                                                "JOIN Events en " +
                                                "ON en.EventId = ca.EventId " +
                                                "JOIN Users us " +
                                                "ON us.UserId = en.OrganiserId " +
                                                "WHERE us.UserId = @OrganiserId AND en.EventId = @EventId;";

                        using (SqlCommand run_query = new SqlCommand(query, connect))
                        {
                            run_query.Parameters.AddWithValue("@OrganiserId", OrganiserId);
                            run_query.Parameters.AddWithValue("@EventId", EventId);

                            using (SqlDataReader reader = run_query.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    do
                                    {
                                        message.Add("ResultId: " + reader["ResultId"] + ", " +
                                                    "EntryId: " + reader["EntryId"] + ", " +
                                                    "Finish Time: " + ((TimeSpan)reader["FinishTime"]).ToString(@"hh\:mm\:ss") + ", " +
                                                    "Position: " + reader["Position"]);
                                    } while (reader.Read());
                                }
                                else
                                {
                                    message.Add("There are no results for this organiser's event found.");
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    message.Add(e.Message);
                }
            }
            else
            {
                message.Add("User not found");
            }
            return message;
        }

    }
}