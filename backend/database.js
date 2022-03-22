const mysql = require('mysql');

let connection = null;

exports.setupDatabaseConnection = function(host, username, password, database_name) {
	connection = mysql.createConnection({
		host: host,
		user: username,
		password: password,
		database: database_name
	});
};

exports.startDatabaseConnection = function() {
	connection.connect(function(err) {
		if(err){
			console.log("A fatal error when trying to start the database connection.");
			console.log(err.code);
			console.log(err.fatal);
		} else {
			console.log("MySQL connection has been succesful");
		}
	});
}

// Posible return codes:
//  1 = Succesful.
//  0 = MySQL error
// -1 = Invalid string.
// -2 = Mod id is already used.
exports.addNewMod = function(modId, modName, version, downloadLink){
	if(isEmpty(modId) || isEmpty(modName) || isEmpty(version) || isEmpty(downloadLink))
		return -1;
	
	// We first check if the mod does not exist.
	connection.query("SELECT COUNT(mods.internal_id) FROM mods WHERE mods.mod_id = ?", [modId], (err, result) => {
		if(err){
			console.log("An error has ocurred while trying to check for internal_id duplicates.");
			console.log(err);
		} else {
			result = JSON.parse(JSON.stringify(result));

			if(result["COUNT(mods.internal_id)"] != 0){
				return -2; // A copy exists, return -2.
			}
		}
	});

	//TODO: URL check.
	
	// We now try to add the mod.

	return 1;
}


function isEmpty(value) {
	return typeof value == 'string' && !value.trim() || typeof value == 'undefined' || value === null;
}