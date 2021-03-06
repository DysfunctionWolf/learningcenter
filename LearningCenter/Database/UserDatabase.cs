﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LearningCenter.Users;
using MySql.Data.MySqlClient;

namespace LearningCenter.Database
{
    public class UserDatabase : Database
    {

        public UserDatabase(MySqlConnection connection)
        {
            AssignConnection(connection);
        }

        public User GetUserByID(int id)
        {
            using (_connection)
            {
                _connection.Open();
                MySqlDataReader reader = GetReader(String.Format(@"SELECT * FROM users WHERE id='{0}';", id), _connection);
                User returnUser = UsersFromReader(reader)[0];
                _connection.Close();
                return returnUser;
            }
        }

        public List<User> GetAllUsers()
        {
            List<User> returnUsers = new List<User>();
            using (_connection)
            {
                _connection.Open();
                MySqlDataReader reader = GetReader(@"SELECT * FROM users", _connection);
                returnUsers.AddRange(UsersFromReader(reader));
                _connection.Close();
            }
            return returnUsers;
        }

        public List<User> GetAllSubordinates(User Superior)
        {
            List<User> returnUsers = new List<User>();
            using (_connection)
            {
                _connection.Open();
                string command = string.Format(@"SELECT * FROM users WHERE id in (SELECT employee_id FROM employee_hierarchey where superior_id = '{0}');", Superior.ID);
                MySqlDataReader reader = GetReader(command, _connection);
                returnUsers.AddRange(UsersFromReader(reader));
                _connection.Close();
            }
            return returnUsers;
        }

        public int AddUser(User user)
        {
            int newId = -1;
            using (_connection)
            {
                try
                {
                    _connection.Open();
                    string commandString = string.Format(@"INSERT INTO users(firstname, lastname, password) VALUES('{0}', '{1}', '1234'); SELECT * FROM `users` WHERE `id`= LAST_INSERT_ID();", user.FirstName, user.LastName);
                    MySqlDataReader reader = GetReader(commandString, _connection);
                    if (reader.Read())
                    {
                        newId = (int)reader[0];
                    }
                    _connection.Close();
                }
                catch (InvalidOperationException e)
                {
                    Console.WriteLine(e.Message);
                }
                catch (MySqlException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            return newId;
        }

        public bool VerifyUser(int userID, string userPassword)
        {
            StringBuilder commandBuilder = new StringBuilder();
            string commandString = string.Format(@"SELECT * FROM users WHERE id='{0}' AND password='{1}';", userID, userPassword);

            using (_connection)
            {
                try
                {
                    _connection.Open();

                    MySqlDataReader reader = GetReader(commandString, _connection);

                    if (reader.Read())
                    {
                        _connection.Close();
                        return true;
                    }
                    else
                    {
                        _connection.Close();
                        return false;
                    }
                }
                catch (InvalidOperationException e)
                {
                    Console.WriteLine(e.Message);
                    return false;
                }
                catch (MySqlException e)
                {
                    Console.WriteLine(e.Message);
                    return false;
                }
            }
        }

        public void RemoveUsers(List<User> users)
        {
            StringBuilder sb = new StringBuilder();

            foreach (User user in users)
            {
                sb.Append(string.Format(@"DELETE FROM users WHERE id='{0}';", user.ID));
            }
            ExecuteNonQuery(sb.ToString());
        }

        public void EditUser(User editedUser)
        {
            String queryString = string.Format(
                @"UPDATE users SET firstname='{0}', lastname='{1}' WHERE id='{2}';"
                , editedUser.FirstName, editedUser.LastName, editedUser.ID);

            ExecuteNonQuery(queryString);
        }

        protected void ExecuteNonQuery(string queryString)
        {
            using (_connection)
            {
                try
                {
                    _connection.Open();
                    new MySqlCommand(queryString, _connection).ExecuteNonQuery();
                    _connection.Close();
                }
                catch (InvalidOperationException e)
                {
                    Console.WriteLine(e.Message);
                }
                catch (MySqlException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        protected User[] UsersFromReader(MySqlDataReader reader)
        {
            List<User> users = new List<User>();
            while (reader.Read())
            {
                users.Add(new User(reader[0].ToString(), (string)reader[1], (string)reader[2]));
            }
            return users.ToArray();
        }
        // returns a data reader from a command
        protected MySqlDataReader GetReader(string commandString, MySqlConnection connection)
        {
            return new MySqlCommand(commandString, connection).ExecuteReader();
        }
    }
}
