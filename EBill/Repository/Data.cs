using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EBill.Models;
using EBill.Repository;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace EBill.Repository
{
    public class Data : IData
    {
        public string dbString { get; set; }

        public Data() 
        {
            dbString = ConfigurationManager.ConnectionStrings["dbConnection"].ConnectionString;
        }
        public void SaveBillDetails(BillDetail details)
        {
            SqlConnection connection = new SqlConnection(dbString);

            try
            {
                details.TotalAmount = details.Items.Sum(i => i.Price * i.Quantity);
                connection.Open();
                SqlCommand cmd = new SqlCommand("sqt_saveEBillDetails", connection);
                cmd.CommandType= CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CustomerName", details.CustomerName);
                cmd.Parameters.AddWithValue("@MobileNumber", details.MobileNumber);
                cmd.Parameters.AddWithValue("@Address", details.Address);
                cmd.Parameters.AddWithValue("@TotalAmount", details.TotalAmount);

                SqlParameter outputParameter = new SqlParameter();
                outputParameter.DbType = DbType.Int32;
                outputParameter.Direction= ParameterDirection.Output;
                outputParameter.ParameterName = "@Id";
                cmd.Parameters.Add(outputParameter);
                cmd.ExecuteNonQuery();
                int id = int.Parse(outputParameter.Value.ToString());

                if(details.Items.Count > 0)
                {
                    SaveBillItems(details.Items, connection, id);
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

        public void SaveBillItems(List<Items> items, SqlConnection con, int id)
        {
            try
            {
                string query = "insert into BillItems(ProductName, Price, Quantity, BillId) values";

                foreach (var item in items)
                {
                    query += String.Format("('{0}', {1}, {2}, {3})", item.ProductName, item.Price, item.Quantity, id);
                }
                query = query.Remove(query.Length - 1);
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<BillDetail> GetAllDetails()
        {
            List<BillDetail> list = new List<BillDetail>();
            BillDetail detaill;
            SqlConnection connection = new SqlConnection(dbString);

            try
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand("sqt_getAllEBillDetails", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                SqlDataReader reader = cmd.ExecuteReader();

                while(reader.Read())
                {
                    detaill = new BillDetail();
                    detaill.Id = int.Parse(reader["Id"].ToString());
                    detaill.CustomerName = reader["CustomerName"].ToString();
                    detaill.MobileNumber = reader["MobileNumber"].ToString();
                    detaill.Address = reader["Address"].ToString();
                    detaill.TotalAmount = int.Parse(reader["TotalAmount"].ToString());
                    list.Add(detaill);
                }
            }
            catch(Exception) 
            {
                throw;
            }
            finally
            {
                connection.Close();
            }
            return list;
        }

        public BillDetail GetDetail(int id)
        {
            SqlConnection connection = new SqlConnection(dbString);
            BillDetail detail = new BillDetail();
            Items item;

            try
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand("spt_getEBillDetails", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", id);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    detail.Id = int.Parse(reader["BillId"].ToString());
                    detail.CustomerName = reader["CustomerName"].ToString();
                    detail.MobileNumber = reader["MobileNumber"].ToString();
                    detail.Address = reader["Address"].ToString();
                    detail.TotalAmount = int.Parse(reader["TotalAmount"].ToString());
                }
                while (reader.Read())
                {
                    item = new Items();
                    item.Id = int.Parse(reader["ItemId"].ToString());
                    item.ProductName = reader["ProductName"].ToString();
                    item.Price = int.Parse(reader["Price"].ToString());
                    item.Quantity = int.Parse(reader["Quantity"].ToString());
                    detail.Items.Add(item);
                }
            }
            catch(Exception)
            {
                throw;
            }
            finally
            {
                connection.Close();
            }
            return detail;
        }
    }
}