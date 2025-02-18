using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopSpeed.Application.ApplicationConstants
{
    public class ApplicationConstants
    {
    }

    public static class CommonMessage 
    {
        public static string RecordCreated = "Record Create Successfully";
        public static string RecordUpdate = "Record Update Successfully";
        public static string RecordDelete = "Record Delete Successfully";
    }

    public static class CustomRole
    {
        public const string MasterAdmin = "MASTERADMIN";
        public const string Admin = "ADMIN";
        public const string Customer = "CUSTOMER";
    }


}
