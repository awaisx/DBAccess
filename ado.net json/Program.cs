using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Dynamic;
using Newtonsoft.Json.Linq;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Xml.Linq;
using System.ComponentModel;
using System.Diagnostics;
using ado.net_json.DbReader;
namespace ado.net_json
{
    public class Employee
    {
        public int EmployeeID { get; set; }
        public string Firstname { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime HireDate { get; set; }
        public string Department { get; set; }
        public decimal Salary { get; set; }

        public override string ToString()
        {
            return $"ID: {EmployeeID}, Name: {Firstname} {LastName}, DOB: {DateOfBirth.ToShortDateString()}, Email: {Email}, Phone: {Phone}, Hire Date: {HireDate.ToShortDateString()}, Department: {Department}, Salary: {Salary:C}";
        }
    }

    public static class Program
    {

        static void Main(string[] args)
        {
            
            string query = "SELECT * FROM Employees WHERE FirstName = @FirstName AND LastName = @LastName";
            //string query = "SELECT * FROM Employees"; //without params

            var EmpList = DatabaseReader.FetchDataFromQuery<Employee>(query, new
            {
                FirstName = "John",
                LastName = "Doe"

            }).FirstOrDefault();

            Console.WriteLine(EmpList.ToString());
        }

    }
   
}
