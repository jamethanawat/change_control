using ChangeControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Npgsql;
using System.Web.Routing;
using System.Dynamic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ChangeControl.Models{
    public class UserModel{
        private DbTapics DB_Tapics;
        private DbCCS DB_CCS;
        public List<Department> A = new List<Department>();
        private SqlConnection cn = new SqlConnection(ConfigurationManager.ConnectionStrings["CCS"].ConnectionString);
        public UserModel(){
            DB_Tapics = new DbTapics();
            DB_CCS = new DbCCS();
        }

        public bool InsertUser(string user, string name, string dept, int position, string email){
            try{
                var sql = $@"INSERT INTO [User] (Code, Name, Dept, [Position], Email) VALUES('{user}', '{name}', '{dept}', (SELECT Name FROM [Position] WHERE ID = {position}), '{email}')
                             INSERT INTO Permission ([User], Department, [Rank], Active, Subscribe) VALUES('{user}', '{dept}', 'Staff', 1, 1);";
                DB_CCS.Database.ExecuteSqlCommand(sql);
                return true;
            }catch(Exception ex){
                return false;
            }
        }
        public bool InsertPermission(string user, string dept,int receive_mail){
            try{
                var sql = $@"INSERT INTO Permission ([User], Department, [Rank], Active, Subscribe) VALUES('{user}', '{dept}', 'Staff', 1, {receive_mail});";
                DB_CCS.Database.ExecuteSqlCommand(sql);
                return true;
            }catch(Exception ex){
                return false;
            }
        }

        public bool CheckExistsUser(string user){
            try{
                var sql = $@"SELECT CASE WHEN EXISTS 
                                    (SELECT Code FROM [User] WHERE Code LIKE '{user}') 			
                                THEN CAST (1 AS BIT)
                                ELSE CAST (0 AS BIT)
                            END	AS [Result]";
                var result = DB_CCS.Database.SqlQuery<bool>(sql).First();
                return result;
            }catch(Exception ex){
                return false;
            }
        }

        public bool CheckExistsPermission(string user,string dept){
            try{
                var sql = $@"SELECT CASE WHEN EXISTS 
                                    (SELECT * FROM Permission WHERE [User] = '{user}' AND Department = '{dept}') 			
                                THEN CAST (1 AS BIT)
                                ELSE CAST (0 AS BIT)
                            END	AS [Result]";
                var result = DB_CCS.Database.SqlQuery<bool>(sql).First();
                return result;
            }catch(Exception ex){
                return false;
            }
        }

        public List<String> GetDepartmentList(){
            var sql = $"SELECT Name FROM Department WHERE Name != 'Guest';";
            var result = DB_CCS.Database.SqlQuery<String>(sql).ToList();
            return result;
        }
        public List<string> GetPosition(){
            var sql = $@"SELECT Name FROM [Position] WHERE Permission IS NULL;";
                var result = DB_CCS.Database.SqlQuery<string>(sql).ToList();
                return result;
        }
        public List<string> GetUserCodeByDept(string dept){
            var sql = $@"SELECT Code FROM [User]
                        LEFT JOIN Department d1 ON Dept = d1.Name 
                        WHERE [Group] = (SELECT [GROUP] FROM Department d2 WHERE d2.Name = '{dept}')";
                var result = DB_CCS.Database.SqlQuery<string>(sql).ToList();
                return result;
        }
        public List<string> GetDepartmentByGroup(string dept){
            var sql = $@"SELECT Name FROM Department d 
                        WHERE [Group] = (SELECT [GROUP] FROM Department d2 WHERE d2.Name = '{dept}')";
                var result = DB_CCS.Database.SqlQuery<string>(sql).ToList();
                return result;
        }

        public List<string> GetAdminPosition(){
            var sql = $@"SELECT Name FROM [Position];";
                var result = DB_CCS.Database.SqlQuery<string>(sql).ToList();
                return result;
        }

        public List<UserWithPermission> GetUserByDeptGroup(string dept){
            var sql = $@"SELECT DISTINCT [User].*, Code as [User], Active, Subscribe FROM [User]
                        LEFT JOIN Department d1 ON Dept = d1.Name 
                        LEFT JOIN Permission p ON p.[User] = [User].Code AND p.Department = [User].Dept
                        WHERE [Group] = (SELECT [GROUP] FROM Department d2 WHERE d2.Name = '{dept}');";
            var result = DB_CCS.Database.SqlQuery<UserWithPermission>(sql).ToList();
            return result;
        }

        public List<UserWithPermission> GetPermissionByUser(string user, string dept){
            var sql = $@"SELECT Department as Dept, Active, Subscribe FROM Permission LEFT JOIN Department d ON d.Name = Department WHERE [User] = '{user}' AND Department != '{dept}' ORDER BY d.[Group], d.ID;";
            var result = DB_CCS.Database.SqlQuery<UserWithPermission>(sql).ToList();
            return result;
        }

        public string UpdatePosition(string user, string pos){
            try{
                var sql = $@"UPDATE [User] SET [Position]='{pos}' WHERE Code='{user}';";
                DB_CCS.Database.ExecuteSqlCommand(sql);
                return "success";
            }catch(Exception ex){
                return "error";
            }
        }
        
        public string UpdateSubscribe(string user, string dept, int status){
            try{
                var sql = $@"UPDATE Permission SET Subscribe={status} WHERE [User]='{user}' AND Department='{dept}';";
                DB_CCS.Database.ExecuteSqlCommand(sql);
                return "success";
            }catch(Exception ex){
                return "error";
            }
        }

        public string DeletePermission(string dept, string user){
            try{
                var sql = $@"DELETE FROM Permission WHERE Department = '{dept}' AND [User] = '{user}';";
                DB_CCS.Database.ExecuteSqlCommand(sql);
                return "success";
            }catch(Exception ex){
                return "error";
            }
        }
        public string DeleteUser(string user){
            try{
                var sql = $@"DELETE FROM Permission WHERE [User] = '{user}'
                            DELETE FROM [User] WHERE Code='{user}';";
                DB_CCS.Database.ExecuteSqlCommand(sql);
                return "success";
            }catch(Exception ex){
                return "error";
            }
        }

    }
        
}