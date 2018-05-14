using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace ChangingFace.Model
{
    public class DataStoreAccess : IDataStoreAccess
    {
        private SQLiteConnection _sqLiteConnection;

        public DataStoreAccess(string databasePath)
        {
            _sqLiteConnection = new SQLiteConnection($"Data Source={databasePath};Version=3");
        }

        public bool DeleteUser(string userName)
        {
            var result = false;
            try
            {
                _sqLiteConnection.Open();
                var query = "Delete from Faces where Username = @username";

                var command = new SQLiteCommand(query, _sqLiteConnection);
                command.Parameters.AddWithValue("username", userName);

                var res = command.ExecuteNonQuery();

                if (res > 0)
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _sqLiteConnection.Close();
            }
            return result;
        }

        public int GenerateUserId() => Convert.ToInt32(DateTime.Now.ToString("MMddHHmmss"));

        public IEnumerable<string> GetAllUserNames()
        {
            var result = new List<string>();
            try
            {
                _sqLiteConnection.Open();
                var query = "Select distinct Username from Faces";

                var command = new SQLiteCommand(query, _sqLiteConnection);

                var reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        result.Add(reader["Username"].ToString());
                    }
                    result.Sort();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _sqLiteConnection.Close();
            }
            return result;
        }

        public IEnumerable<Face> GetFaces(string userName)
        {
            var faces = new List<Face>();
            try
            {
                _sqLiteConnection.Open();
                var query = string.Equals(userName.ToLower(), "ALL_USERS".ToLower()) ?
                            "Select * from Faces" : "Select * from Faces where Username = @username";
                var command = new SQLiteCommand(query, _sqLiteConnection);
                if (!string.Equals(userName.ToLower(), "ALL_USERS".ToLower()))
                {
                    command.Parameters.AddWithValue("username", userName);
                }
                var result = command.ExecuteReader();
                 
                if (!result.HasRows)
                {
                    faces = null;
                }
                else
                {
                    while (result.Read())
                    {
                        var face = new Face
                        {
                            Image = (byte[])result["FaceSample"],
                            Id = Convert.ToInt32(result["Id"]),
                            UserName = result["Username"].ToString(),
                            UserId = Convert.ToInt32(result["UserId"])
                        };
                        faces.Add(face);
                    }
                    faces = faces.OrderBy(f => f.Id).ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _sqLiteConnection.Close();
            }
            return faces;
        }

        public int GetUserId(string username)
        {
            var result = 0;
            try
            {
                _sqLiteConnection.Open();
                var query = "Select UserId from Faces where Username = @username limit 1";
                var command = new SQLiteCommand(query, _sqLiteConnection);
                command.Parameters.AddWithValue("username", username);

                var reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        result = Convert.ToInt32(reader["UserId"]);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _sqLiteConnection.Close();
            }
            return result;
        }

        public string GetUserName(int userId)
        {
            var result = "";
            try
            {
                _sqLiteConnection.Open();
                var query = "Select Username from Faces where UserId = @userId limit 1";

                var command = new SQLiteCommand(query, _sqLiteConnection);
                command.Parameters.AddWithValue("userId", userId);

                var reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        result = reader["Username"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _sqLiteConnection.Close();
            }
            return result;
        }

        public string SaveFace(string username, byte[] faceBlob)
        {
            try
            {
                var existingUserId = GetUserId(username);
                if (existingUserId == 0)
                {
                    existingUserId = GenerateUserId();
                }
                _sqLiteConnection.Open();
                var insertQuery = $"INSERT INTO Faces(Username, FaceSample, UserId) VALUES (@username, @faceSample, @userId)";
                var command = new SQLiteCommand(insertQuery, _sqLiteConnection);
                command.Parameters.AddWithValue("username", username);
                command.Parameters.AddWithValue("userId", existingUserId);
                command.Parameters.Add("faceSample", System.Data.DbType.Binary, faceBlob.Length).Value = faceBlob;
                var result = command.ExecuteNonQuery();
                return $"{result} faces saved successfully";
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _sqLiteConnection.Close();
            }
        }
    }
}
