// Requires
const db = require('./database');
const dotenv = require("dotenv");
dotenv.config("../.");

db.setupDatabaseConnection(process.env.MYSQL_HOST, process.env.MYSQL_USERNAME, process.env.MYSQL_PASSWORD, process.env.MYSQL_DB);

// TO DO: Some sort of cache.
// This might be implemented later.

/*function isEmpty(value) {
	return typeof value == 'string' && !value.trim() || typeof value == 'undefined' || value === null;
}*/