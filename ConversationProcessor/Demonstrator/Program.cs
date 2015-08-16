using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


using Npgsql;
using System.Data;

using IODWrapper;

using EntityPack;


namespace Demonstrator
{

	class Program
	{

		static void Main(string[] args)
		{
            List<RawConversation> rawList = GetConversationList();
            List<RawConversation> uniqueList = GetUniqueConversationList(rawList);
            List<ProcessedConversation> processedConversation = GetProcessedConversationList(uniqueList);

            SaveToDataStore(processedConversation);
		}

        private static List<RawConversation> GetConversationList()
        {
            List<RawConversation> rawMessageList = new List<RawConversation>();

            NpgsqlConnection conn = new NpgsqlConnection("Server=52.74.179.57;Port=5432;UserId=postgres;Password=1234;Database=smsdb;");

            conn.Open();

            try
            {
                #region Retrieve Data
                // Define a query returning a single row result set

                string query = @"
                                    select M.phone_number, vm, rm, U.location, U.nationality, U.age_group, U.gender
                                    from
                                    (
	                                    select phone_number, sms_content as vm, null as rm
	                                    from raw_conversation
	                                    where ""from"" like 'Vol%'
	                                    union
	                                    select phone_number, null as vm, sms_content as rm
	                                    from raw_conversation
	                                    where ""from"" not like 'Vol%'
                                    ) M
                                    INNER JOIN 
                                    user_data U
                                    ON (M.phone_number = U.contact_number)
                                ";

                NpgsqlCommand command = new NpgsqlCommand(query, conn);

                // Execute the query and obtain a result set
                NpgsqlDataReader dr = command.ExecuteReader();

                // Output rows
                while (dr.Read())
                {
                    //V.phone_number, vm, rm, U.location, U.nationality, U.age_group, U.gender
                    RawConversation message = new RawConversation();
                    message.PhoneNumber = dr[0].ToString();
                    message.VolunteerMessage = dr[1].ToString();
                    message.ReporterMessage = dr[2].ToString();
                    message.Location = dr[3].ToString();
                    message.Nationality = dr[4].ToString();
                    message.AgeGroup = Convert.ToInt32(dr[5]);
                    message.Gender = dr[6].ToString();

                    rawMessageList.Add(message);
                }
                #endregion

            }

            finally
            {
                conn.Close();
            }

            return rawMessageList;
        }

        private static List<RawConversation> GetUniqueConversationList(List<RawConversation> rawlist)
        {
            List<RawConversation> uniqueList = new List<RawConversation>();

            string phoneNumber = "";
            RawConversation tempMessage = new RawConversation();

            foreach(RawConversation message in rawlist)
            {
                //V.phone_number, vm, rm, U.location, U.nationality, U.age_group, U.gender
                if(phoneNumber != message.PhoneNumber)
                {
                    if(phoneNumber != "")
                    {
                        uniqueList.Add(tempMessage);
                    }

                    tempMessage = new RawConversation();
                    phoneNumber = message.PhoneNumber;
                    tempMessage.PhoneNumber = message.PhoneNumber;
                    tempMessage.VolunteerMessage = message.VolunteerMessage;
                    tempMessage.ReporterMessage = message.ReporterMessage;
                    tempMessage.Location = message.Location;
                    tempMessage.Nationality = message.Nationality;
                    tempMessage.AgeGroup = message.AgeGroup;
                    tempMessage.Gender = message.Gender;
                    tempMessage.Exchanges = 1;
                }
                else
                {
                    tempMessage.VolunteerMessage += message.VolunteerMessage;
                    tempMessage.ReporterMessage += message.ReporterMessage;
                    tempMessage.Exchanges++;
                }
            }
            uniqueList.Add(tempMessage);

            return uniqueList;
        }

        private static List<ProcessedConversation> GetProcessedConversationList(List<RawConversation> uniqueList)
        {
            List<ProcessedConversation> processedConversationList = new List<ProcessedConversation>();

            foreach(RawConversation rawMessage in uniqueList)
            {
                ProcessedConversation processedConversation = GetSentiment(rawMessage);
                processedConversationList.Add(processedConversation);
            }

            return processedConversationList;
        }

        private static ProcessedConversation GetSentiment(RawConversation rawConversation)
        {
            ProcessedConversation processedConversation = new ProcessedConversation();
            processedConversation.PhoneNumber = rawConversation.PhoneNumber;
            processedConversation.Location = rawConversation.Location;
            processedConversation.Nationality = rawConversation.Nationality;
            processedConversation.Gender = rawConversation.Gender;
            processedConversation.AgeGroup = rawConversation.AgeGroup;
            processedConversation.Exchanges = rawConversation.Exchanges;
            Random random = new Random();
            processedConversation.Category = random.Next(0, 8);

            string IODApiKey = "Put your API Key here.";

            try
            {
                IODJobBatchRequest req = new IODJobBatchRequest();
                req.ApiKey = IODApiKey;

                SentimentRequest srVM = new SentimentRequest(IODSource.text, rawConversation.VolunteerMessage);
                req.AddRequest(srVM);
                srVM.OnResponse = (status, response) =>
                {
                    processedConversation.VolunteerMsgSenti = response.aggregate.score;
                };

                SentimentRequest srRM = new SentimentRequest(IODSource.text, rawConversation.ReporterMessage);
                req.AddRequest(srRM);
                srRM.OnResponse = (status, response) =>
                {
                    processedConversation.ReporterMsgSenti = response.aggregate.score;
                };

                SentimentRequest srOM = new SentimentRequest(IODSource.text, rawConversation.VolunteerMessage + rawConversation.ReporterMessage);
                req.AddRequest(srOM);
                srOM.OnResponse = (status, response) =>
                {
                    processedConversation.OverallMsgSenti = response.aggregate.score;
                };

                Console.WriteLine("Making request ...");
                req.MakeRequest();

            }
            catch (Exception e)
            {
                Console.WriteLine(string.Format("\r\nException: {0}", e.Message));
            }

            return processedConversation;
        }

        private static void SaveToDataStore(List<ProcessedConversation> processedConversationList)
        {
            NpgsqlConnection conn = new NpgsqlConnection("Server=52.74.179.57;Port=5432;UserId=postgres;Password=1234;Database=smsdb;");

            conn.Open();
            try
            {
                foreach(ProcessedConversation conversation in processedConversationList)
                {
//                    "reporter_sentiment"   "volunteer_sentiment"  "overall_sentiment"    "reporter_gender"      "reporter_country"    
//"reporter_nationality" "reporter_age_group"   "category"             "days"                 "exchanges"

                    NpgsqlCommand cmd = new NpgsqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandText = @"
                                INSERT INTO processed_conversation 
                                (
                                    reporter_sentiment, 
                                    volunteer_sentiment, 
                                    overall_sentiment,
                                    reporter_gender,
                                    reporter_country,
                                    reporter_nationality,
                                    reporter_age_group,
                                    category,
                                    days,
                                    exchanges
                                ) 
                                values
                                (
                                    @reporter_sentiment, 
                                    @volunteer_sentiment, 
                                    @overall_sentiment,
                                    @reporter_gender,
                                    @reporter_country,
                                    @reporter_nationality,
                                    @reporter_age_group,
                                    @category,
                                    @days,
                                    @exchanges
                                )
                    ";

                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add(new NpgsqlParameter("@reporter_sentiment", conversation.ReporterMsgSenti));
                    cmd.Parameters.Add(new NpgsqlParameter("@volunteer_sentiment", conversation.VolunteerMsgSenti));
                    cmd.Parameters.Add(new NpgsqlParameter("@overall_sentiment", conversation.OverallMsgSenti));
                    cmd.Parameters.Add(new NpgsqlParameter("@reporter_gender", conversation.Gender));
                    cmd.Parameters.Add(new NpgsqlParameter("@reporter_country", conversation.Location));
                    cmd.Parameters.Add(new NpgsqlParameter("@reporter_nationality", conversation.Nationality));
                    cmd.Parameters.Add(new NpgsqlParameter("@reporter_age_group", conversation.AgeGroup));
                    NpgsqlParameter category = new NpgsqlParameter("@category", DbType.Int32);
                    category.Value = conversation.Category;
                    cmd.Parameters.Add(category);
                    NpgsqlParameter days = new NpgsqlParameter("@days", DbType.Int32);
                    days.Value = 0;
                    cmd.Parameters.Add(days);
                    NpgsqlParameter exchanges = new NpgsqlParameter("@exchanges", DbType.Int32);
                    exchanges.Value = conversation.Exchanges;
                    cmd.Parameters.Add(exchanges);

                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
            }

            finally
            {
                conn.Close();
            }
        }
	}

}
