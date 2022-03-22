// Requires
const db = require('./database');
const dotenv = require("dotenv");
dotenv.config("../.");

db.setupDatabaseConnection(process.env.MYSQL_HOST, process.env.MYSQL_USERNAME, process.env.MYSQL_PASSWORD, process.env.MYSQL_DB);

function isEmpty(value) {
	return typeof value == 'string' && !value.trim() || typeof value == 'undefined' || value === null;
}