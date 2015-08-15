#-------------------------------------------------------------------------------
# Purpose: Generate Heat map
# Input data frame viewData is structured as follows
#   Column 1: x axis
#   Column 2: y axis
#   Column 3: value for x-y pair
#-------------------------------------------------------------------------------

heat_map <- function(funData, fun.x, fun.y, fun.val, x_title, y_title, legend_title, map_title, x_fontsize, y_fontsize, fun.col) 
{
  funData$fun.x = funData[,fun.x]
  funData$fun.y = funData[,fun.y]
  funData$fun.val = funData[,fun.val]
  
  h_map = 
    ggplot(funData, aes(fun.x, fun.y, group=fun.x)) +
    xlab(x_title) +
    ylab(y_title) + 
    geom_tile(aes(fill = fun.val)) + 
    ggtitle(map_title) +
    theme(panel.background = element_blank()) +
#    theme(axis.text.x = element_text(angle = 90, vjust = 0.5, hjust = 1)) +
    scale_x_discrete(labels = c("1"="10","2"="20","3"="30","4"="40","5"="50","6"="> 60")) +
    scale_fill_gradient(name=legend_title, low = "white", high = fun.col)

  if (x_fontsize != 0) { h_map = h_map + theme(axis.text.x = element_text(size = x_fontsize)) }
  if (y_fontsize != 0) { h_map = h_map + theme(axis.text.y = element_text(size = y_fontsize)) }
    
  print(h_map)
  
}