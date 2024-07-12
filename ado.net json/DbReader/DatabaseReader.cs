using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ado.net_json.DbReader
{

    public static class DatabaseReader
    {
        static string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DbConnectionStr"].ConnectionString;

        /// <summary>
        /// Get Data From db using sql Query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlQuery"></param>
        /// <param name="sqlParameters"></param>
        /// <returns></returns>
        public static T GetDataFromQuery<T>(string sqlQuery, object sqlParameters)
         where T : class, new()
        {
            DataTable dt = new DataTable();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {

                    using (var command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.ConvertToSqlParams(sqlParameters);

                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = 0;

                        connection.Open();

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader != null)
                            {
                                dt.Load(reader);
                            }


                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {

                    connection.Close();

                }
            }


            if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(List<>))
            {
                var itemType = typeof(T).GetGenericArguments()[0];
                var method = typeof(DatabaseReader).GetMethod("ConvertToList", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                                            .MakeGenericMethod(itemType);
                return (T)method.Invoke(null, new object[] { dt });
            }
            else
            {
                return ConvertToObject<T>(dt);
            }
        }

        private static List<T> ConvertToList<T>(DataTable dt) where T : class, new()
        {
            var list = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                Dictionary<string, object> valuePairs = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    valuePairs[col.ColumnName] = row[col.ColumnName];
                }
                var obj = new T();
                list.Add(AddDataToClass<T>(obj, valuePairs));
            }
            return list;
        }

        private static T ConvertToObject<T>(DataTable dt) where T : class, new()
        {
            if (dt.Rows.Count == 0)
            {
                return new T();
            }

            var row = dt.Rows[0];
            Dictionary<string, object> valuePairs = new Dictionary<string, object>();
            foreach (DataColumn col in dt.Columns)
            {
                valuePairs[col.ColumnName] = row[col.ColumnName];
            }
            var obj = new T();
            return AddDataToClass<T>(obj, valuePairs);
        }
        public static T AddDataToClass<T>(T genericClassObj, Dictionary<string, object> valuePair) where T : class, new()
        {
            foreach (var property in genericClassObj.GetType().GetProperties())
            {
                var result = valuePair.Where(x => x.Key.ToLower() == property.Name.ToLower()).FirstOrDefault();
                if (result.Key != null)
                {
                    var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                    if (type == typeof(int))
                    {
                        property.SetValue(genericClassObj, Convert.ToInt32(result.Value), null);

                    }
                    else if (type == typeof(string))
                    {
                        property.SetValue(genericClassObj, Convert.ToString(result.Value), null);

                    }
                    else if (type == typeof(decimal))
                    {
                        property.SetValue(genericClassObj, Convert.ToDecimal(result.Value), null);

                    }
                    else if (type == typeof(DateTime))
                    {
                        property.SetValue(genericClassObj, Convert.ToDateTime(Convert.ToString(result.Value)), null);

                    }

                }

            }
            return genericClassObj;
        }

        /// <summary>
        /// Get Data From db using sql store proc
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storedProcedure"></param>
        /// <param name="sqlParameters"></param>
        /// <returns></returns>
        public static T GetDataFromSP<T>(string storedProcedure, object sqlParameters)
         where T : class, new()
        {
            DataTable dt = new DataTable();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {

                    using (var command = new SqlCommand(storedProcedure, connection))
                    {
                        command.Parameters.ConvertToSqlParams(sqlParameters);

                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 0;

                        connection.Open();

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader != null)
                            {
                                dt.Load(reader);
                            }


                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {

                    connection.Close();

                }
            }


            if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(List<>))
            {
                var itemType = typeof(T).GetGenericArguments()[0];
                var method = typeof(DatabaseReader).GetMethod("ConvertToList", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                                            .MakeGenericMethod(itemType);
                return (T)method.Invoke(null, new object[] { dt });
            }
            else
            {
                return ConvertToObject<T>(dt);
            }
        }

        private static void ConvertToSqlParams(this SqlParameterCollection collection, object parameters)
        {

            if (parameters != null)
            {
                foreach (PropertyInfo property in parameters.GetType().GetProperties())
                {
                    var parameter = new SqlParameter("@" + property.Name, null);
                    parameter.Value = Convert.ToString(property.GetValue(parameters));

                    collection.Add(parameter);

                }
            }

        }
    }
}
