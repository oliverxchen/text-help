#-------------------------------------------------------------------------------
# Purpose: Output on Shiny Dashboard
#-------------------------------------------------------------------------------

shiny_dashboard <- function(viewData) 
{
  
  ui <- dashboardPage(
    dashboardHeader(title = "UNited We Hack"),
    dashboardSidebar(),
    dashboardBody(
      # Boxes need to be put in a row (or column)
      fluidRow(
        box(
          title = "Heat map", status = "primary", solidHeader = TRUE,
          collapsible = TRUE,
        )
      )
    )
  )
  
  server <- function(input, output) { 
    return heat_map (viewData, "ageGroup", "nationality", "numExchanges", "Age", "Nationality", "Number of Exchanges", "", 0, 0, "steelblue")
  }
  
  shinyApp(ui, server)  
}