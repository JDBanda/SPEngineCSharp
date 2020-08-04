﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace SPEngineCSharp
{
    public static class SPEngine
    {
        /// <summary>
        /// Execute the Stored Procedure with the defalt connection
        /// This method is for insert, update and delete db operations
        /// </summary>
        /// <param name="sp">Stored Procedure</param>
        public static Boolean ExecuteSP(string sp, List<Parameter> lst)
        {
            bool response;
            SqlCommand cmd;
            IDbConnection connection = ConnectionDB.Conection();
            connection.Open();
            try
            {
                cmd = new SqlCommand(sp, connection as SqlConnection);
                cmd.CommandType = CommandType.StoredProcedure;
                if (lst != null)
                {
                    foreach (Parameter parametro in lst)
                        if (parametro.Direction == ParameterDirection.Input)
                            cmd.Parameters.AddWithValue(parametro.Name, parametro.Value);
                        else if (parametro.Direction == ParameterDirection.Output)
                            cmd.Parameters.Add(parametro.Name, parametro.DataType, parametro.Size).Direction = ParameterDirection.Output;
                    response = Convert.ToBoolean(cmd.ExecuteNonQuery());
                    for (int i = 0; i < lst.Count; i++)
                        if (lst[i].Direction == ParameterDirection.Input)
                            lst[i].Value = cmd.Parameters[i].Value.ToString();
                }
                else
                {
                    //Debería existir un mensaje de error
                    response = false;
                }
            }
            catch (Exception ex)
            {
                response = false;
                throw ex;
            }
            finally
            {
                connection.Close();
            }
            return response;
        }
        /// <summary>
        /// Execute the Stored Procedure with a new connection, DON'T USE IT
        /// This method is for insert, update and delete db operations
        /// </summary>
        /// <param name="sp">Stored Procedure</param>
        public static void ExecuteSP(StoreProcedure sp, SqlConnection connection)
        {
            SqlCommand cmd;
            connection.Open();
            try
            {
                cmd = new SqlCommand(sp.Name, connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                if (sp.Parameters != null)
                {
                    foreach (Parameter parametro in sp.Parameters)
                        if (parametro.Direction == ParameterDirection.Input)
                            cmd.Parameters.AddWithValue(parametro.Name, parametro.Value);
                        else if (parametro.Direction == ParameterDirection.Output)
                            cmd.Parameters.Add(parametro.Name, parametro.DataType, parametro.Size).Direction = ParameterDirection.Output;
                    cmd.ExecuteNonQuery();
                    for (int i = 0; i < sp.Parameters.Count; i++)
                        if (sp.Parameters[i].Direction == ParameterDirection.Input)
                            sp.Parameters[i].Value = cmd.Parameters[i].Value.ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }

        /// <summary>
        /// Execute a consult Store Procedure with the default connection
        /// </summary>
        /// <param name="sp">Store Procedure</param>
        /// <param name="lst">Parameters List</param>
        /// <returns>The result content</returns>
        public static DataTable ExecuteConsultSP(string sp, List<Parameter> lst)
        {
            DataTable tableResult = new DataTable();
            //El adapter es como una carcasa mas grande que puede contener multiples SqlCommands
            SqlCommand _Command;
            SqlDataReader reader;
            using (IDbConnection connection = ConnectionDB.Conection())
            {
                connection.Open();
                try
                {
                    _Command = new SqlCommand(sp, connection as SqlConnection);
                    _Command.CommandText = sp;
                    _Command.CommandType = CommandType.StoredProcedure;
                    //Si existe lista de parámetros
                    //Si no existe
                    if (lst != null)
                    {
                        foreach (Parameter item in lst)
                        {
                            _Command.Parameters.AddWithValue(item.Name, item.Value);
                        }
                    }
                    else
                    {
                        _Command.Parameters.Clear();
                    }
                    //El reader guarda el resultado del comando ejecutado
                    reader = _Command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        tableResult.Load(reader);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return tableResult;
        }

        /// <summary>
        /// Execute a consult Store Procedure with a new connection DON'T USE IT
        /// </summary>
        /// <param name="sp">Store Procedure</param>
        /// <returns>The result content</returns>
        public static DataTable ExecuteConsultSP(StoreProcedure sp, SqlConnection connection)
        {
            DataTable tableResult = new DataTable();
            SqlDataAdapter adapter;

            connection.Open();
            try
            {
                adapter = new SqlDataAdapter(sp.Name, connection);
                if (sp.Parameters != null)
                    foreach (Parameter parametro in sp.Parameters)
                        adapter.SelectCommand.Parameters.AddWithValue(parametro.Name, parametro.Value);
                adapter.Fill(tableResult);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
            return tableResult;
        }
    }
}