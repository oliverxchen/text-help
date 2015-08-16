library(ggplot2)
library(shiny)

function(input, output) {
  
  dataset <- reactive({
    viewData[sample(nrow(viewData), nrow(viewData)),]
  })
  output$table <- renderDataTable({
    viewData
  })
  
  #, options = list(lengthMenu = c(5, 30, 50), pageLength = 5))
  
  output$plot <- renderPlot({
    p <- ggplot(dataset(), aes_string(x=input$x, y=input$y)) + 
         theme(axis.text.y = element_text(size=20)) +
         theme(axis.text.x = element_text(size=20)) +
         theme(axis.title.y = element_text(size=20)) +
         theme(axis.title.x = element_text(size=20)) +
         theme(legend.title = element_text(size=15)) +
         theme(legend.text = element_text(size=15)) +
         geom_point(size=10, aes(colour=factor(round(reporter_sentiment,2))))
    p <- p + theme(legend.title=element_blank())
   print(p)
    
  }, height=700)
  
}