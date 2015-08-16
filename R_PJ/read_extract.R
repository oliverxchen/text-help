read_extract <- function() 
{  
  # To read data from PSQL
  con <- dbConnect(PostgreSQL(), user= "user", password="password", host="52.74.179.57",port=5432, dbname="smsdb")
  viewData <- dbGetQuery(con,"select * from processed_conversation")
  dbDisconnect(con)
  return(viewData)
}