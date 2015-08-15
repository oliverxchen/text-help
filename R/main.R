#-------------------------------------------------------------------------------
# Purpose: Calls the other R functions used.
#-------------------------------------------------------------------------------
  library(ggplot2)
  library(plotly)
  library(shinydashboard)

  viewData = read_extract(input_file)  
  
  # generate heatmap value for x-y pair
  shiny_dashboard(viewData)
 
