#-------------------------------------------------------------------------------
# Purpose: Calls the other R functions used.
#-------------------------------------------------------------------------------

  library(RPostgreSQL)
  library(shinyapps)

  # read data
  viewData = read_extract()  
