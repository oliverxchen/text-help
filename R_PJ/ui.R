library(ggplot2)
library(shiny)

dataset <- viewData

fluidPage(

  titlePanel("Text-Help"),
  
  sidebarPanel(

    selectInput('x', 'X-axis', names(dataset), names(dataset)[[6]]),
    selectInput('y', 'Y-axis', names(dataset))

  ),
  
  mainPanel(
      tabsetPanel(
        tabPanel('Plot',
                 plotOutput('plot')),
#        tabPanel('Histogram',
#                 plotOutput('hist')),
        tabPanel('List',
                 dataTableOutput("table"))
      )
  )
)
