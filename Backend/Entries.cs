using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;
using Part2_Raceday.Models;

namespace Part2_Raceday.Backend
{
    public class Entries
    {
        //Event object
        Events events = new Events();

        private const string connection_string = @"Data source=(localdb)\MSSQLLocalDB;database=Race_day;";
        /****************************/

        // > 0 = new EntryId, -1 = not a participant, -2 = category not found,
        // -3 = already entered, -4 = category full
        public int create_entry(int participant_id, int category_id)
        {
            if (events.role_valiation(participant_id) != "participant") return -1;

            using (SqlConnection connect = new SqlConnection(connection_string))
            {
                connect.Open();

                int max;
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT c.MaxParticipants, e.EventDate
                      FROM Categories c JOIN Events e ON c.EventId = e.EventId
                      WHERE c.CategoryId = @CategoryId;", connect))
                {
                    cmd.Parameters.AddWithValue("@CategoryId", category_id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read()) return -2;
                        max = reader.GetInt32(0);
                        if (reader.GetDateTime(1).Date < DateTime.Today) return -5;
                    }
                }

                using (SqlCommand cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Entries WHERE ParticipantId = @ParticipantId AND CategoryId = @CategoryId AND Status <> 'Cancelled';", connect))
                {
                    cmd.Parameters.AddWithValue("@ParticipantId", participant_id);
                    cmd.Parameters.AddWithValue("@CategoryId", category_id);
                    if ((int)cmd.ExecuteScalar() > 0) return -3;
                }

                using (SqlCommand cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Entries WHERE CategoryId = @CategoryId AND Status <> 'Cancelled';", connect))
                {
                    cmd.Parameters.AddWithValue("@CategoryId", category_id);
                    if ((int)cmd.ExecuteScalar() >= max) return -4;
                }

                string insert = @"INSERT INTO Entries (ParticipantId, CategoryId, EntryDate, Status)
                          OUTPUT INSERTED.EntryId
                          VALUES (@ParticipantId, @CategoryId, @EntryDate, 'Confirmed');";

                using (SqlCommand cmd = new SqlCommand(insert, connect))
                {
                    cmd.Parameters.AddWithValue("@ParticipantId", participant_id);
                    cmd.Parameters.AddWithValue("@CategoryId", category_id);
                    cmd.Parameters.AddWithValue("@EntryDate", DateTime.Today);
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        // Returns null if the user is not a participant
        public List<ParticipantEntryDto>? get_entries_for_participant(int participant_id)
        {
            if (events.role_valiation(participant_id) != "participant") return null;

            var list = new List<ParticipantEntryDto>();

            using (SqlConnection connect = new SqlConnection(connection_string))
            {
                connect.Open();

                string query = @"SELECT en.EntryId, en.CategoryId, en.EntryDate, en.Status,
                                e.EventId, e.Name, e.EventDate, c.Name, c.DistanceKm
                         FROM Entries en
                         JOIN Categories c ON en.CategoryId = c.CategoryId
                         JOIN Events e ON c.EventId = e.EventId
                         WHERE en.ParticipantId = @ParticipantId
                         ORDER BY e.EventDate;";

                using (SqlCommand cmd = new SqlCommand(query, connect))
                {
                    cmd.Parameters.AddWithValue("@ParticipantId", participant_id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new ParticipantEntryDto
                            {
                                EntryId = reader.GetInt32(0),
                                CategoryId = reader.GetInt32(1),
                                EntryDate = reader.GetDateTime(2),
                                Status = reader.GetString(3),
                                EventId = reader.GetInt32(4),
                                EventName = reader.GetString(5),
                                EventDate = reader.GetDateTime(6),
                                CategoryName = reader.GetString(7),
                                DistanceKm = reader.GetDecimal(8)
                            });
                        }
                    }
                }
            }

            return list;
        }

        // 1 = deleted, 0 = not found, -1 = entry already has a result
        public int delete_entry_by_id(int entry_id, int participant_id)
        {
            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    using (SqlCommand cmd = new SqlCommand("DELETE FROM Entries WHERE EntryId = @EntryId AND ParticipantId = @ParticipantId;", connect))
                    {
                        cmd.Parameters.AddWithValue("@EntryId", entry_id);
                        cmd.Parameters.AddWithValue("@ParticipantId", participant_id);
                        return cmd.ExecuteNonQuery() > 0 ? 1 : 0;
                    }
                }
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                return -1;
            }
        }

        /****************************/
        public string uploading_entry(int ParticipantId, string CategoryId, string EntryDate, string Status)
        {
            string message = string.Empty;

            //Converting the string "event_date" to DateTime
            Convert.ToDateTime(EntryDate);

            //EntryId ParticipantId CategoryId EntryDate Status

            string role = events.role_valiation(ParticipantId);

            if (role == "participant")
            {
                try
                {
                    using (SqlConnection connect = new SqlConnection(connection_string))
                    {
                        connect.Open();

                        string query = "INSERT INTO Entries (ParticipantId, CategoryId, EntryDate, Status) " +
                                        "VALUES (@ParticipantId, @CategoryId, @EntryDate, @Status);";

                        using (SqlCommand run_query = new SqlCommand(query, connect))
                        {
                            run_query.Parameters.AddWithValue("@ParticipantId", ParticipantId);
                            run_query.Parameters.AddWithValue("@CategoryId", CategoryId);
                            run_query.Parameters.AddWithValue("@EntryDate", EntryDate);
                            run_query.Parameters.AddWithValue("@Status", Status);

                            run_query.ExecuteNonQuery();
                            message = "Participant Entered Into The Event Successfully.";
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
                message = "User is not Entered Into The Event Unsuccessfully because of they don't exist.";
            }
                return message;
        }


        public List<string> get_participant_event_entries(int ParticipantId)
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

                        string query = "SELECT * FROM Entries WHERE ParticipantId = @ParticipantId;";

                        using (SqlCommand run_query = new SqlCommand(query, connect))
                        {
                            run_query.Parameters.AddWithValue("@ParticipantId", ParticipantId);

                            using (SqlDataReader reader = run_query.ExecuteReader())
                            {

                                if (reader.Read())
                                {
                                    do
                                    {
                                        //ParticipantId, CategoryId, EntryDate, Status
                                        message.Add("EntryId: " + reader["EntryId"] + ", " +
                                                  "ParticipantId: " + reader["ParticipantId"] + ", " +
                                                  "CategoryId: " + reader["CategoryId"] + ", " +
                                                  "EntryDate: " + Convert.ToDateTime(reader["EntryDate"]).ToString("yyyy-MM-dd") + ", " +
                                                  "Status: " + reader["Status"]);
                                    } while (reader.Read());
                                }
                                else
                                {
                                    //message
                                    message.Add("There is Events Entries no found.");
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


        public List<string> displaying_organiser_event_entries(int OrganiserId, int EventId)
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

                        // Update by CategoryId; verify the organiser owns the parent event via a join
                        string query = @"SELECT en.EntryId, en.ParticipantId, en.CategoryId, en.EntryDate, en.Status 
                                        FROM Entries en
                                        JOIN Categories c ON en.CategoryId = c.CategoryId
                                        JOIN Events e ON c.EventId = e.EventId
                                        WHERE e.EventId = @EventId AND e.OrganiserId = @OrganiserId;";

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
                                        message.Add($"[ EntryId: {reader["EntryId"]}, " +
                                                $"ParticipantId: {reader["ParticipantId"]}, " +
                                                $"CategoryId: {reader["CategoryId"]}, " +
                                                $"EntryDate: {reader["EntryDate"]}, " +
                                                $"Status: {reader["Status"]} ]");
                                    } while (reader.Read());
                                }
                                else
                                {
                                    message.Add("No Entry found with that id.");
                                }
                            }
                        }
                    }
                }
                catch (Exception error)
                {
                    message.Add(error.Message);
                }
            }
            else
            {
                message.Add("You are not a organiser.");
            }

            return message;
        }


        public string deleting_entry(int EntryId, int ParticipantId)
        {
            string message = string.Empty;

            string role = events.role_valiation(ParticipantId);

            if (role == "participant")
            {
                try
                {
                    using (SqlConnection connect = new SqlConnection(connection_string))
                    {
                        connect.Open();

                        string query = "DELETE Entries WHERE EntryId = @EntryId AND ParticipantId = @ParticipantId;";

                        using (SqlCommand run_query = new SqlCommand(query, connect))
                        {
                            run_query.Parameters.AddWithValue("@ParticipantId", ParticipantId);
                            run_query.Parameters.AddWithValue("@EntryId", EntryId);

                            run_query.ExecuteNonQuery();
                            message = "Participant Withdraws From A Event Successfully.";
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
                message = "User can not Withdraw From the Event because of they don't exist in the event.";
            }
            return message;
        }


    }
}
