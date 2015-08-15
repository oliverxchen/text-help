var app = require('express')();
var http = require('http').Server(app);
var io = require('socket.io')(http);

var pg = require('pg');
var conString = "postgres://postgres:1234@localhost/smsdb";

var bodyParser = require('body-parser')
app.use(bodyParser.json());
app.use(bodyParser.urlencoded({
    extended: true
}));

app.get('/', function(req, res) {
    res.sendFile(__dirname + '/index.html');
});

//app.get('/new', function(req, res) {
//    var client = new pg.Client(conString);
//    client.connect(function(err) {
//        if(err) {
//            return console.error('could not connect to postgres', err);
//        }
//        client.query('SELECT * from raw_conversation order by timestamp_local desc limit 1', function(err, result) {
//            if(err) {
//                return console.error('error running query 2', err);
//            }
//            return result.rows[0].sms_content;
//
//            client.end();
//        });
//    });
//    res.send('finished');
//});

app.get('/new', function (res, res) {
  var client = new pg.Client(conString);
  client.connect(function(err){
      if (err){
          return console.error('could not connect to postgres',err);
      }
      client.query('select phone_number from raw_conversation where is_sent = false order by timestamp_local desc',function(err,result){
          if (err){
              return console.error('error running query',err);
          }
          res.writeHead(200,{"Content-Type":'application/json'});
          res.end(JSON.stringify({"payload":{"task": "send","secret": "secret_key","messages":[{"to": '+' + result.rows[0].phone_number.toString(),"message": "the message goes here","uuid": "aada21b0-0615-4957-bcb3"}]}}));
    });
  });
});

var query_insert;
var message;

app.post('/new', function(req, res) {
    message = req.body.message;
    query_insert = req.body.from + ',' + 'to_timestamp(' + req.body.sent_timestamp/1000 + '),\'' + req.body.message + '\'';  
    //console.log(query_insert);

    io.emit('chat message', req.body.message);


    var client = new pg.Client(conString);
    client.connect(function(err, req) {
        if(err) {
            return console.error('could not connect to postgres', err);
        }
        query_insert = 'INSERT INTO raw_conversation SELECT (\'Victim1\',\'Volunteer1\',' + query_insert + ', false) WHERE \'' + message + '\' NOT IN (SELECT sms_content from raw_conversation)';
        //console.log(query_insert);
        client.query(query_insert, function(err, result) {
            if(err) {
                return console.error('error running insert query', err);
            }
            res.writeHead(200,{"Content-Type":'application/json'});
            res.end("success");
            client.end();
        });
    });
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
