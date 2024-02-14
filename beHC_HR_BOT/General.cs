using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beHC_HR_BOT
{
    public class General
    {
        public static apiResponse GetResponse(object data, long tNoOfRecord, string message, ResponseCode responseCode)
        {
            apiResponse response = new apiResponse
            {
                ResponseCode = Convert.ToInt16(responseCode),
                ResponseStatus = message,
                NoOfRecord = tNoOfRecord,
                Data = data
            };
            return response;
        }
    }

    public enum ResponseCode
    {
        Success = 100,
        Duplicate = 101,
        InvalidEmployeeID = 102,
        NoRecordFound = 103,
        Error = 104,
        InValidExcelTemplate = 105,
        UnAuthrized = 401,
        InvalidRequest = 99,
        SeesionExpired = 440
    }
}
