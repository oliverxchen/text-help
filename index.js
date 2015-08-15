var app = require('express')();
var http = require('http').Server(app);
var io = require('socket.io')(http);

app.get('/', function(req, res) {
    res.sendFile(__dirname + '/index.html');
});

io.on('connection', function(socket) {
    socket.on('chat message', function(msg) {
        io.emit('chat message', msg);
    });
});

io.emit('some event', {for: 'everyone'});

http.listen(3000, function() {
    console.log('listening on http://52.74.179.57:3000');
});
