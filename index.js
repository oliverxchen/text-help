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
      client.query('select request_id, phone_number, sms_content from raw_conversation where is_sent = false order by timestamp_local desc',function(err,result){
          if (err){
              return console.error('error running query',err);
          }
          res.writeHead(200,{"Content-Type":'application/json'});

          if (result.rows[0] !== undefined) {
              var output = JSON.stringify({"payload":{"task": "send","secret": "secret_key","messages":[{"to": result.rows[0].phone_number.toString(),"message": result.rows[0].sms_content, "uuid": "aada21b0-0615-4957-bcb3"}]}});
              console.log(output);
              res.end(output);
              
              client.query('update raw_conversation set is_sent = true where request_id = ' + result.rows[0].request_id, function(err,result) {
              });
          }
          else {
              res.end('');
          }
      });
  });
});

var query_insert;
var message;

app.post('/new', function(req, res) {
    message = req.body.message;
    query_insert = '\'' + req.body.from + '\',' + 'to_timestamp(' + req.body.sent_timestamp/1000 + '),\'' + req.body.message + '\'';  
    //console.log(query_insert);

    console.log(message);
    var client = new pg.Client(conString);
    client.connect(function(err, req) {
        if(err) {
            return console.error('could not connect to postgres', err);
        }

        var query_check = 'SELECT COUNT(*) FROM raw_conversation WHERE is_sent = true and sms_content = \'' + message + '\'';

        client.query(query_check, function(err, result) {
            if(err) {
                return console.error('error running query check', err);
            }

            console.log(result.rows[0].count);
            if (result.rows[0].count < 2) {
                console.log('in the if');
                io.emit('chat message', message);
            }
            client.end();
        });
    });


    var client = new pg.Client(conString);
    client.connect(function(err, req) {
        if(err) {
            return console.error('could not connect to postgres', err);
        }
        query_complete = 'INSERT INTO raw_conversation(\"from\", \"to\", phone_number, timestamp_local, sms_content, is_sent) SELECT \'Victim1\',\'Volunteer1\',' + query_insert + ', true WHERE \'' + message + '\' NOT IN (SELECT sms_content from raw_conversation)';
        console.log(query_complete);
        client.query(query_complete, function(err, result) {
            if(err) {
                return console.error('error 1 running insert query', err);
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
        var client = new pg.Client(conString);
        client.connect(function(err, req) {
            if(err) {
                return console.error('could not connect to postgres', err);
            }
            query_insert = 'INSERT INTO raw_conversation VALUES (\'Volunteer1\',\'Victim1\', \'+6596837238\', NOW(),\'' + msg + '\', false)'; 
            console.log(query_insert);

            client.query(query_insert, function(err, result) {
                if(err) {
                    return console.error('error running insert query', err);
                }
                client.end();
            });
        });
    });
});

io.emit('some event', {for: 'everyone'});

http.listen(3000, function() {
    console.log('listening on http://52.74.179.57:3000');
});
