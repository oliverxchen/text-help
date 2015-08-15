#-------------------------------------------------------------------------------
# Purpose: Output on Shiny Dashboard
#-------------------------------------------------------------------------------

dashboard_body <- function(viewData) 
{
  dashboard_body <- dashboardBody(
    heat_map (viewData, "ageGroup", "nationality", "numExchanges", "Age", "Nationality", "Number of Exchanges", "", 0, 0, "steelblue") 
  )
  return (dashboard_body)
}