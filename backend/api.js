// Requires
const db = require('./database');
const express = require('express');

// Basic setup
const app = express();
app.use(express.json());
app.listen(3000);

db.setupDatabaseConnection(process.env.MYSQL_HOST, process.env.MYSQL_USERNAME, process.env.MYSQL_PASSWORD, process.env.MYSQL_DB);

app.post('/mods', function(request, response){
    console.log(request.body);
    response.send(request.body);
});

app.get("/mods", function(request, response){
    response.send("It works!");
});

function isEmpty(value) {
	return typeof value == 'string' && !value.trim() || typeof value == 'undefined' || value === null;
}