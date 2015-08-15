read_extract <- function(input_file) 
{  
  # To read data from csv
  viewData <- read.csv(input_file, header=FALSE) 
  
  # To read data from PSQL
#  drv <- dbDriver("PostgreSQL")
#  con <- dbConnect(drv, dbname="smsdb")
  con <- dbConnect(PostgreSQL(), user= "postgres", password="1234", host="52.74.179.57",port=5432, dbname="smsdb")
  viewData <- dbGetQuery(con,"select * from ?")
  dbDisconnect(con)
  
  # name the fields
  colnames(viewData)[1]<- "reporterScore"
  colnames(viewData)[2]<- "volunteerScore"
  colnames(viewData)[3]<- "conversationScore"
  colnames(viewData)[4]<- "gender"
  colnames(viewData)[5]<- "country"
  colnames(viewData)[6]<- "nationality"
  colnames(viewData)[7]<- "ageGroup"
  colnames(viewData)[8]<- "convCategory"
  colnames(viewData)[9]<- "elapsedDays"
  colnames(viewData)[10]<- "numExchanges"
  # viewData = unique(viewData) 
  
  return(viewData)
}